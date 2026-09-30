using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    // Empleados y Liquidacion de Sueldos (2026-09-29, ver docs/DECISIONS.md). Solo Postgres --
    // a diferencia del resto de Negocio/*.cs no hay constructor legado SQL Server: el modulo nace
    // nuevo sin nada que portar (decision del usuario). Wrapper delgado sobre
    // Contratos.IEmpleadoRepository/ITarifaGeneralRepository, orquestando Negocio.Persona y
    // Negocio.Usuario para el alta unificada.
    public class Empleado
    {
        private readonly Contratos.IEmpleadoRepository oEmpleadoD;
        private readonly Contratos.ITarifaGeneralRepository oTarifaGeneralD;
        private readonly Negocio.Persona oPersonaN;
        private readonly Negocio.Usuario oUsuarioN;

        public Empleado(Contratos.IEmpleadoRepository repositorio, Contratos.ITarifaGeneralRepository tarifaGeneralRepositorio,
            Negocio.Persona personaN, Negocio.Usuario usuarioN)
        {
            oEmpleadoD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            oTarifaGeneralD = tarifaGeneralRepositorio ?? throw new ArgumentNullException(nameof(tarifaGeneralRepositorio));
            oPersonaN = personaN ?? throw new ArgumentNullException(nameof(personaN));
            oUsuarioN = usuarioN ?? throw new ArgumentNullException(nameof(usuarioN));
        }

        public List<Entidades.Empleado> Listar(int idEmpresa, string texto, Entidades.Empleado.formaLiquidacion? forma, bool? soloActivos)
        {
            return oEmpleadoD.Listar(idEmpresa, texto, forma, soloActivos);
        }

        public Entidades.Empleado ObtenerPorId(int id, int idEmpresa)
        {
            return oEmpleadoD.ObtenerPorId(id, idEmpresa);
        }

        public Entidades.Empleado ObtenerPorIdUsuario(int idUsuario, int idEmpresa)
        {
            return oEmpleadoD.ObtenerPorIdUsuario(idUsuario, idEmpresa);
        }

        // Usados por EmpleadosController.BuscarPersonas/BuscarUsuarios para excluir de la busqueda
        // (alta) a quien ya es empleado de otro -- validacion preventiva, ver docs/DECISIONS.md
        // 2026-09-30. GuardarUnificado revalida lo mismo server-side al confirmar.
        public bool ExistePersonaVinculada(int idPersona, int idEmpresa)
        {
            return oEmpleadoD.ExistePersonaVinculada(idPersona, idEmpresa, 0);
        }

        public bool ExisteUsuarioVinculado(int idUsuario, int idEmpresa)
        {
            return oEmpleadoD.ExisteUsuarioVinculado(idUsuario, idEmpresa, 0);
        }

        // Alta/edicion unificada (una sola pantalla, decision del usuario 2026-09-29).
        //
        // Alta (empleado.Id==0): oPersonaE.idPersona==0 crea una Persona nueva (Tipo="Empleado");
        // >0 VINCULA una Persona ya existente tal cual está (no se editan sus campos -- si se
        // quisiera tocarlos habría que hacerlo desde la pantalla de Personas). Mismo criterio para
        // oUsuarioE.Id con Usuario.
        //
        // Edicion (empleado.Id>0): siempre actualiza los datos de la Persona/Usuario YA vinculados
        // a este empleado (se ignora cualquier Id que venga en oPersonaE/oUsuarioE) -- este
        // formulario no permite re-vincular a otra Persona/Usuario existente; si hiciera falta,
        // se resuelve dando de baja este empleado y creando uno nuevo.
        public int GuardarUnificado(Entidades.Empleado empleado, Entidades.Persona oPersonaE, Entidades.Usuario oUsuarioE)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));
            if (oUsuarioE == null) throw new ArgumentNullException(nameof(oUsuarioE));

            if (string.IsNullOrWhiteSpace(empleado.Legajo))
                throw new InvalidOperationException("El legajo es obligatorio.");
            empleado.Legajo = empleado.Legajo.Trim();

            if (oEmpleadoD.ExisteLegajo(empleado.Legajo, empleado.IdEmpresa, empleado.Id))
                throw new InvalidOperationException($"Ya existe un empleado con el legajo \"{empleado.Legajo}\".");

            if (empleado.Id > 0)
            {
                var actual = oEmpleadoD.ObtenerPorId(empleado.Id, empleado.IdEmpresa);
                if (actual == null) throw new InvalidOperationException("El empleado no existe.");

                oPersonaE.idPersona = actual.Persona.idPersona;
                oPersonaE.IdEmpresa = empleado.IdEmpresa;
                oPersonaN.addOrEditPersona(oPersonaE);

                // Misma mecanica que UsuariosController.Guardar: addOrEditUser siempre recibe el
                // campo legado Clave (se conserva el valor existente si no se tipeo una nueva), y
                // la clave real (hash) se actualiza aparte solo si se tipeo una.
                var usuarioActual = oUsuarioN.getUsuarioById(actual.Usuario.Id);
                string claveNueva = oUsuarioE.Clave;
                oUsuarioE.Id = actual.Usuario.Id;
                oUsuarioE.IdEmpresa = empleado.IdEmpresa;
                oUsuarioE.Clave = usuarioActual?.Clave ?? "";
                oUsuarioE.ColorForm = usuarioActual != null && !string.IsNullOrWhiteSpace(usuarioActual.ColorForm) ? usuarioActual.ColorForm : "SteelBlue";
                oUsuarioN.addOrEditUser(oUsuarioE);

                if (!string.IsNullOrWhiteSpace(claveNueva))
                    oUsuarioN.ActualizarPasswordWebSeguro(oUsuarioE.Id, claveNueva.Trim());

                empleado.Persona = oPersonaE;
                empleado.Usuario = oUsuarioE;
                oEmpleadoD.Editar(empleado);
                return empleado.Id;
            }

            if (oPersonaE.idPersona == 0)
            {
                oPersonaE.Tipo = "Empleado";
                oPersonaE.IdEmpresa = empleado.IdEmpresa;
                oPersonaE.idPersona = oPersonaN.addOrEditPersonaConId(oPersonaE);
            }
            else if (oEmpleadoD.ExistePersonaVinculada(oPersonaE.idPersona, empleado.IdEmpresa, 0))
            {
                throw new InvalidOperationException("Esa persona ya está vinculada a otro empleado.");
            }

            if (oUsuarioE.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(oUsuarioE.Clave))
                    throw new InvalidOperationException("La clave es obligatoria para un usuario nuevo.");

                oUsuarioE.IdEmpresa = empleado.IdEmpresa;
                oUsuarioN.addOrEditUser(oUsuarioE);

                // addOrEditUser no siempre devuelve el Id en la instancia (ver
                // UsuariosController.Guardar): se resuelve buscando por login en la empresa.
                if (oUsuarioE.Id <= 0)
                {
                    oUsuarioN.obtenerUsuarios(false);
                    oUsuarioE.Id = (oUsuarioN.listaUsuario() ?? new List<Entidades.Usuario>())
                        .Where(u => u != null && u.IdEmpresa == empleado.IdEmpresa)
                        .Where(u => string.Equals(u.User ?? "", oUsuarioE.User ?? "", StringComparison.OrdinalIgnoreCase))
                        .Select(u => u.Id)
                        .FirstOrDefault();
                }
            }
            else if (oEmpleadoD.ExisteUsuarioVinculado(oUsuarioE.Id, empleado.IdEmpresa, 0))
            {
                throw new InvalidOperationException("Ese usuario ya está vinculado a otro empleado.");
            }

            empleado.Persona = oPersonaE;
            empleado.Usuario = oUsuarioE;
            return oEmpleadoD.Agregar(empleado);
        }

        // Baja/reactivacion con cascada al Usuario vinculado (regla de negocio -- ver
        // Entidades/Empleado.cs). Se carga el Usuario completo antes de guardar: addOrEditUser
        // actualiza la fila entera, un objeto parcial pisaria Nombre/Clave/etc. con vacio.
        public void SetActivo(int idEmpleado, int idEmpresa, bool activo)
        {
            var empleado = oEmpleadoD.ObtenerPorId(idEmpleado, idEmpresa);
            if (empleado == null) throw new InvalidOperationException("El empleado no existe.");

            oEmpleadoD.SetActivo(idEmpleado, idEmpresa, activo);

            var usuarioCompleto = oUsuarioN.getUsuarioById(empleado.Usuario.Id);
            if (usuarioCompleto != null)
            {
                usuarioCompleto.Activo = activo;
                oUsuarioN.addOrEditUser(usuarioCompleto);
            }
        }

        public List<Entidades.EmpleadoTarifa> ListarTarifas(int idEmpleado)
        {
            return oEmpleadoD.ListarTarifas(idEmpleado);
        }

        public void AgregarTarifa(Entidades.EmpleadoTarifa tarifa)
        {
            if (tarifa == null) throw new ArgumentNullException(nameof(tarifa));
            if (tarifa.Valor < 0) throw new InvalidOperationException("El valor de la tarifa no puede ser negativo.");
            oEmpleadoD.AgregarTarifa(tarifa);
        }

        public List<Entidades.EmpleadoVacacion> ListarVacaciones(int idEmpleado)
        {
            return oEmpleadoD.ListarVacaciones(idEmpleado);
        }

        public void AgregarVacacion(Entidades.EmpleadoVacacion vacacion)
        {
            if (vacacion == null) throw new ArgumentNullException(nameof(vacacion));
            if (vacacion.FechaHasta.Date < vacacion.FechaDesde.Date)
                throw new InvalidOperationException("La fecha hasta debe ser posterior o igual a la fecha desde.");
            oEmpleadoD.AgregarVacacion(vacacion);
        }

        public Entidades.EmpleadoVacacion ObtenerVacacionVigente(int idEmpleado, DateTime fecha)
        {
            return oEmpleadoD.ObtenerVacacionVigente(idEmpleado, fecha);
        }

        public List<Entidades.TarifaGeneral> ListarTarifaGeneral(int idEmpresa)
        {
            return oTarifaGeneralD.Listar(idEmpresa);
        }

        public void AgregarTarifaGeneral(Entidades.TarifaGeneral tarifa)
        {
            if (tarifa == null) throw new ArgumentNullException(nameof(tarifa));
            oTarifaGeneralD.Agregar(tarifa);
        }

        // Para el boton "Copiar tarifa estandar" en Empleados/Editar.
        public List<Entidades.TarifaGeneral> ObtenerTarifaGeneralVigente(int idEmpresa, Entidades.Empleado.formaLiquidacion forma)
        {
            return oTarifaGeneralD.ListarVigentes(idEmpresa, forma);
        }

        // Resolucion de tarifa vigente (turno+dia exacto > turno general), compartida entre la
        // grilla de Empleados/Historial y el calculo de Negocio.LiquidacionSueldo. Devuelve null si
        // no hay ninguna fila configurada para ese turno (ver "que pasa si no hay tarifa" en
        // docs/03-modulos/empleados-y-liquidacion-sueldos.md -- no se inventa un valor).
        public static Entidades.EmpleadoTarifa ResolverTarifaVigente(List<Entidades.EmpleadoTarifa> historial, Entidades.Turno? turno, DateTime fecha)
        {
            if (historial == null) return null;
            var diaSemana = ConvertirDiaSemana(fecha.DayOfWeek);

            var candidatas = historial.Where(t => t.Turno == turno && t.VigenteDesde.Date <= fecha.Date);

            var conDia = candidatas.Where(t => t.DiaSemana == diaSemana).OrderByDescending(t => t.VigenteDesde).FirstOrDefault();
            if (conDia != null) return conDia;

            return candidatas.Where(t => t.DiaSemana == null).OrderByDescending(t => t.VigenteDesde).FirstOrDefault();
        }

        public static Entidades.DiaSemana ConvertirDiaSemana(DayOfWeek dow)
        {
            switch (dow)
            {
                case DayOfWeek.Monday: return Entidades.DiaSemana.Lunes;
                case DayOfWeek.Tuesday: return Entidades.DiaSemana.Martes;
                case DayOfWeek.Wednesday: return Entidades.DiaSemana.Miercoles;
                case DayOfWeek.Thursday: return Entidades.DiaSemana.Jueves;
                case DayOfWeek.Friday: return Entidades.DiaSemana.Viernes;
                case DayOfWeek.Saturday: return Entidades.DiaSemana.Sabado;
                default: return Entidades.DiaSemana.Domingo;
            }
        }
    }
}
