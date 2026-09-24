using System;
using System.Collections.Generic;

namespace Contratos
{
    // Borradores en servidor de Compras/Stock/Movimientos/Embutidos y notificaciones al admin (ver
    // docs/DECISIONS.md "Borradores de Compras/Stock/Movimientos/Embutidos"). Solo Postgres: no hay
    // implementacion SQL Server (mismo criterio que IVentaBorradorRepository). Todos los metodos
    // operan sobre la empresa del tenant con que se creo el repositorio (RLS). Analogo generico de
    // IVentaBorradorRepository, sin la seccion de "producto sin agregar" (exclusiva de POS) y con
    // "modulo" como parametro extra en listado/latido/marcado (una tabla reusada por los 4 modulos).
    public interface IBorradorGenericoRepository
    {
        // ===== Borradores (borradorgenerico) =====

        // Upsert por clientId. Solo actualiza si el borrador esta ACTIVA y es del mismo operador,
        // sucursal y modulo; de lo contrario informa por que no lo hizo. Completa borrador.Id al guardar.
        Entidades.ResultadoGuardarBorradorGenerico Guardar(Entidades.BorradorGenerico borrador);

        // Solo actualiza ultimolatido (el formulario no cambio). false si no existe, esta cerrado o es ajeno.
        bool RegistrarLatido(Guid clientId, int idOperador, int idSucursal, string modulo);

        // El navegador avisa (pagehide) que cierra la pestana: en vez de esperar el umbral de latido,
        // se envejece ultimolatido para que el borrador quede "interrumpido" (recuperable/notificable)
        // de inmediato. false si no existe o no esta ACTIVA.
        bool EnvejecerLatidoPorCierre(int idBorrador);

        // Con Payload incluido. null si no existe.
        Entidades.BorradorGenerico ObtenerPorClientId(Guid clientId, string modulo);
        Entidades.BorradorGenerico ObtenerPorId(int id);

        // ACTIVOS (con Payload) de toda una sucursal para un modulo, mas viejos primero.
        List<Entidades.BorradorGenerico> ListarActivasPorSucursal(int idSucursal, string modulo);

        // ACTIVOS (sin Payload) de una cuenta de sesion en cualquier sucursal/modulo; los usa el logout.
        List<Entidades.BorradorGenerico> ListarActivasPorUsuarioSesion(int idUsuarioSesion);

        // Traspasa un borrador ACTIVO a un operador (recuperarlo): actualiza operador, cuenta de sesion
        // y ultimo latido (queda "en uso"). false si no estaba ACTIVO.
        bool TomarBorrador(int id, int idOperador, int idUsuarioSesion);

        // ACTIVO -> FINALIZADA con el id de lo creado; vacia el payload. false si no estaba ACTIVO.
        bool MarcarFinalizada(Guid clientId, string modulo, int idResultado);

        // ACTIVO -> DESCARTADA; vacia el payload. false si no estaba ACTIVO.
        bool MarcarDescartada(int id);

        // Borra las FINALIZADAS de un modulo con mas de "dias" de antiguedad (solo sirven para idempotencia).
        int PurgarFinalizadasAntiguas(string modulo, int dias);

        // ===== Eventos de borrador (borradorgenericoevento, append-only) =====

        void AgregarEvento(Entidades.BorradorGenericoEvento evento);
        List<Entidades.BorradorGenericoEvento> ListarEventosPorBorrador(int idBorrador);

        // ===== Notificaciones (mismo criterio que IVentaBorradorRepository: sin repositorio separado) =====

        // Crea o actualiza la notificacion (idempresa, tipo, refid). Si ya existe actualiza titulo/
        // mensaje/severidad/actualizado y, con reabrir=true, la deja de nuevo como no atendida.
        void UpsertNotificacion(Entidades.Notificacion notificacion, bool reabrir);

        // Deteccion perezosa: crea BORRADOR_INTERRUMPIDO_<modulo> para los ACTIVA de ese modulo sin
        // latido reciente que todavia no tienen ninguna notificacion. Devuelve cuantas creo.
        int CrearNotificacionesInterrumpidas(string modulo, int minutosSinLatido);
    }
}
