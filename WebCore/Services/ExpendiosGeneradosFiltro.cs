// Filtros de Punto de Expendio > Expendios generados, en un solo lugar para que el listado JSON y los exports
// PDF/Excel muestren exactamente lo mismo. Logica pura (sin HTTP ni base): tests en
// WebCore.Tests/ExpendiosGeneradosFiltroTests.cs. La pantalla repite los filtros de estado/sucursal/sector/usuario/
// texto/producto en JavaScript (filtrado "en vivo" sobre lo ya cargado): si se cambia una regla, cambiar las dos.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebCore.Models;

namespace WebCore.Services
{
    // Filtros "en vivo" de la pantalla. Cadena vacia = sin filtro.
    public class ExpendiosGeneradosFiltros
    {
        public string Estado { get; set; } = "";
        public string Sucursal { get; set; } = "";
        public string Sector { get; set; } = "";
        public string Usuario { get; set; } = "";
        // Cliente / identificacion o nro de remito (coincidencia parcial).
        public string Texto { get; set; } = "";
        // Codigo o descripcion del producto (coincidencia parcial). Filtra LINEAS, no solo expendios.
        public string Producto { get; set; } = "";
    }

    public static class ExpendiosGeneradosFiltro
    {
        // Minusculas, espacios colapsados y sin bordes: igual que normalizeText de la pantalla.
        public static string Normalizar(string? texto)
        {
            return Regex.Replace((texto ?? "").ToLowerInvariant(), @"\s+", " ").Trim();
        }

        // Identificacion para comparar personas: sin acentos, en mayusculas y sin espacios en los bordes
        // (mismo criterio que upper(unaccent(btrim())) de DatosPostgres.ExpendioClientePg).
        public static string NormalizarIdentificacion(string? texto)
        {
            var descompuesto = (texto ?? "").Trim().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(descompuesto.Length);
            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC).ToUpperInvariant();
        }

        // Un expendio es de la persona si tiene guardado su id (idpersona) O si su "Identificacion cliente" es igual
        // a la identificacion de la persona. Es la misma regla que DatosPostgres.ExpendioClientePg (filtro Persona
        // del POS, 2026-10-29); se repite aca para funcionar tambien en SQL Server, donde el expendio no guarda
        // idpersona. Una identificacion vacia nunca coincide.
        public static bool CoincidePersona(int? idPersonaExpendio, string? identificacionExpendio, ISet<int> idsPersona, ISet<string> identificacionesNormalizadas)
        {
            if (idPersonaExpendio.HasValue && idsPersona.Contains(idPersonaExpendio.Value))
                return true;

            string identificacion = NormalizarIdentificacion(identificacionExpendio);
            return identificacion.Length > 0 && identificacionesNormalizadas.Contains(identificacion);
        }

        // "Filtros: Estado: Pendiente  ·  Cliente: COCINA X  ·  Producto: cerdo" para el encabezado de los exports.
        // "" si no hay ninguno activo. La sucursal no va aca: ya figura en la linea del periodo. Con personas elegidas
        // (nombresPersonas) se listan sus nombres; sin ellas, el texto escrito.
        public static string TextoFiltros(ExpendiosGeneradosFiltros filtros, IEnumerable<string> nombresPersonas)
        {
            var partes = new List<string>();
            if (!string.IsNullOrWhiteSpace(filtros.Estado)) partes.Add("Estado: " + filtros.Estado.Trim());
            if (!string.IsNullOrWhiteSpace(filtros.Sector)) partes.Add("Sector: " + filtros.Sector.Trim());
            if (!string.IsNullOrWhiteSpace(filtros.Usuario)) partes.Add("Usuario: " + filtros.Usuario.Trim());

            string personas = string.Join(", ", nombresPersonas.Where(x => !string.IsNullOrWhiteSpace(x)));
            if (personas.Length > 0) partes.Add("Cliente: " + personas);
            else if (!string.IsNullOrWhiteSpace(filtros.Texto)) partes.Add("Cliente / remito: " + filtros.Texto.Trim());

            if (!string.IsNullOrWhiteSpace(filtros.Producto)) partes.Add("Producto: " + filtros.Producto.Trim());

            return partes.Count == 0 ? "" : "Filtros: " + string.Join("  ·  ", partes);
        }

        // Aplica los filtros en vivo. Con Producto, cada expendio conserva SOLO las lineas que coinciden (por codigo
        // o descripcion), sus kilos e importe pasan a ser la suma de esas lineas, y un expendio sin lineas sale.
        // Devuelve copias cuando hay que recortar lineas: los items originales no se modifican.
        public static List<ExpendioGeneradoVm> Aplicar(IEnumerable<ExpendioGeneradoVm> items, ExpendiosGeneradosFiltros filtros)
        {
            string estado = Normalizar(filtros.Estado);
            string sucursal = Normalizar(filtros.Sucursal);
            string sector = Normalizar(filtros.Sector);
            string usuario = Normalizar(filtros.Usuario);
            string texto = Normalizar(filtros.Texto);
            string producto = Normalizar(filtros.Producto);

            var resultado = new List<ExpendioGeneradoVm>();
            foreach (var item in items)
            {
                if (texto.Length > 0
                    && !Normalizar(item.IdentificacionExpendio).Contains(texto)
                    && !Normalizar(item.NroRemito).Contains(texto)) continue;
                if (sucursal.Length > 0 && Normalizar(item.Sucursal) != sucursal) continue;
                if (estado.Length > 0 && Normalizar(item.Estado) != estado) continue;
                if (sector.Length > 0 && Normalizar(item.Sector) != sector) continue;
                if (usuario.Length > 0 && Normalizar(item.Usuario) != usuario) continue;

                if (producto.Length == 0)
                {
                    resultado.Add(item);
                    continue;
                }

                var lineas = item.Lineas
                    .Where(l => Normalizar(l.Codigo.ToString(CultureInfo.InvariantCulture)).Contains(producto)
                                || Normalizar(l.Producto).Contains(producto))
                    .ToList();
                if (lineas.Count == 0) continue;

                resultado.Add(new ExpendioGeneradoVm
                {
                    IdExpendio = item.IdExpendio,
                    FechaExpendio = item.FechaExpendio,
                    Fecha = item.Fecha,
                    Hora = item.Hora,
                    IdentificacionExpendio = item.IdentificacionExpendio,
                    NroRemito = item.NroRemito,
                    Sucursal = item.Sucursal,
                    Sector = item.Sector,
                    Usuario = item.Usuario,
                    CantItems = lineas.Count.ToString(CultureInfo.InvariantCulture),
                    TotalKg = lineas.Sum(l => l.CantKg),
                    TotalImporte = lineas.Sum(l => l.Total),
                    IdVenta = item.IdVenta,
                    Estado = item.Estado,
                    PdfUrl = item.PdfUrl,
                    EmailUrl = item.EmailUrl,
                    IdPersona = item.IdPersona,
                    Lineas = lineas
                });
            }

            return resultado;
        }
    }
}
