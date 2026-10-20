using System;
using System.Collections.Generic;

namespace Contratos
{
    // Pedidos de compra (Entidades/Pedido.cs). Solo Postgres (ver Negocio/Pedido.cs). IniciarUnitOfWork: alta y modificacion
    // escriben cabecera + lineas + historial en UNA transaccion, igual que ILiquidacionSueldoRepository.
    public interface IPedidoRepository
    {
        IUnitOfWork IniciarUnitOfWork();

        // Inserta cabecera + lineas en la transaccion recibida. Devuelve el id nuevo (tambien lo deja en pedido.Id).
        int Agregar(Entidades.Pedido pedido, IUnitOfWork unitOfWork);

        // Actualiza la cabecera (nota, totales, modificado) y REEMPLAZA las lineas por las de pedido.Lineas.
        void Actualizar(Entidades.Pedido pedido, IUnitOfWork unitOfWork);

        // Agrega un renglon al historial (append-only).
        void AgregarEvento(Entidades.PedidoEvento evento, IUnitOfWork unitOfWork);

        // Cabecera + lineas + historial. Null si no existe (o es de otra empresa).
        Entidades.Pedido ObtenerPorId(int idPedido);

        // Id del pedido ya guardado con ese ClientId (idempotencia del guardado), o null.
        int? ObtenerIdPorClientId(Guid clientId);

        // Listado de resumen, lo mas reciente primero.
        List<Entidades.PedidoResumen> Listar(Entidades.PedidoFiltro filtro);
    }
}
