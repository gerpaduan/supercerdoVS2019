using System;
using System.Data;

namespace Contratos
{
    // Auditoria de las autorizaciones provisorias del POS (2026-11-03, ver docs/DECISIONS.md "POS: autorizacion
    // provisoria de otro usuario"). Cada vez que un usuario con permiso autoriza al operador a hacer una accion que no
    // podia, queda una fila con quien autorizo, a quien, que accion y sobre que registro; al cerrarse la ventana de la
    // accion (guardar, cerrar, terminar la venta, vencer) se completa fechafin y motivofin. Solo Postgres.
    public interface IAutorizacionPosRepository
    {
        // Registra la autorizacion y devuelve el id de la fila (para cerrarla despues).
        int Registrar(int idOperador, int idAutorizador, int idUsuarioSesion, string posInstanceId, string accion, int idRegistro);

        // Marca el fin de la autorizacion. No hace nada si ya estaba cerrada.
        void Cerrar(int idAuditoria, string motivoFin);

        // Autorizaciones con fecha en el rango, para el modulo Actividades (solo lectura). Columnas: id, fecha, accion,
        // idregistro, operador, autorizador, cuenta, fechafin, motivofin.
        DataTable ObtenerAutorizaciones(DateTime desde, DateTime hasta);
    }
}
