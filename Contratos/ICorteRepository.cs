using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja el bloque CRUD/referencia de Datos.Corte (Corte, ActualizacionCorte,
    // CatalogoGlobalImportacionProductos, Formulas/CortePorFormula, AlicuotasIva, TiposProducto,
    // Etapa 6) mas Embutido (Etapa 11a) y Movimiento (Etapa 11b). Stock/Reportes (el resto de
    // Datos.Corte) queda fuera de esta interfaz -- se agrega en una etapa futura dedicada.
    // obtenerEmbutidos NO esta en esta interfaz a proposito: el SP real hace INNER JOIN contra
    // StockCorteSucursal (tabla obsoleta, 0 filas reales) y por eso siempre devuelve 0 filas en
    // SQL Server hoy, ademas de no tener ningun caller real (verificado por grep) -- codigo
    // muerto y ya roto, no se porta. Ver docs/DECISIONS.md.
    public interface ICorteRepository
    {
        List<Entidades.Corte> findAllCortes(bool buscarMaestro);
        List<Entidades.Corte> findAllCortesListado();
        Entidades.Corte findCorteById(int idCorte, bool buscarMaestro);
        Entidades.Corte findCorteByCodigo(long codigo, bool buscarMaestro);
        List<Entidades.Corte> ObtenerCortesPorEmpresa(int idEmpresa, bool buscarMaestro);
        List<Entidades.Corte> ObtenerCortesPorEmpresaListado(int idEmpresa);
        Entidades.Corte findCorteByCodigoEmpresa(long codigo, int idEmpresa, bool buscarMaestro);
        void editPrecioCorte(Entidades.Corte oCorteE);
        void addOrEditCorte(Entidades.Corte oCorteE);
        int InsertarCorteEnEmpresa(Entidades.Corte oCorteE);
        DataTable buscarCorte(string txtBusqueda);
        DataTable buscarCorteSinMaestro(string txtBusqueda);
        DataTable buscarCodigoCorte(long codigo);
        void eliminarCorte(Entidades.Corte oCorteE);
        DataTable obtenerCortes();
        DataTable cargarDtCortes();
        long sugerirCodigo(string tipo);
        int obtenerNivelCorte(int idCorteMaestro);

        void AsegurarTablaImportacionCatalogoGlobal();
        List<Entidades.CatalogoGlobalImportacionProducto> ObtenerImportacionesCatalogoGlobal(IEnumerable<int> idsProductosGlobales = null);
        void GuardarImportacionCatalogoGlobal(int idProductoGlobal, int idProductoEmpresa, int? idUsuarioAlta);

        DataTable buscarFormula(string texto);
        Entidades.Formula findFormulaByID(int idFormula, int idEmbutido);
        List<Entidades.CortePorFormula> cargarCortesPorFormula(Entidades.Formula oFormula);
        int existeFormula(int idEmbutido);
        int addOrEditFormula(Entidades.Formula oFormula, List<Entidades.CortePorFormula> listaCortesPorFormula);
        void eliminarFormula(int idFormula);
        DataTable getFormulaEmbutido(int idEmbutido);

        DataTable obtenerAlicuotasIva(bool mostrarTodos);
        Entidades.AlicuotaIva findAlicuotaIvaById(int idIva);

        DataTable obtenerTiposProducto(bool mostrarTodos);
        DataTable obtenerTiposProductoGrilla(string buscarText);
        DataTable obtenerTiposProductoGrillaEmpresa(string buscarText);
        DataTable obtenerTiposProductoCatalogoGlobal(string buscarText);
        string importarTiposProductoGlobales(IEnumerable<string> tiposProducto, int? idUsuarioAlta);
        string addOrEditTipoProducto(string tiposProducto, string orden, bool esInsert, string tipoToUpdate);
        string eliminarTipoProducto(string tiposProducto);

        DataTable getListaElegirEmbutido();
        DataTable buscarEmbutido(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta);
        DataTable obtenerUltimosElaboradosDashboard(int cantidad, int idSucursal, DateTime fechaDesde, DateTime fechaHasta);
        DataTable obtenerLineasEmb(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta);
        HashSet<int> ObtenerIdsEmbutidosIngresoRapido(IEnumerable<int> idsEmbutidos);
        DataTable obtenerInfoCorte(int idCorte);
        DataTable obtenerCorteProveedor(int idCorte);
        DataTable obtenerCortesPorProveedor(int idProveedor);
        Entidades.Embutido findEmbutidoById(int idEmbutido);
        int agregarEmbutido(Entidades.Embutido oEmbutido);
        void anularEmbutido(Entidades.Embutido oEmbutidoE);
        DataTable obtenerCortesPorEmbutidos(Entidades.Embutido oEmbutidoE);
        void agregarCortePorEmbutido(Entidades.CortePorEmbutido oCortePorEmbutido);

        int addOrEditMovimiento(Entidades.Movimiento oMovimientoE);
        void modificarMovimiento(Entidades.Movimiento oMovimientoE);
        void eliminarMovimiento(int idMovimiento, Entidades.Usuario oUsuario);
        void agregarCortePorMovimiento(Entidades.CortePorMovimiento cortePorMovimiento);
        void quitarCortesPorMovimiento(Entidades.Movimiento oMovimientoE);
        DataTable obtenerMovimientos(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto);
        DataTable obtenerUltimosMovimientosDashboard(int cantidad);
        Entidades.Movimiento cargarMovimiento(int idMovimiento, bool acumulado);
        List<Entidades.CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento, bool acumulado);
        Dictionary<int, Tuple<decimal, decimal>> ObtenerTotalesPorMovimiento(IEnumerable<int> idsMovimiento);
        DataTable obtenerLineasMov(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto);
    }
}
