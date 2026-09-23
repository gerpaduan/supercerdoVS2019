using System;
using System.Collections.Generic;

namespace Contratos
{
    // Ventas en curso del POS guardadas en el servidor, advertencias de "producto sin agregar" y
    // notificaciones al admin (ver docs/DECISIONS.md "Ventas en curso"). Solo Postgres: no hay
    // implementacion SQL Server (mismo criterio que IUsuarioPasskeyRepository). Todos los metodos
    // operan sobre la empresa del tenant con que se creo el repositorio (RLS).
    public interface IVentaBorradorRepository
    {
        // ===== Borradores (ventaborrador) =====

        // Upsert por clientId. Solo actualiza si el borrador esta ACTIVA y es del mismo operador y
        // sucursal; de lo contrario informa por que no lo hizo. Completa borrador.Id al guardar.
        Entidades.ResultadoGuardarBorrador Guardar(Entidades.VentaBorrador borrador);

        // Solo actualiza ultimolatido (el carrito no cambio). false si no existe, esta cerrada o es ajena.
        bool RegistrarLatido(Guid clientId, int idOperador, int idSucursal);

        // Con Payload incluido. null si no existe.
        Entidades.VentaBorrador ObtenerPorClientId(Guid clientId);
        Entidades.VentaBorrador ObtenerPorId(int id);

        // ACTIVAS (con Payload) de toda una sucursal, mas viejas primero.
        List<Entidades.VentaBorrador> ListarActivasPorSucursal(int idSucursal);

        // ACTIVAS (sin Payload) de un operador en una sucursal; las usa el cierre de caja.
        List<Entidades.VentaBorrador> ListarActivasPorOperador(int idOperador, int idSucursal);

        // ACTIVAS (sin Payload) de una cuenta de sesion en cualquier sucursal; las usa el logout.
        List<Entidades.VentaBorrador> ListarActivasPorUsuarioSesion(int idUsuarioSesion);

        // Traspasa una venta ACTIVA a un operador (recuperarla en su POS): actualiza operador, cuenta de
        // sesion y ultimo latido (queda "en uso"). false si no estaba ACTIVA.
        bool TomarBorrador(int id, int idOperador, int idUsuarioSesion);

        // ACTIVA -> FINALIZADA con el id de la venta real; vacia el payload. false si no estaba ACTIVA.
        bool MarcarFinalizada(Guid clientId, int idVenta);

        // ACTIVA -> DESCARTADA; vacia el payload. false si no estaba ACTIVA.
        bool MarcarDescartada(int id);

        // Borra las FINALIZADAS con mas de "dias" de antiguedad (solo sirven para idempotencia).
        int PurgarFinalizadasAntiguas(int dias);

        // ACTIVAS cuyo ultimo latido cae en [desde, hasta] y superan el umbral (para Actividades).
        List<Entidades.VentaBorrador> ListarInterrumpidasPorRango(DateTime desde, DateTime hasta, int minutosSinLatido);

        // ===== Eventos de borrador (ventaborradorevento, append-only) =====

        void AgregarEvento(Entidades.VentaBorradorEvento evento);
        List<Entidades.VentaBorradorEvento> ListarEventosPorBorrador(int idBorrador);
        List<Entidades.VentaBorradorEvento> ListarEventosPorRango(DateTime desde, DateTime hasta);

        // ===== Producto sin agregar (ventaproductosinagregar) =====

        // Inserta calculando Inicio/Fin con el reloj de la base a partir de SegundosDesdeSalida y
        // SegundosEnPantalla. Devuelve el id nuevo.
        int AgregarProductoSinAgregar(Entidades.ProductoSinAgregar producto);
        Entidades.ProductoSinAgregar ObtenerProductoSinAgregar(int id);
        List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarPorRango(DateTime desde, DateTime hasta);
        List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarDelDia(int idOperador, DateTime dia);
        Entidades.ResumenProductoSinAgregar ResumirProductoSinAgregar(int idOperador, DateTime desde, DateTime hasta);

        // Guarda la revision del admin (JUSTIFICADA / SOSPECHOSA). false si el id no existe.
        bool RevisarProductoSinAgregar(int id, string revision, string comentario, int idUsuario);

        // ===== Notificaciones =====

        // Crea o actualiza la notificacion (idempresa, tipo, refid). Si ya existe actualiza titulo/
        // mensaje/severidad/actualizado y, con reabrir=true, la deja de nuevo como no atendida.
        void UpsertNotificacion(Entidades.Notificacion notificacion, bool reabrir);

        // Las mas recientes primero; con soloPendientes solo las no atendidas.
        List<Entidades.Notificacion> ListarNotificaciones(bool soloPendientes, int max);
        int ContarNotificacionesPendientes();
        Entidades.Notificacion ObtenerNotificacion(int id);
        bool AtenderNotificacion(int id, int idUsuario);

        // Deteccion perezosa: crea VENTA_INTERRUMPIDA para las ventas ACTIVAS sin latido reciente
        // que todavia no tienen ninguna notificacion. Devuelve cuantas creo.
        int CrearNotificacionesVentasInterrumpidas(int minutosSinLatido);
    }
}
