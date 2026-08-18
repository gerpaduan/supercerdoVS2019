using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja SOLO el bloque CRUD/referencia de Datos.Corte (Corte, ActualizacionCorte,
    // CatalogoGlobalImportacionProductos, Formulas/CortePorFormula, AlicuotasIva, TiposProducto).
    // El resto de la clase (Embutido, Movimiento, cascade de StockCorteSucursal, reportes,
    // obtenerCorteProveedor/obtenerCortesPorProveedor) queda fuera de esta interfaz -- se agrega
    // en una etapa futura cuando se aborde ese bloque. Ver docs/DECISIONS.md, Etapa 6.
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
    }
}
