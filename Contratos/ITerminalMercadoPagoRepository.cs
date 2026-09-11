using System.Collections.Generic;

namespace Contratos
{
    public interface ITerminalMercadoPagoRepository
    {
        List<Entidades.TerminalMercadoPago> Listar(int idEmpresa);
        List<Entidades.TerminalMercadoPago> ListarPorSucursal(int idSucursal);
        void Agregar(Entidades.TerminalMercadoPago terminal);
        void Actualizar(Entidades.TerminalMercadoPago terminal);
        void Eliminar(int id, int idEmpresa);
    }
}
