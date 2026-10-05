// Manual de uso y ayuda por pantalla (2026-10-03, ver docs/DECISIONS.md "Manuales por rol y ayuda en
// cada pantalla"). Lee los .md de docs/11-manual-de-uso (embebidos en el .dll, ver WebCore.csproj),
// parsea su encabezado (front-matter) y los entrega ya renderizados a HTML. Una sola fuente de verdad:
// el mismo archivo alimenta el panel de ayuda de la pantalla, el manual de cada rol y la busqueda.
//
// Tres tipos de documento (carpeta = tipo):
//   pantallas/<Controller>.<Action>.md   ayuda de UNA pantalla (clave = controller/action de la ruta)
//   conceptos/<slug>.md                  explicaciones transversales (roles, dispositivo seguro...)
//   referencia/<slug>.md                 tablas de consulta (mapa de configuraciones)
// Cada uno declara en su encabezado a que rol pertenece (usuario | admin | superadmin): el manual de
// un rol lista los documentos de ese rol, y un usuario solo puede abrir los de su nivel o inferiores.
// La ayuda NO reemplaza el control de permisos de cada pantalla: solo explica.
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace WebCore.Helpers
{
    public enum NivelAyuda
    {
        Usuario = 0,
        Admin = 1,
        SuperAdmin = 2
    }

    public enum TipoDocAyuda
    {
        Pantalla,
        Concepto,
        Referencia
    }

    public sealed class DocAyuda
    {
        public string Recurso { get; init; } = "";
        public TipoDocAyuda Tipo { get; init; }
        // Pantalla: "Usuarios.Index". Concepto/Referencia: el slug (nombre del archivo sin .md).
        public string Clave { get; init; } = "";
        public string Titulo { get; init; } = "";
        public NivelAyuda Nivel { get; init; }
        // Agrupa en el indice del manual (ej. "Acceso y seguridad"). Vacio = "General".
        public string Modulo { get; init; } = "";
        public int Orden { get; init; }
        // Otras claves controller.action que muestran la misma pantalla (ej. Usuarios.Guardar renderiza
        // Usuarios/Editar tras un error de validacion). Solo para pantallas.
        public IReadOnlyList<string> Alias { get; init; } = Array.Empty<string>();
        public string Permiso { get; init; } = "";
        public string Revisada { get; init; } = "";
        public string Markdown { get; init; } = "";
    }

    public interface IAyudaService
    {
        DocAyuda? BuscarPantalla(string controller, string action, NivelAyuda nivelUsuario);
        DocAyuda? Buscar(TipoDocAyuda tipo, string clave, NivelAyuda nivelUsuario);
        // Documentos del manual de un rol (solo los de ESE rol), ordenados por modulo/orden/titulo.
        IReadOnlyList<DocAyuda> ManualDe(NivelAyuda rol);
        IReadOnlyList<DocAyuda> Buscar(string texto, NivelAyuda nivelUsuario);
        string RenderizarHtml(DocAyuda doc);
        string FragmentoParaBusqueda(DocAyuda doc, string texto);
    }

    public sealed class AyudaService : IAyudaService
    {
        private const string PrefijoRecurso = "manual/";

        // Imagenes y videos del manual (2026-10-03): viven en WebCore/wwwroot/ayuda/media/ (NO embebidos en
        // el .dll: un video pesaria en cada build) y se sirven con UseStaticFiles. En el .md se escriben
        // como ![texto](media:Usuarios.Index/buscar.png); los videos (.mp4/.webm) los convierte en <video>
        // la extension MediaLinks de Markdig (incluida en UseAdvancedExtensions).
        private const string PrefijoMedia = "media:";
        private const string RutaBaseMedia = "/ayuda/media/";

        private readonly ILogger<AyudaService> _logger;
        private readonly Lazy<IReadOnlyList<DocAyuda>> _docs;
        private readonly MarkdownPipeline _pipeline;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _htmlCache = new();

        public AyudaService(ILogger<AyudaService> logger)
        {
            _logger = logger;
            // HTML crudo deshabilitado: el contenido es del repo (confiable), pero no hay razon para
            // permitir <script> en un manual. Extensiones avanzadas: tablas, listas, etc.
            _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build();
            _docs = new Lazy<IReadOnlyList<DocAyuda>>(CargarDocumentos);
        }

        // ---- Consulta ----

        public DocAyuda? BuscarPantalla(string controller, string action, NivelAyuda nivelUsuario)
        {
            string clave = (controller ?? "") + "." + (action ?? "");
            return Buscar(TipoDocAyuda.Pantalla, clave, nivelUsuario);
        }

        public DocAyuda? Buscar(TipoDocAyuda tipo, string clave, NivelAyuda nivelUsuario)
        {
            return _docs.Value.FirstOrDefault(d =>
                d.Tipo == tipo
                && (string.Equals(d.Clave, clave ?? "", StringComparison.OrdinalIgnoreCase)
                    || d.Alias.Any(a => string.Equals(a, clave ?? "", StringComparison.OrdinalIgnoreCase)))
                && d.Nivel <= nivelUsuario);
        }

        public IReadOnlyList<DocAyuda> ManualDe(NivelAyuda rol)
        {
            return _docs.Value
                .Where(d => d.Nivel == rol)
                .OrderBy(d => d.Tipo == TipoDocAyuda.Concepto ? 0 : d.Tipo == TipoDocAyuda.Referencia ? 2 : 1)
                .ThenBy(d => string.IsNullOrEmpty(d.Modulo) ? "~" : d.Modulo, StringComparer.OrdinalIgnoreCase)
                .ThenBy(d => d.Orden)
                .ThenBy(d => d.Titulo, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public IReadOnlyList<DocAyuda> Buscar(string texto, NivelAyuda nivelUsuario)
        {
            texto = (texto ?? "").Trim();
            if (texto.Length < 2)
                return Array.Empty<DocAyuda>();

            return _docs.Value
                .Where(d => d.Nivel <= nivelUsuario
                    && (d.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                        || d.Markdown.Contains(texto, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(d => d.Titulo.Contains(texto, StringComparison.OrdinalIgnoreCase))
                .ThenBy(d => d.Titulo, StringComparer.OrdinalIgnoreCase)
                .Take(30)
                .ToList();
        }

        // Una linea de contexto alrededor del texto buscado (sin markdown) para la lista de resultados.
        public string FragmentoParaBusqueda(DocAyuda doc, string texto)
        {
            int i = doc.Markdown.IndexOf(texto ?? "", StringComparison.OrdinalIgnoreCase);
            if (i < 0)
                return "";

            int desde = Math.Max(0, i - 60);
            int largo = Math.Min(doc.Markdown.Length - desde, 160);
            string frag = doc.Markdown.Substring(desde, largo);
            // Se quitan los destinos de los enlaces markdown "](ayuda:...)" / "](http...)" y los signos de formato.
            frag = Regex.Replace(frag, @"\]\([^)]*\)?", " ");
            frag = Regex.Replace(frag, @"\(ayuda:[^)]*\)?", " ");
            frag = Regex.Replace(frag, @"[#*_`>|\[\]]+", " ");
            frag = Regex.Replace(frag, @"\s+", " ").Trim();
            return (desde > 0 ? "… " : "") + frag + " …";
        }

        // ---- Render ----

        // Los enlaces internos se escriben en el .md como (ayuda:pantalla/Usuarios.Index),
        // (ayuda:concepto/dispositivo-seguro) o (ayuda:referencia/configuraciones) y salen como
        // enlaces de la ayuda (data-ayuda="tipo/clave"): el panel los abre ADENTRO, sin navegar. Los
        // enlaces externos (http/https) abren en pestaña nueva para no sacar al usuario de su pantalla.
        public string RenderizarHtml(DocAyuda doc)
        {
            return _htmlCache.GetOrAdd(doc.Recurso, _ =>
            {
                var documento = Markdown.Parse(doc.Markdown, _pipeline);
                foreach (var link in documento.Descendants<LinkInline>().ToList())
                {
                    string url = link.Url ?? "";
                    if (url.StartsWith(PrefijoMedia, StringComparison.OrdinalIgnoreCase))
                    {
                        string subruta = url.Substring(PrefijoMedia.Length).Replace('\\', '/').TrimStart('/');
                        // El .md es del repo (confiable), pero igual se corta cualquier intento de salir de la carpeta.
                        if (subruta.Length == 0 || subruta.Contains("..", StringComparison.Ordinal))
                        {
                            link.Url = "#";
                            _logger.LogWarning("Ruta de media invalida '{Url}' en {Recurso}.", url, doc.Recurso);
                        }
                        else
                        {
                            link.Url = RutaBaseMedia + Uri.EscapeDataString(subruta).Replace("%2F", "/");
                            if (link.IsImage)
                                link.GetAttributes().AddClass("ayuda-media");
                        }
                    }
                    else if (url.StartsWith("ayuda:", StringComparison.OrdinalIgnoreCase))
                    {
                        link.Url = "#";
                        link.GetAttributes().AddProperty("data-ayuda", url.Substring("ayuda:".Length));
                    }
                    else if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        link.GetAttributes().AddProperty("target", "_blank");
                        link.GetAttributes().AddProperty("rel", "noopener noreferrer");
                    }
                    else if (!url.StartsWith("#", StringComparison.Ordinal))
                    {
                        // Ruta relativa a otro .md del repo: no existe como pagina de la app, se deja inerte.
                        link.Url = "#";
                    }
                }

                return Markdown.ToHtml(documento, _pipeline);
            });
        }

        // ---- Carga ----

        private IReadOnlyList<DocAyuda> CargarDocumentos()
        {
            var lista = new List<DocAyuda>();
            var assembly = typeof(AyudaService).Assembly;

            foreach (string nombre in assembly.GetManifestResourceNames())
            {
                string normalizado = nombre.Replace('\\', '/');
                if (!normalizado.StartsWith(PrefijoRecurso, StringComparison.Ordinal))
                    continue;

                string subruta = normalizado.Substring(PrefijoRecurso.Length);
                var partes = subruta.Split('/');
                // README.md y cualquier archivo suelto en la raiz son indices para quien lee el repo.
                if (partes.Length < 2)
                    continue;

                TipoDocAyuda? tipo = partes[0].ToLowerInvariant() switch
                {
                    "pantallas" => TipoDocAyuda.Pantalla,
                    "conceptos" => TipoDocAyuda.Concepto,
                    "referencia" => TipoDocAyuda.Referencia,
                    _ => null
                };
                if (tipo == null)
                    continue;

                try
                {
                    string contenido;
                    using (var stream = assembly.GetManifestResourceStream(nombre))
                    {
                        if (stream == null) continue;
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        contenido = reader.ReadToEnd();
                    }

                    string clave = Path.GetFileNameWithoutExtension(partes[^1]);
                    var (meta, cuerpo) = SepararEncabezado(contenido);

                    lista.Add(new DocAyuda
                    {
                        Recurso = normalizado,
                        Tipo = tipo.Value,
                        Clave = meta.TryGetValue("pantalla", out var p) && tipo == TipoDocAyuda.Pantalla ? p : clave,
                        Titulo = meta.TryGetValue("titulo", out var t) && t.Length > 0 ? t : clave,
                        Nivel = ParsearNivel(meta.GetValueOrDefault("rol", "usuario"), normalizado),
                        Modulo = meta.GetValueOrDefault("modulo", ""),
                        Orden = int.TryParse(meta.GetValueOrDefault("orden", "0"), out var o) ? o : 0,
                        Permiso = meta.GetValueOrDefault("permiso", ""),
                        Alias = meta.TryGetValue("alias", out var al) ? al.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : Array.Empty<string>(),
                        Revisada = meta.GetValueOrDefault("revisada", ""),
                        Markdown = cuerpo
                    });
                }
                catch (Exception ex)
                {
                    // Un .md roto no debe tirar abajo toda la ayuda: se loguea y se sigue.
                    _logger.LogError(ex, "No se pudo cargar el documento de ayuda {Recurso}.", normalizado);
                }
            }

            _logger.LogInformation("Ayuda: {Cantidad} documentos cargados.", lista.Count);
            return lista;
        }

        private NivelAyuda ParsearNivel(string valor, string recurso)
        {
            switch ((valor ?? "").Trim().ToLowerInvariant())
            {
                case "usuario": return NivelAyuda.Usuario;
                case "admin": return NivelAyuda.Admin;
                case "superadmin": return NivelAyuda.SuperAdmin;
                default:
                    // Ante un rol desconocido se usa el mas restrictivo: nunca exponer de mas.
                    _logger.LogWarning("Rol de ayuda desconocido '{Valor}' en {Recurso}: se trata como superadmin.", valor, recurso);
                    return NivelAyuda.SuperAdmin;
            }
        }

        // Encabezado opcional "--- clave: valor ... ---" al principio del archivo.
        internal static (Dictionary<string, string> meta, string cuerpo) SepararEncabezado(string contenido)
        {
            var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            contenido = (contenido ?? "").TrimStart('﻿');

            var lineas = contenido.Replace("\r\n", "\n").Split('\n');
            if (lineas.Length == 0 || lineas[0].Trim() != "---")
                return (meta, contenido);

            int fin = -1;
            for (int i = 1; i < lineas.Length; i++)
            {
                if (lineas[i].Trim() == "---") { fin = i; break; }
            }

            if (fin < 0)
                return (meta, contenido);

            for (int i = 1; i < fin; i++)
            {
                string linea = lineas[i];
                int dosPuntos = linea.IndexOf(':');
                if (dosPuntos <= 0) continue;

                string clave = linea.Substring(0, dosPuntos).Trim();
                string valor = linea.Substring(dosPuntos + 1);
                // Comentario al final de la linea: "rol: admin   # quien la ve".
                int numeral = valor.IndexOf(" #", StringComparison.Ordinal);
                if (numeral >= 0) valor = valor.Substring(0, numeral);
                meta[clave] = valor.Trim();
            }

            return (meta, string.Join("\n", lineas.Skip(fin + 1)).TrimStart('\n'));
        }
    }
}
