// Logica compartida del selector multi-seleccion de los filtros Cliente / Vendedor / Proveedor
// (componente filtro-picker.js + FiltroPickerController). Lo usan el controller JSON del modal y
// los controllers de Ventas/Compras (parseo de IDs y nombres resueltos para el encabezado del export).
// Detalle del componente: docs/DECISIONS.md (2026-10-13, "Buscador multi-seleccion en filtros").
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace WebCore.Services
{
    // Una persona (cliente / proveedor) o un vendedor (usuario) tal como lo muestra el modal.
    public class FiltroPickerItem
    {
        public int Id { get; set; }
        // Razon social (personas) o nombre (usuarios).
        public string Nombre { get; set; } = "";
        // Identificacion (personas) o usuario de login (vendedores), solo si es distinta del nombre:
        // es la etiqueta chica que se muestra debajo del input.
        public string Sub { get; set; } = "";
        // CUIT de la persona (columna extra del modal). Vacio para vendedores.
        public string Cuit { get; set; } = "";
        // Persona con cuenta corriente reservada (candado en el modal, igual que Personas/Listar).
        public bool Reservada { get; set; }
        // Usuario dado de baja: sus ventas historicas siguen existiendo, por eso se lista igual.
        public bool Inactivo { get; set; }
    }

    public class FiltroPickerService
    {
        // Tope de filas que devuelve una busqueda del modal: sin limite, un filtro vacio traeria
        // todas las personas de la empresa. Si hay mas, el modal avisa "refina la busqueda".
        public const int MaxFilasModal = 200;

        // Tope de IDs aceptados por un filtro (querystring / IN (...) en SQL). Mas alla de esto el
        // usuario esta pidiendo "casi todos": se descartan los sobrantes en vez de armar un IN gigante.
        public const int MaxIdsFiltro = 500;

        private readonly Negocio.Persona _personaN;
        private readonly Negocio.Usuario _usuarioN;

        public FiltroPickerService(Negocio.Persona personaN, Negocio.Usuario usuarioN)
        {
            _personaN = personaN;
            _usuarioN = usuarioN;
        }

        // CSV "1,2,3" -> lista de IDs validos. Input externo (querystring): solo enteros > 0, sin
        // repetidos, hasta MaxIdsFiltro. Cualquier otra cosa se ignora (nunca llega a un SQL como texto).
        public static List<int> ParsearIds(string csv)
        {
            var resultado = new List<int>();
            if (string.IsNullOrWhiteSpace(csv)) return resultado;

            foreach (string parte in csv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(parte.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int id) || id <= 0)
                    continue;
                if (resultado.Contains(id)) continue;

                resultado.Add(id);
                if (resultado.Count >= MaxIdsFiltro) break;
            }

            return resultado;
        }

        // Opciones de JSON para el atributo data-items de los inputs (el JS lee id / nombre / sub en minuscula).
        private static readonly System.Text.Json.JsonSerializerOptions OpcionesJsonAtributo = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        // Items seleccionados como JSON para data-items="..." (Razor escapa el atributo). "" si no hay.
        public static string ItemsAJson(IEnumerable<FiltroPickerItem> items)
        {
            var lista = (items ?? Enumerable.Empty<FiltroPickerItem>()).ToList();
            return lista.Count == 0 ? "" : System.Text.Json.JsonSerializer.Serialize(lista, OpcionesJsonAtributo);
        }

        // Nombres separados por coma para encabezados de export ("Pérez S.A., Gómez"). Vacio si no hay items.
        public static string UnirNombres(IEnumerable<FiltroPickerItem> items)
        {
            return string.Join(", ", (items ?? Enumerable.Empty<FiltroPickerItem>())
                .Select(x => x.Nombre)
                .Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        // Personas (clientes y proveedores son la misma tabla) que coinciden con el texto. Mismo
        // buscador que el modal de persona del POS (buscarPersona, marca=false).
        public (List<FiltroPickerItem> Items, bool HayMas) BuscarPersonas(string filtro)
        {
            DataTable dt = _personaN.buscarPersona(filtro ?? "", false) ?? new DataTable();

            var todos = dt.AsEnumerable()
                .Select(LeerPersona)
                .Where(x => x.Id > 0)
                .OrderBy(x => x.Nombre, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return (todos.Take(MaxFilasModal).ToList(), todos.Count > MaxFilasModal);
        }

        // Vendedores = usuarios de la empresa (activos e inactivos: hay ventas historicas de usuarios
        // dados de baja). No hay busqueda por texto en negocio, se filtra en memoria por nombre/usuario.
        public (List<FiltroPickerItem> Items, bool HayMas) BuscarVendedores(string filtro)
        {
            string texto = (filtro ?? "").Trim();

            var todos = LeerUsuarios()
                // Sub = usuario de login cuando difiere del nombre; si es igual ya se busca por Nombre.
                .Where(x => texto.Length == 0
                    || x.Nombre.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0
                    || x.Sub.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                // Activos primero: los inactivos solo importan para ventas historicas.
                .OrderBy(x => x.Inactivo)
                .ThenBy(x => x.Nombre, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return (todos.Take(MaxFilasModal).ToList(), todos.Count > MaxFilasModal);
        }

        // Datos de las personas con esos IDs (para hidratar un input que llega con IDs por URL y para
        // el encabezado de los exports). Los IDs que ya no existen se omiten.
        public List<FiltroPickerItem> ResolverPersonas(IEnumerable<int> ids)
        {
            var resultado = new List<FiltroPickerItem>();
            foreach (int id in (ids ?? Enumerable.Empty<int>()).Where(x => x > 0).Take(MaxIdsFiltro))
            {
                var persona = _personaN.findById(id);
                if (persona == null || persona.IdPersona <= 0) continue;

                resultado.Add(new FiltroPickerItem
                {
                    Id = persona.IdPersona,
                    Nombre = persona.RazonSocial ?? "",
                    Sub = SubSiDistinto(persona.Identificacion, persona.RazonSocial),
                    Cuit = persona.Cuit ?? "",
                    Reservada = persona.CtaCteReservada
                });
            }

            return resultado;
        }

        public List<FiltroPickerItem> ResolverVendedores(IEnumerable<int> ids)
        {
            var pedidos = new HashSet<int>((ids ?? Enumerable.Empty<int>()).Where(x => x > 0).Take(MaxIdsFiltro));
            if (pedidos.Count == 0) return new List<FiltroPickerItem>();

            return LeerUsuarios().Where(x => pedidos.Contains(x.Id)).ToList();
        }

        // ---- helpers de lectura de DataTable (las columnas pueden faltar segun el motor) ----

        private static FiltroPickerItem LeerPersona(DataRow row)
        {
            string razonSocial = Texto(row, "razonSocial");
            return new FiltroPickerItem
            {
                Id = Entero(row, "idPersona"),
                Nombre = razonSocial,
                Sub = SubSiDistinto(Texto(row, "nombreIdentif"), razonSocial),
                Cuit = Texto(row, "cuit"),
                Reservada = Booleano(row, "ctaCteReservada")
            };
        }

        // Usuarios de la empresa (activos e inactivos) como items. La propiedad Sub lleva el usuario
        // de login solo si difiere del nombre (misma regla que la identificacion de las personas).
        private List<FiltroPickerItem> LeerUsuarios()
        {
            DataTable dt = _usuarioN.obtenerUsuarios(false) ?? new DataTable();

            return dt.AsEnumerable()
                .Select(row =>
                {
                    string nombre = Texto(row, "nombre");
                    return new FiltroPickerItem
                    {
                        Id = Entero(row, "id"),
                        Nombre = nombre,
                        Sub = SubSiDistinto(Texto(row, "usuario"), nombre),
                        // Si la columna "activo" no viene, se asume activo (no se marca "inactivo" por error).
                        Inactivo = row.Table.Columns.Contains("activo") && row["activo"] != DBNull.Value && !Convert.ToBoolean(row["activo"])
                    };
                })
                .Where(x => x.Id > 0 && !string.IsNullOrWhiteSpace(x.Nombre))
                .ToList();
        }

        // Identificacion a mostrar debajo del input: solo si existe y es distinta de la razon social.
        private static string SubSiDistinto(string identificacion, string nombre)
        {
            string sub = (identificacion ?? "").Trim();
            if (sub.Length == 0) return "";
            return string.Equals(sub, (nombre ?? "").Trim(), StringComparison.OrdinalIgnoreCase) ? "" : sub;
        }

        private static string Texto(DataRow row, string columna)
        {
            if (!row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value) return "";
            return Convert.ToString(row[columna], CultureInfo.InvariantCulture) ?? "";
        }

        private static int Entero(DataRow row, string columna)
        {
            if (!row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value) return 0;
            return int.TryParse(Convert.ToString(row[columna], CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor) ? valor : 0;
        }

        private static bool Booleano(DataRow row, string columna)
        {
            if (!row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value) return false;
            return Convert.ToBoolean(row[columna], CultureInfo.InvariantCulture);
        }
    }
}
