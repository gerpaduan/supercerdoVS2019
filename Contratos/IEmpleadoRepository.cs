using System;
using System.Collections.Generic;

namespace Contratos
{
    // Modulo Empleados y Liquidacion de Sueldos (2026-09-29, ver docs/DECISIONS.md). Solo Postgres
    // (decision del usuario: el modulo nace nuevo, sin nada que portar del legado SQL Server) --
    // no hay implementacion Datos/, NegocioFactory no tiene rama SQL Server para este repositorio.
    public interface IEmpleadoRepository
    {
        List<Entidades.Empleado> Listar(int idEmpresa, string texto, Entidades.Empleado.formaLiquidacion? forma, bool? soloActivos);
        Entidades.Empleado ObtenerPorId(int id, int idEmpresa);
        Entidades.Empleado ObtenerPorIdUsuario(int idUsuario, int idEmpresa);

        int Agregar(Entidades.Empleado empleado);
        void Editar(Entidades.Empleado empleado);
        void SetActivo(int idEmpleado, int idEmpresa, bool activo);

        bool ExisteLegajo(string legajo, int idEmpresa, int idExcluir);
        bool ExistePersonaVinculada(int idPersona, int idEmpresa, int idExcluir);
        bool ExisteUsuarioVinculado(int idUsuario, int idEmpresa, int idExcluir);

        // Tarifas: append-only (nunca se edita/borra una fila -- ver Entidades/EmpleadoTarifa.cs).
        // ListarTarifas devuelve TODO el historial (usado tanto para resolver la vigente en el
        // calculo como para la pantalla Empleados/Historial).
        List<Entidades.EmpleadoTarifa> ListarTarifas(int idEmpleado);
        void AgregarTarifa(Entidades.EmpleadoTarifa tarifa);

        List<Entidades.EmpleadoVacacion> ListarVacaciones(int idEmpleado);
        void AgregarVacacion(Entidades.EmpleadoVacacion vacacion);

        // Periodo de vacaciones vigente para esa fecha, o null. Usado por "Mi Jornada" (aviso, no
        // exige marcacion) y por el calculo de liquidacion.
        Entidades.EmpleadoVacacion ObtenerVacacionVigente(int idEmpleado, DateTime fecha);
    }
}
