using System;
using System.Collections.Generic;
using System.Linq;

namespace NegocioTests.Fakes
{
    // Repositorio en memoria de comprobantes de compra (Compras: titular propio y comprobantes del proveedor, 2026-11-09).
    public sealed class FakeComprobanteCompraRepository : Contratos.IComprobanteCompraRepository
    {
        public readonly Dictionary<int, Entidades.Compra> Compras = new Dictionary<int, Entidades.Compra>();
        public readonly Dictionary<int, decimal> Totales = new Dictionary<int, decimal>();
        public readonly List<Entidades.ComprobanteCompra> Comprobantes = new List<Entidades.ComprobanteCompra>();
        // Titular que quedo en los movimientos de cuenta corriente de cada compra (SetTitularCompra).
        public readonly Dictionary<int, int?> TitularMovimientos = new Dictionary<int, int?>();
        private int ultimoId = 0;

        public Entidades.Compra AgregarCompra(int idCompra, int idProveedor, string tipoCompra, decimal total,
            string estadoFactura = Entidades.EstadoFacturaCompra.Pendiente, int? idTitular = null)
        {
            var compra = new Entidades.Compra
            {
                IdCompra = idCompra,
                Proveedor = new Entidades.Persona { idPersona = idProveedor },
                TipoCompra = tipoCompra,
                EstadoFactura = estadoFactura,
                IdTitular = idTitular,
                FechaCompra = new DateTime(2026, 11, 3),
                Estado = ""
            };
            Compras[idCompra] = compra;
            Totales[idCompra] = total;
            return compra;
        }

        public Entidades.Compra ObtenerCompraLiviana(int idCompra)
        {
            Entidades.Compra c;
            return Compras.TryGetValue(idCompra, out c) ? c : null;
        }

        public decimal ObtenerTotalCompra(int idCompra, string tipoCompra)
        {
            decimal t;
            return Totales.TryGetValue(idCompra, out t) ? t : 0m;
        }

        public List<Entidades.ComprobanteCompra> ListarPorCompra(int idCompra)
        {
            return Comprobantes.Where(c => c.IdCompra == idCompra && !c.Eliminado).OrderBy(c => c.IdComprobante).ToList();
        }

        public Entidades.ComprobanteCompra Obtener(int idComprobante)
        {
            return Comprobantes.FirstOrDefault(c => c.IdComprobante == idComprobante);
        }

        public int Guardar(Entidades.ComprobanteCompra comprobante, int idUsuario)
        {
            if (comprobante.IdComprobante == 0)
            {
                comprobante.IdComprobante = ++ultimoId;
                Comprobantes.Add(comprobante);
            }
            else
            {
                int i = Comprobantes.FindIndex(c => c.IdComprobante == comprobante.IdComprobante);
                Comprobantes[i] = comprobante;
            }
            return comprobante.IdComprobante;
        }

        public void Eliminar(int idComprobante, int idUsuario, string motivo)
        {
            Obtener(idComprobante).Eliminado = true;
        }

        public bool ExisteNumero(int idProveedor, string tipo, int ptoVta, long numero, int idComprobanteExcluir)
        {
            return Comprobantes.Any(c => !c.Eliminado && c.IdProveedor == idProveedor && c.TipoComprobante == tipo
                && c.PtoVta == ptoVta && c.Numero == numero && c.IdComprobante != idComprobanteExcluir);
        }

        public void SetEstadoFactura(int idCompra, string estado, int idUsuario)
        {
            Compras[idCompra].EstadoFactura = estado;
        }

        public void SetTitularCompra(int idCompra, int? idTitular, int idUsuario)
        {
            Compras[idCompra].IdTitular = idTitular;
            TitularMovimientos[idCompra] = idTitular;
        }

        public Dictionary<int, Entidades.InfoFacturaCompra> ObtenerInfoPorCompras(IEnumerable<int> idsCompra)
        {
            var r = new Dictionary<int, Entidades.InfoFacturaCompra>();
            foreach (int id in idsCompra.Distinct())
            {
                Entidades.Compra c;
                if (!Compras.TryGetValue(id, out c)) continue;
                var comprobantes = ListarPorCompra(id);
                r[id] = new Entidades.InfoFacturaCompra
                {
                    IdCompra = id,
                    IdTitular = c.IdTitular,
                    EstadoFactura = c.EstadoFactura,
                    FacturadoNeto = comprobantes.Sum(x => x.TotalConSigno)
                };
            }
            return r;
        }
    }
}
