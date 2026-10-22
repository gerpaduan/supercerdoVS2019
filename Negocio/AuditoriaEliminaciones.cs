using System;
using System.Data;

namespace Negocio
{
    // Consulta de la auditoria de registros eliminados (Compras, Stock y Movimientos). La escritura la hacen los metodos que
    // eliminan (Negocio.Compra.eliminarCompra / Negocio.Corte.eliminarMovimientoConMotivo). Ver docs/DECISIONS.md
    // "Eliminar compras, stock y movimientos".
    public class AuditoriaEliminaciones
    {
        private readonly Contratos.IAuditoriaEliminacionRepository _repo;

        public AuditoriaEliminaciones(Contratos.IAuditoriaEliminacionRepository repositorio)
        {
            _repo = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public DataTable listar(string modulo, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            return _repo.listar(modulo ?? "", fechaDesde, fechaHasta, texto ?? "");
        }

        public Entidades.AuditoriaEliminacion obtener(int id)
        {
            return _repo.obtener(id);
        }

        public Entidades.AuditoriaEliminacion obtenerPorRegistro(string modulo, int idRegistro)
        {
            return _repo.obtenerPorRegistro(modulo, idRegistro);
        }
    }
}
