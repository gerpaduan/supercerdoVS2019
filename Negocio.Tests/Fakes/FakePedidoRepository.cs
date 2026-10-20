using System;
using System.Collections.Generic;
using System.Linq;

namespace NegocioTests.Fakes
{
    // Repositorio de pedidos en memoria para probar las reglas de Negocio.Pedido sin base de datos. Guarda los pedidos y el historial,
    // asigna ids como la base y registra la ultima transaccion para verificar que se completo (o se descarto).
    public sealed class FakePedidoRepository : Contratos.IPedidoRepository
    {
        public List<Entidades.Pedido> Pedidos = new List<Entidades.Pedido>();
        public List<Entidades.PedidoEvento> Eventos = new List<Entidades.PedidoEvento>();
        public FakeUnitOfWork UltimaTransaccion;
        public int Transacciones;
        public bool FallarAlAgregar;
        private int _proximoId = 1;

        public Contratos.IUnitOfWork IniciarUnitOfWork()
        {
            Transacciones++;
            UltimaTransaccion = new FakeUnitOfWork();
            return UltimaTransaccion;
        }

        public int Agregar(Entidades.Pedido pedido, Contratos.IUnitOfWork unitOfWork)
        {
            if (FallarAlAgregar) throw new InvalidOperationException("falla simulada de la base");
            pedido.Id = _proximoId++;
            pedido.Creado = pedido.Fecha;
            Pedidos.Add(pedido);
            return pedido.Id;
        }

        public void Actualizar(Entidades.Pedido pedido, Contratos.IUnitOfWork unitOfWork)
        {
            var guardado = Pedidos.First(p => p.Id == pedido.Id);
            guardado.Nota = pedido.Nota;
            guardado.TotalEstimado = pedido.TotalEstimado;
            guardado.LineasSinPrecio = pedido.LineasSinPrecio;
            guardado.Lineas = pedido.Lineas;
            guardado.ModificadoPor = pedido.ModificadoPor;
            // La base fija "modificado" con now(): se simula con un instante nuevo para que la version cambie.
            guardado.Modificado = (guardado.Modificado ?? guardado.Creado ?? guardado.Fecha).AddSeconds(1);
        }

        public void AgregarEvento(Entidades.PedidoEvento evento, Contratos.IUnitOfWork unitOfWork)
        {
            Eventos.Add(evento);
        }

        public Entidades.Pedido ObtenerPorId(int idPedido)
        {
            var pedido = Pedidos.FirstOrDefault(p => p.Id == idPedido);
            if (pedido != null) pedido.Eventos = Eventos.Where(e => e.IdPedido == idPedido).ToList();
            return pedido;
        }

        public int? ObtenerIdPorClientId(Guid clientId)
        {
            var pedido = Pedidos.FirstOrDefault(p => p.ClientId == clientId);
            return pedido == null ? (int?)null : pedido.Id;
        }

        public List<Entidades.PedidoResumen> Listar(Entidades.PedidoFiltro filtro)
        {
            return new List<Entidades.PedidoResumen>();
        }
    }
}
