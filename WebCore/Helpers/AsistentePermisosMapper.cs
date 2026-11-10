// Traduce entre la fila guardada de `permisosusuarios` (dias ver / dias editar / solo propios, con -1 = sin permiso) y el
// estado simple que maneja el asistente de usuarios (nivel Sin acceso / Ver / Ver y modificar, dias, "todo el historial",
// alcance). Es logica pura, sin base de datos ni sesion, para poder probarla (WebCore.Tests/AsistentePermisosMapperTests).
// Regla central: guardar sin tocar nada NO cambia ningun valor guardado. Lo que el asistente no puede representar
// (ej. editar sin ver) queda marcado "Personalizado" y se conserva tal cual; los controles que el catalogo oculta por no
// tener efecto conservan el valor que ya tenian. Detalle: docs/DECISIONS.md (2026-11-10).
using Entidades;

namespace WebCore.Helpers
{
    // Estado de un permiso en el asistente. Es tambien lo que viaja en el formulario (binding) y en el JSON de la pagina.
    public class EstadoPermisoAsistente
    {
        public int IdForm { get; set; }
        // 0 = sin acceso; 1 = ver (o "si" en los permisos de un solo nivel); 2 = ver y modificar.
        public int Nivel { get; set; }
        public int DiasVer { get; set; }
        // "Todo el historial": se guarda como AsistentePermisosMapper.DiasTodoElHistorial dias.
        public bool TodoVer { get; set; }
        public int DiasEditar { get; set; }
        public bool TodoEditar { get; set; }
        // true = solo lo que cargo el propio usuario (alcance "Propios").
        public bool SoloPropios { get; set; } = true;
        // La fila guardada tiene una combinacion que el asistente no representa: se muestra aparte y NO se toca al guardar.
        public bool Personalizado { get; set; }
    }

    public static class AsistentePermisosMapper
    {
        // "Todo el historial" = 10 anios. No se agrega un valor especial a la base: el chequeo de fechas sigue siendo el de siempre.
        public const int DiasTodoElHistorial = 3650;
        // Tope de lo que se acepta tipeado (100 anios): evita que un valor absurdo desborde la resta de fechas del chequeo de permisos.
        public const int DiasMaximos = 36500;

        // Estado del asistente a partir de la fila guardada (null = el usuario nunca tuvo fila: sin acceso).
        public static EstadoPermisoAsistente EstadoDesde(PermisosUsuarios? guardado, DefinicionPermiso definicion)
        {
            int diasVerGuardados = guardado?.DiasPermitidosVer ?? -1;
            int diasEditarGuardados = guardado?.DiasPermitidosEditar ?? -1;
            bool ver = diasVerGuardados >= 0;
            bool editar = diasEditarGuardados >= 0;

            var estado = new EstadoPermisoAsistente
            {
                IdForm = definicion.IdForm,
                DiasVer = ver ? diasVerGuardados : CatalogoPermisos.DiasVerPorDefecto,
                DiasEditar = editar ? diasEditarGuardados : CatalogoPermisos.DiasEditarPorDefecto,
                SoloPropios = guardado?.SoloRegistrosPropios ?? true
            };
            estado.TodoVer = estado.DiasVer >= DiasTodoElHistorial;
            estado.TodoEditar = estado.DiasEditar >= DiasTodoElHistorial;

            switch (definicion.Tipo)
            {
                case TipoPermiso.VerYModificar:
                    if (!ver && !editar) estado.Nivel = 0;
                    else if (ver && !editar) estado.Nivel = 1;
                    else if (ver) estado.Nivel = 2;
                    else estado.Nivel = 0; // editar sin ver
                    estado.Personalizado = !ver && editar;
                    break;

                case TipoPermiso.SoloModificar:
                    // El codigo solo mira la clave de edicion; "ver" suelto no habilita nada.
                    if (editar) estado.Nivel = 1;
                    else estado.Nivel = 0;
                    estado.Personalizado = !editar && ver;
                    break;

                default: // SoloVer
                    if (ver) estado.Nivel = 1;
                    else estado.Nivel = 0;
                    estado.Personalizado = !ver && editar;
                    break;
            }

            return estado;
        }

        // Fila a guardar a partir del estado del asistente. `existente` es la fila guardada hoy (para conservar lo que el
        // asistente no muestra). Una cuenta de produccion debe llamar con el Nivel ya forzado a 0 en las filas bloqueadas.
        public static PermisosUsuarios Aplicar(int idUsuario, EstadoPermisoAsistente estado, DefinicionPermiso definicion, PermisosUsuarios? existente)
        {
            int existenteVer = existente?.DiasPermitidosVer ?? -1;
            int existenteEditar = existente?.DiasPermitidosEditar ?? -1;
            bool propiosExistente = existente?.SoloRegistrosPropios ?? true;

            // Fila especial que el asistente no representa: se deja como esta.
            if (estado.Personalizado && existente != null)
            {
                return Fila(idUsuario, definicion.IdForm, existenteVer, existenteEditar, propiosExistente);
            }

            int nivelMaximo = definicion.Tipo == TipoPermiso.VerYModificar ? 2 : 1;
            int nivel = Math.Max(0, Math.Min(estado.Nivel, nivelMaximo));
            if (nivel == 0) return Fila(idUsuario, definicion.IdForm, -1, -1, true);

            int diasVer = DiasAGuardar(definicion.UsaDiasVer, estado.TodoVer, estado.DiasVer, existenteVer);
            int diasEditar = DiasAGuardar(definicion.UsaDiasEditar, estado.TodoEditar, estado.DiasEditar, existenteEditar);

            switch (definicion.Tipo)
            {
                case TipoPermiso.VerYModificar:
                    if (nivel == 1) return Fila(idUsuario, definicion.IdForm, diasVer, -1, true);
                    return Fila(idUsuario, definicion.IdForm, diasVer, diasEditar,
                        definicion.UsaAlcance ? estado.SoloPropios : propiosExistente);

                case TipoPermiso.SoloModificar:
                    // El permiso real es el de edicion; "ver" no lo mira ningun chequeo, asi que se conserva tal cual estaba.
                    return Fila(idUsuario, definicion.IdForm, existenteVer, diasEditar,
                        definicion.UsaAlcance ? estado.SoloPropios : propiosExistente);

                default: // SoloVer
                    // Cierres de Caja: ver arrastra modificar con los mismos dias. En el resto se conserva lo que ya hubiera en "editar".
                    int editarGuardado = definicion.EspejaVerEnModificar ? diasVer : (existenteEditar >= 0 ? existenteEditar : -1);
                    return Fila(idUsuario, definicion.IdForm, diasVer, editarGuardado, propiosExistente);
            }
        }

        // Dias a guardar de un control. Si el catalogo lo oculta por no tener efecto, se conserva el valor guardado (o 0 = hoy).
        private static int DiasAGuardar(bool controlVisible, bool todoElHistorial, int diasElegidos, int diasGuardados)
        {
            if (!controlVisible) return diasGuardados >= 0 ? diasGuardados : 0;
            if (todoElHistorial) return Math.Max(DiasTodoElHistorial, diasGuardados);
            return Math.Min(DiasMaximos, Math.Max(0, diasElegidos));
        }

        private static PermisosUsuarios Fila(int idUsuario, int idForm, int diasVer, int diasEditar, bool soloPropios)
        {
            return new PermisosUsuarios
            {
                IdUsuario = idUsuario,
                IdForm = idForm,
                DiasPermitidosVer = diasVer,
                DiasPermitidosEditar = diasEditar,
                SoloRegistrosPropios = soloPropios
            };
        }

        // true si con estos niveles el usuario entra al POS. Misma regla que VentasController.TieneAlgunPermisoPOS
        // (ver "Da acceso al POS" en el catalogo); la usa el indicador "Entra al POS" del asistente.
        public static bool DaAccesoAlPos(IEnumerable<(DefinicionPermiso Definicion, EstadoPermisoAsistente Estado)> permisos)
        {
            foreach (var (definicion, estado) in permisos)
            {
                if (estado.Nivel >= 1 && definicion.PosConVer) return true;
                if (estado.Nivel >= 2 && definicion.PosConModificar) return true;
                // Los de un solo nivel "Si" usan Nivel 1 como su unica opcion.
                if (definicion.Tipo != TipoPermiso.VerYModificar && estado.Nivel >= 1 && definicion.PosConModificar) return true;
            }
            return false;
        }
    }
}
