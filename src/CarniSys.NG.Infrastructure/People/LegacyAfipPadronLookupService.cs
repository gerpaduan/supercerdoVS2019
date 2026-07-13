using System.Net;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using CarniSys.NG.Application.People;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using System.Data.SqlClient;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyAfipPadronLookupService(
    ILegacyConnectionStringResolver connectionStringResolver,
    IWebHostEnvironment webHostEnvironment) : IAfipPadronLookupService
{
    private const string PadronServiceName = "ws_sr_padron_a13";

    public async Task<AfipPadronLookupResult> LookupAsync(
        int companyId,
        string taxId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedTaxId = NormalizeTaxId(taxId);
            if (normalizedTaxId.Length != 11 || !long.TryParse(normalizedTaxId, out var personTaxId))
            {
                return Failure("Ingrese un CUIT valido de 11 digitos.", "warning");
            }

            var company = await GetCompanyAsync(companyId, cancellationToken);
            if (company is null || company.TaxId <= 0)
            {
                return Failure("No se encontro la configuracion AFIP de la empresa actual.");
            }

            var environmentIsProduction = !string.IsNullOrWhiteSpace(company.EnvironmentMode)
                && company.EnvironmentMode.Trim().ToUpperInvariant().Contains("PROD");

            var loginUrl = environmentIsProduction
                ? "https://wsaa.afip.gov.ar/ws/services/LoginCms"
                : "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";

            var padronUrl = environmentIsProduction
                ? "https://aws.afip.gov.ar/sr-padron/webservices/personaServiceA13"
                : "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA13";

            var basePath = Path.Combine(webHostEnvironment.ContentRootPath, "AFIP", company.TaxId.ToString());
            var certificateName = string.IsNullOrWhiteSpace(company.CertificateFileName)
                ? "certif-prod.pfx"
                : company.CertificateFileName.Trim();
            var certificatePath = Path.Combine(basePath, certificateName);
            var ticketPath = Path.Combine(basePath, "TicketAccesoPerson.txt");
            var templatePath = Path.Combine(basePath, "LoginTemplate.xml");

            if (!File.Exists(certificatePath))
            {
                throw new FileNotFoundException("No se encontro el certificado AFIP configurado para la empresa.", certificatePath);
            }

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("No se encontro el template de login AFIP configurado para la empresa.", templatePath);
            }

            var certificate = X509CertificateLoader.LoadPkcs12FromFile(
                certificatePath,
                string.Empty,
                X509KeyStorageFlags.PersistKeySet);

            var loginTicket = await GetLoginTicketAsync(
                certificate,
                loginUrl,
                ticketPath,
                templatePath,
                cancellationToken);

            var response = await GetPersonaAsync(
                padronUrl,
                certificate,
                loginTicket.Token,
                loginTicket.Sign,
                company.TaxId,
                personTaxId,
                cancellationToken);

            if (response is null)
            {
                return Failure("No se encontraron datos para el CUIT ingresado.", "info");
            }

            var businessName = string.Equals(response.PersonType, "FISICA", StringComparison.OrdinalIgnoreCase)
                ? (response.LastName + " " + response.FirstName).Trim()
                : response.BusinessName;
            var address = response.Address;
            var city = string.IsNullOrWhiteSpace(response.Locality) && string.IsNullOrWhiteSpace(response.Province)
                ? string.Empty
                : (response.Locality + ", " + response.Province).Trim().Trim(',');

            if (string.IsNullOrWhiteSpace(businessName) &&
                string.IsNullOrWhiteSpace(address) &&
                string.IsNullOrWhiteSpace(city) &&
                string.IsNullOrWhiteSpace(response.TaxStatus) &&
                string.IsNullOrWhiteSpace(response.MainActivity))
            {
                return Failure("No se encontraron datos para el CUIT ingresado.", "info");
            }

            return new AfipPadronLookupResult
            {
                Success = true,
                Message = "Datos recuperados desde AFIP/ARCA.",
                Identification = businessName,
                BusinessName = businessName,
                Address = address,
                City = city,
                SuggestedVatId = 0,
                VatCondition = string.Empty,
                TaxStatus = response.TaxStatus,
                MainActivity = response.MainActivity,
                MessageType = "success"
            };
        }
        catch (Exception ex)
        {
            return Failure(TranslateError(ex));
        }
    }

    private async Task<CompanyAfipConfig?> GetCompanyAsync(int companyId, CancellationToken cancellationToken)
    {
        if (companyId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT TOP 1
                idEmpresa,
                cuit,
                nombreCertificado_pfx,
                Entorno_HOMO_PROD
            FROM dbo.Empresas
            WHERE idEmpresa = @idEmpresa;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new CompanyAfipConfig(
            reader["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(reader["idEmpresa"]),
            reader["cuit"] == DBNull.Value ? 0L : Convert.ToInt64(reader["cuit"]),
            Convert.ToString(reader["nombreCertificado_pfx"]) ?? string.Empty,
            Convert.ToString(reader["Entorno_HOMO_PROD"]) ?? string.Empty);
    }

    private static async Task<LoginTicket> GetLoginTicketAsync(
        X509Certificate2 certificate,
        string loginUrl,
        string ticketPath,
        string templatePath,
        CancellationToken cancellationToken)
    {
        if (File.Exists(ticketPath))
        {
            var cachedTicket = await File.ReadAllTextAsync(ticketPath, cancellationToken);
            if (TryParseActiveTicket(cachedTicket, out var activeTicket))
            {
                return activeTicket;
            }
        }

        var requestDocument = XDocument.Load(templatePath);
        var uniqueIdNode = requestDocument.Descendants().First(x => x.Name.LocalName == "uniqueId");
        var generationTimeNode = requestDocument.Descendants().First(x => x.Name.LocalName == "generationTime");
        var expirationTimeNode = requestDocument.Descendants().First(x => x.Name.LocalName == "expirationTime");
        var serviceNode = requestDocument.Descendants().First(x => x.Name.LocalName == "service");

        uniqueIdNode.Value = DateTime.UtcNow.Ticks.ToString();
        generationTimeNode.Value = DateTime.Now.AddMinutes(-10).ToString("s");
        expirationTimeNode.Value = DateTime.Now.AddMinutes(10).ToString("s");
        serviceNode.Value = PadronServiceName;

        var cms = SignCms(requestDocument.ToString(SaveOptions.DisableFormatting), certificate);
        var soapEnvelope = BuildLoginEnvelope(cms);
        var soapResponse = await PostSoapAsync(loginUrl, soapEnvelope, certificate: null, cancellationToken);
        var loginTicketResponse = GetSoapBodyValue(soapResponse, "loginCmsReturn");

        Directory.CreateDirectory(Path.GetDirectoryName(ticketPath)!);
        await File.WriteAllTextAsync(ticketPath, loginTicketResponse, cancellationToken);

        if (!TryParseTicket(loginTicketResponse, out var parsedTicket))
        {
            throw new InvalidOperationException("AFIP/ARCA devolvio un ticket de acceso invalido.");
        }

        return parsedTicket;
    }

    private static async Task<PersonaResponse?> GetPersonaAsync(
        string padronUrl,
        X509Certificate2 certificate,
        string token,
        string sign,
        long representedCompanyTaxId,
        long personTaxId,
        CancellationToken cancellationToken)
    {
        var soapEnvelope = BuildGetPersonaEnvelope(token, sign, representedCompanyTaxId, personTaxId);
        var soapResponse = await PostSoapAsync(padronUrl, soapEnvelope, certificate, cancellationToken);

        var personaReturn = GetSoapBodyElement(soapResponse, "personaReturn");
        if (personaReturn is null)
        {
            return null;
        }

        var persona = personaReturn.Descendants().FirstOrDefault(x => x.Name.LocalName == "persona");
        if (persona is null)
        {
            return null;
        }

        var domicilio = persona.Descendants().FirstOrDefault(x => x.Name.LocalName == "domicilio");
        return new PersonaResponse(
            GetElementValue(persona, "tipoPersona"),
            GetElementValue(persona, "apellido"),
            GetElementValue(persona, "nombre"),
            GetElementValue(persona, "razonSocial"),
            GetElementValue(persona, "estadoClave"),
            GetElementValue(persona, "descripcionActividadPrincipal"),
            domicilio is null ? string.Empty : GetElementValue(domicilio, "direccion"),
            domicilio is null ? string.Empty : GetElementValue(domicilio, "localidad"),
            domicilio is null ? string.Empty : GetElementValue(domicilio, "descripcionProvincia"));
    }

    private static string SignCms(string xml, X509Certificate2 certificate)
    {
        var contentInfo = new ContentInfo(Encoding.UTF8.GetBytes(xml));
        var signedCms = new SignedCms(contentInfo);
        var signer = new CmsSigner(certificate)
        {
            IncludeOption = X509IncludeOption.EndCertOnly
        };

        signedCms.ComputeSignature(signer);
        return Convert.ToBase64String(signedCms.Encode());
    }

    private static async Task<string> PostSoapAsync(
        string url,
        string envelope,
        X509Certificate2? certificate,
        CancellationToken cancellationToken)
    {
        using var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        };

        if (certificate is not null)
        {
            handler.ClientCertificates.Add(certificate);
        }

        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
        request.Headers.TryAddWithoutValidation("SOAPAction", "\"\"");
        request.Content = new StringContent(envelope, Encoding.UTF8, "text/xml");

        using var response = await client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(GetSoapFaultOrBody(body));
        }

        var fault = TryGetSoapFault(body);
        if (!string.IsNullOrWhiteSpace(fault))
        {
            throw new InvalidOperationException(fault);
        }

        return body;
    }

    private static string BuildLoginEnvelope(string cms)
    {
        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:wsaa="http://wsaa.view.sua.dvadac.desein.afip.gov">
              <soapenv:Body>
                <wsaa:loginCms>
                  <wsaa:in0>{SecurityElement.Escape(cms)}</wsaa:in0>
                </wsaa:loginCms>
              </soapenv:Body>
            </soapenv:Envelope>
            """;
    }

    private static string BuildGetPersonaEnvelope(string token, string sign, long representedCompanyTaxId, long personTaxId)
    {
        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:a13="http://a13.soap.ws.server.puc.sr/">
              <soapenv:Body>
                <a13:getPersona>
                  <token>{SecurityElement.Escape(token)}</token>
                  <sign>{SecurityElement.Escape(sign)}</sign>
                  <cuitRepresentada>{representedCompanyTaxId}</cuitRepresentada>
                  <idPersona>{personTaxId}</idPersona>
                </a13:getPersona>
              </soapenv:Body>
            </soapenv:Envelope>
            """;
    }

    private static bool TryParseActiveTicket(string xml, out LoginTicket loginTicket)
    {
        if (TryParseTicket(xml, out loginTicket))
        {
            return loginTicket.ExpirationTime.AddHours(-2) > DateTime.Now;
        }

        return false;
    }

    private static bool TryParseTicket(string xml, out LoginTicket loginTicket)
    {
        loginTicket = default!;
        if (string.IsNullOrWhiteSpace(xml))
        {
            return false;
        }

        var document = XDocument.Parse(xml);
        var token = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "token")?.Value ?? string.Empty;
        var sign = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "sign")?.Value ?? string.Empty;
        var generationTimeRaw = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "generationTime")?.Value ?? string.Empty;
        var expirationTimeRaw = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "expirationTime")?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(sign) ||
            !DateTime.TryParse(generationTimeRaw, out var generationTime) ||
            !DateTime.TryParse(expirationTimeRaw, out var expirationTime))
        {
            return false;
        }

        loginTicket = new LoginTicket(token, sign, generationTime, expirationTime);
        return true;
    }

    private static string GetSoapBodyValue(string xml, string elementName)
    {
        var element = GetSoapBodyElement(xml, elementName);
        if (element is null)
        {
            throw new InvalidOperationException("AFIP/ARCA devolvio una respuesta invalida.");
        }

        return element.Value;
    }

    private static XElement? GetSoapBodyElement(string xml, string elementName)
    {
        var document = XDocument.Parse(xml);
        return document.Descendants().FirstOrDefault(x => x.Name.LocalName == elementName);
    }

    private static string GetElementValue(XContainer node, string elementName)
    {
        return node.Descendants().FirstOrDefault(x => x.Name.LocalName == elementName)?.Value ?? string.Empty;
    }

    private static string TryGetSoapFault(string xml)
    {
        try
        {
            return GetSoapFaultOrBody(xml);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string GetSoapFaultOrBody(string xml)
    {
        var document = XDocument.Parse(xml);
        var faultString = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "faultstring")?.Value;
        if (!string.IsNullOrWhiteSpace(faultString))
        {
            return faultString;
        }

        var detail = document.Descendants().FirstOrDefault(x => x.Name.LocalName == "detail")?.Value;
        return string.IsNullOrWhiteSpace(detail) ? xml : detail;
    }

    private static AfipPadronLookupResult Failure(string message, string type = "error")
    {
        return new AfipPadronLookupResult
        {
            Success = false,
            Message = message,
            MessageType = type
        };
    }

    private static string NormalizeTaxId(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().Replace("-", string.Empty).Replace(" ", string.Empty);
    }

    private static string TranslateError(Exception ex)
    {
        var detail = GetTechnicalDetail(ex);
        var text = ((ex.Message ?? string.Empty) + " " + (ex.InnerException?.Message ?? string.Empty)).ToLowerInvariant();

        if (ex is TimeoutException || text.Contains("timeout") || text.Contains("timed out"))
        {
            return "Timeout al consultar AFIP/ARCA. Revise la conexion e intente nuevamente. " + detail;
        }

        if (ex is WebException ||
            text.Contains("unable to connect") ||
            text.Contains("forcibly closed") ||
            text.Contains("connection") ||
            text.Contains("remote name could not be resolved"))
        {
            return "No se pudo conectar con AFIP/ARCA. Revise la conexion o el servicio. " + detail;
        }

        if (text.Contains("token") ||
            text.Contains("sign") ||
            text.Contains("cms") ||
            text.Contains("certificado") ||
            text.Contains("certificate") ||
            text.Contains("wsaa"))
        {
            return "Token o certificado AFIP invalido o vencido. " + detail;
        }

        if (text.Contains("no se encontro el certificado") ||
            text.Contains("template de login"))
        {
            return "La configuracion AFIP de la empresa esta incompleta. " + detail;
        }

        return "Error inesperado al consultar AFIP/ARCA. " + detail;
    }

    private static string GetTechnicalDetail(Exception ex)
    {
        var detail = ex.Message ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(ex.InnerException?.Message))
        {
            detail = string.IsNullOrWhiteSpace(detail)
                ? ex.InnerException.Message
                : detail + " | " + ex.InnerException.Message;
        }

        detail = detail.Trim();
        return string.IsNullOrWhiteSpace(detail) ? string.Empty : "Detalle: " + detail;
    }

    private sealed record CompanyAfipConfig(int CompanyId, long TaxId, string CertificateFileName, string EnvironmentMode);

    private sealed record LoginTicket(string Token, string Sign, DateTime GenerationTime, DateTime ExpirationTime);

    private sealed record PersonaResponse(
        string PersonType,
        string LastName,
        string FirstName,
        string BusinessName,
        string TaxStatus,
        string MainActivity,
        string Address,
        string Locality,
        string Province);
}
