using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidades;
using Utilidades;

namespace Datos
{
    public class Corte
    {
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        public Corte(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa)); _param = param;
        }

        // =========================
        // MAPPER
        // =========================
        private Entidades.Corte MapCorte(SqlDataReader drCorte, bool cargarMaestro)
        {
            Entidades.Corte oCorteE = new Entidades.Corte();

            oCorteE.IdCorte = Convert.ToInt32(drCorte["idCorte"]);
            oCorteE.Codigo = Convert.ToInt64(drCorte["codigo"]);
            oCorteE.CorteDesc = Convert.ToString(drCorte["corte"]);

            
            if (drCorte["idMarca"] != DBNull.Value)
            {
                //TODO: Nota: esto abre otra consulta por cada corte.
                // Si querés performance, después lo cambiamos a JOIN.
                Datos.Persona oPersonaD = new Datos.Persona(_empresa, _param);
                oCorteE.Marca = oPersonaD.findById(Convert.ToInt32(drCorte["idMarca"]));            
            }

            oCorteE.Tipo = Convert.ToString(drCorte["tipo"]);
            oCorteE.Promedio = float.Parse(drCorte["promedio"].ToString());
            oCorteE.PuntoStock = Convert.ToInt32(drCorte["puntoStock"]);
            oCorteE.Nivel = Convert.ToInt32(drCorte["nivel"]);

            if (cargarMaestro)
            {
                int idMaestro = drCorte["idCorteMaestro"] == DBNull.Value ? 0 : Convert.ToInt32(drCorte["idCorteMaestro"]);
                oCorteE.CorteMaestro = (idMaestro > 0) ? findCorteById(idMaestro, false) : null;
            }

            oCorteE.Porcentaje = float.Parse(drCorte["porcentaje"].ToString());
            oCorteE.PrecioKg = float.Parse(drCorte["precioKg"].ToString());
            oCorteE.PrecioKgReferencia = float.Parse(drCorte["precioKg"].ToString());
            oCorteE.IngresoRapidoEmbutido = Convert.ToBoolean(drCorte["ingresoRapidoEmbutido"]);
            oCorteE.Habilitado = Convert.ToBoolean(drCorte["habilitado"]);
            oCorteE.EnCierreStock = Convert.ToBoolean(drCorte["enCierreStock"]);
            oCorteE.PorcentajeHueso = float.Parse(drCorte["porcentajeHueso"].ToString());
            oCorteE.Independiente = Convert.ToInt32(drCorte["independiente"]);
            oCorteE.DesvioEstandar = float.Parse(drCorte["desvioEstandar"].ToString());

            oCorteE.Creado = Convert.ToDateTime(drCorte["creado"]);
            oCorteE.Actualizado = drCorte["actualizado"] == DBNull.Value ? null : (DateTime?)drCorte["actualizado"];

            oCorteE.IdAlicuotaIva = Convert.ToInt32(drCorte["idAlicuotaIva"]);
            oCorteE.AlicuotaIva = float.Parse(drCorte["alicuotaIva"].ToString());
            oCorteE.Pesable = Convert.ToBoolean(drCorte["pesable"]);

            // Presentación
            oCorteE.Presentacion = oCorteE.EsPresentacion(oCorteE.porcentajeHueso);
            if (oCorteE.Presentacion)
            {
                oCorteE.porcentaje = oCorteE.getCantPresentacion(oCorteE.porcentajeHueso);
            }

            return oCorteE;
        }

        // =========================
        // CORTES
        // =========================
        public List<Entidades.Corte> findAllCortes(bool buscarMaestro)
        {
            return Db.Reader(
                _empresa,
                "SELECT * FROM Corte ORDER BY codigo ASC",
                CommandType.Text,
                dr => MapCorte(dr, buscarMaestro)
            );
        }

        public Entidades.Corte findCorteById(int idCorte, bool buscarMaestro)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT * FROM Corte WHERE idCorte = @idCorte",
                CommandType.Text,
                dr => MapCorte(dr, buscarMaestro),
                p => p.Add("@idCorte", SqlDbType.Int).Value = idCorte
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.Corte findCorteByCodigo(long codigo, bool buscarMaestro)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT * FROM Corte WHERE codigo = @codigo",
                CommandType.Text,
                dr => MapCorte(dr, buscarMaestro),
                p => p.Add("@codigo", SqlDbType.BigInt).Value = codigo
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        public void editPrecioCorte(Entidades.Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            Db.NonQuery(
                _empresa,
                "UPDATE Corte SET precioKg = @precioKg WHERE idCorte = @idCorte",
                CommandType.Text,
                p =>
                {
                    p.AddWithValue("@precioKg", oCorteE.precioKg);
                    p.AddWithValue("@idCorte", oCorteE.idCorte);
                }
            );
        }

        public void addOrEditCorte(Entidades.Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            Db.NonQuery(
                _empresa,
                "addOrEditCorte",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idCorte", oCorteE.idCorte);
                    p.AddWithValue("@codigo", oCorteE.codigo);
                    p.AddWithValue("@corte", oCorteE.corte ?? "");
                    p.AddWithValue("@tipo", oCorteE.tipo ?? "");
                    p.AddWithValue("@idMarca", oCorteE.Marca != null ? oCorteE.Marca.IdPersona : 0);
                    p.AddWithValue("@puntoStock", oCorteE.PuntoStock);
                    p.AddWithValue("@promedio", oCorteE.Promedio);
                    p.AddWithValue("@independiente", oCorteE.independiente);
                    p.AddWithValue("@precioKg", oCorteE.precioKg);
                    p.AddWithValue("@ingresoRapidoEmbutido", oCorteE.IngresoRapidoEmbutido);
                    p.AddWithValue("@habilitado", oCorteE.Habilitado);
                    p.AddWithValue("@enCierreStock", oCorteE.EnCierreStock);
                    p.AddWithValue("@idCorteMaestro", oCorteE.corteMaestro != null ? oCorteE.corteMaestro.idCorte : 0);
                    p.AddWithValue("@porcentaje", oCorteE.porcentaje);
                    p.AddWithValue("@porcentajeHueso", oCorteE.porcentajeHueso);
                    p.AddWithValue("@desvioEstandar", oCorteE.desvioEstandar);
                    p.AddWithValue("@idAlicuotaIva", oCorteE.IdAlicuotaIva);
                    p.AddWithValue("@alicuotaIva", oCorteE.AlicuotaIva);
                    p.AddWithValue("@pesable", oCorteE.Pesable);
                }
            );
        }

        public DataTable buscarCorte(string txtBusqueda)
        {
            return Db.DataTable(
                _empresa,
                "buscarCorte",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@texto", txtBusqueda ?? "")
            );
        }

        public DataTable buscarCorteSinMaestro(string txtBusqueda)
        {
            return Db.DataTable(
                _empresa,
                "buscarCorteSinMaestro",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@texto", txtBusqueda ?? "")
            );
        }

        public DataTable buscarCodigoCorte(long codigo)
        {
            return Db.DataTable(
                _empresa,
                "buscarCodigoCorte",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@codigo", codigo)
            );
        }

        public void eliminarCorte(Entidades.Corte oCorteE)
        {
            if (oCorteE == null) throw new ArgumentNullException(nameof(oCorteE));

            Db.NonQuery(
                _empresa,
                "EliminarCorte",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idCorte", oCorteE.idCorte)
            );
        }

        public DataTable obtenerCortes()
        {
            string sql =
                "SELECT CorteP.idCorte, CorteP.codigo, CorteP.corte, CorteP.precioKg, CorteP.ingresoRapidoEmbutido, CorteP.enCierreStock, " +
                "CorteP.tipo, CorteP.pesable, CorteP.nivel, CorteP.idCorteMaestro, CorteM.corte AS corteMaestro, CorteP.porcentaje, CorteP.porcentajeHueso, CorteP.desvioEstandar, " +
                "CorteP.independiente, CorteP.promedio, CorteP.idAlicuotaIva, CorteP.alicuotaIva " +
                "FROM dbo.Corte AS CorteM RIGHT OUTER JOIN dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro";

            return Db.DataTable(_empresa, sql, CommandType.Text);
        }

        public DataTable cargarDtCortes()
        {
            return Db.DataTable(_empresa, "SELECT * FROM Corte", CommandType.Text);
        }

        public DataTable obtenerEmbutidos(string txtBusqueda)
        {
            return Db.DataTable(
                _empresa,
                "obtenerEmbutidos",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@texto", txtBusqueda ?? "")
            );
        }

        public DataTable getListaElegirEmbutido()
        {
            return Db.DataTable(_empresa, "getListaElegirEmbutido", CommandType.StoredProcedure);
        }

        public DataTable buscarEmbutido(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return Db.DataTable(
                _empresa,
                "buscarEmbutido",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                }
            );
        }

        public DataTable obtenerLineasEmb(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return Db.DataTable(
                _empresa,
                "obtenerLineasEmb",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                }
            );
        }

        public DataTable obtenerInfoCorte(int idCorte)
        {
            return Db.DataTable(
                _empresa,
                "obtenerInfoCorte",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idCorte", idCorte)
            );
        }

        public DataTable obtenerCorteProveedor(int idCorte)
        {
            string sql =
                "SELECT dbo.Personas.razonSocial, dbo.CorteProveedor.ultimoPrecio, dbo.CorteProveedor.fechaUltimaCompra " +
                "FROM dbo.Corte " +
                "INNER JOIN dbo.CorteProveedor ON dbo.Corte.idCorte = dbo.CorteProveedor.idCorte " +
                "INNER JOIN dbo.Personas ON dbo.CorteProveedor.idProveedor = dbo.Personas.idPersona " +
                "WHERE dbo.CorteProveedor.idCorte = @idCorte " +
                "ORDER BY dbo.CorteProveedor.fechaUltimaCompra DESC";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p => p.AddWithValue("@idCorte", idCorte)
            );
        }

        public DataTable obtenerCortesPorProveedor(int idProveedor)
        {
            const string sql = @"
                SELECT DISTINCT cpc.idCorte
                FROM dbo.Compras c
                INNER JOIN dbo.CortePorCompra cpc ON cpc.idCompra = c.idCompra
                WHERE c.idProveedor = @idProveedor
                  AND ISNULL(c.estado, '') = '';";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p => p.AddWithValue("@idProveedor", idProveedor)
            );
        }

        // =========================
        // FORMULAS
        // =========================
        public DataTable buscarFormula(string texto)
        {
            string sql =
                "SELECT DISTINCT F.idFormula, C.codigo, C.corte, F.creado, F.actualizado " +
                "FROM dbo.Corte C " +
                "INNER JOIN dbo.Formulas F ON C.idCorte = F.idEmbutido " +
                "WHERE C.corte LIKE @texto " +
                "ORDER BY C.codigo";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p => p.AddWithValue("@texto", "%" + (texto ?? "") + "%")
            );
        }

        public Entidades.Formula findFormulaByID(int idFormula, int idEmbutido)
        {
            string sql = idFormula > 0
                ? "SELECT * FROM Formulas WHERE idFormula = @idFormula"
                : "SELECT * FROM Formulas WHERE idEmbutido = @idEmbutido";

            var lista = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr =>
                {
                    var oFormula = new Entidades.Formula();

                    oFormula.IdFormula = Convert.ToInt32(dr["idFormula"]);
                    oFormula.Embutido = findCorteById(Convert.ToInt32(dr["idEmbutido"]), false);
                    oFormula.Receta = dr["receta"].ToString();

                    oFormula.Creado = Convert.ToDateTime(dr["creado"]);
                    oFormula.Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)dr["actualizado"];

                    Datos.Usuario oUsuarioD = new Usuario(_empresa);
                    oFormula.CreadoPor = string.IsNullOrEmpty(dr["creadoPor"].ToString())
                        ? null
                        : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["creadoPor"]));
                    oFormula.ActualizadoPor = string.IsNullOrEmpty(dr["actualizadoPor"].ToString())
                        ? null
                        : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["actualizadoPor"]));

                    return oFormula;
                },
                p =>
                {
                    if (idFormula > 0)
                        p.Add("@idFormula", SqlDbType.Int).Value = idFormula;
                    else
                        p.Add("@idEmbutido", SqlDbType.Int).Value = idEmbutido;
                }
            );

            if (lista.Count == 0) return null;

            var formula = lista[0];
            formula.ListaCortesEnFormula = cargarCortesPorFormula(formula);
            return formula;
        }

        public List<Entidades.CortePorFormula> cargarCortesPorFormula(Entidades.Formula oFormula)
        {
            if (oFormula == null) throw new ArgumentNullException(nameof(oFormula));

            return Db.Reader(
                _empresa,
                "SELECT * FROM CortePorFormula WHERE idFormula = @idFormula",
                CommandType.Text,
                dr =>
                {
                    Entidades.CortePorFormula item = new Entidades.CortePorFormula();
                    item.IdCorteEnFormula = Convert.ToInt32(dr["idCortePorFormula"]);
                    item.Formula = oFormula;
                    item.CorteEnFormula = findCorteById(Convert.ToInt32(dr["idCorte"]), false);
                    item.Porcentaje = float.Parse(dr["porcentaje"].ToString());
                    item.AgregarAuto = Convert.ToBoolean(dr["agregarAuto"]);
                    return item;
                },
                p => p.Add("@idFormula", SqlDbType.Int).Value = oFormula.IdFormula
            );
        }

        public int existeFormula(int idEmbutido)
        {
            object obj = Db.Scalar(
                _empresa,
                "SELECT idFormula FROM Formulas WHERE idEmbutido = @idEmbutido",
                CommandType.Text,
                p => p.Add("@idEmbutido", SqlDbType.Int).Value = idEmbutido
            );

            return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
        }

        public int addOrEditFormula(Entidades.Formula oFormula, List<Entidades.CortePorFormula> listaCortesPorFormula)
        {
            if (oFormula == null) throw new ArgumentNullException(nameof(oFormula));
            if (listaCortesPorFormula == null) listaCortesPorFormula = new List<Entidades.CortePorFormula>();

            // 1) Guardar cabecera
            object obj = Db.Scalar(
                _empresa,
                "addOrEditFormula",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idFormula", oFormula.IdFormula);
                    p.AddWithValue("@idEmbutido", oFormula.Embutido.idCorte);
                    p.AddWithValue("@receta", oFormula.Receta ?? "");
                    p.AddWithValue("@creadoPor", oFormula.CreadoPor.Id);
                    p.AddWithValue("@actualizadoPor", oFormula.ActualizadoPor != null ? oFormula.ActualizadoPor.Id : 0);
                }
            );

            oFormula.IdFormula = (obj == null || obj == DBNull.Value) ? oFormula.IdFormula : Convert.ToInt32(obj);

            // 2) Insertar detalle (si tu SP NO borra antes, vas a duplicar)
            // Si tu lógica requiere "borrar y reinsertar", decime y lo hacemos acá.
            foreach (var item in listaCortesPorFormula)
            {
                Db.NonQuery(
                    _empresa,
                    "agregarCortePorFormula",
                    CommandType.StoredProcedure,
                    p =>
                    {
                        p.AddWithValue("@idFormula", oFormula.IdFormula);
                        p.AddWithValue("@idCorte", item.CorteEnFormula1.idCorte);
                        p.AddWithValue("@porcentaje", item.Porcentaje);
                        p.AddWithValue("@agregarAuto", item.AgregarAuto);
                    }
                );
            }

            return oFormula.IdFormula;
        }

        public void eliminarFormula(int idFormula)
        {
            // Mantengo tu lógica en 2 sentencias
            Db.NonQuery(
                _empresa,
                "DELETE FROM CortePorFormula WHERE idFormula = @idFormula",
                CommandType.Text,
                p => p.Add("@idFormula", SqlDbType.Int).Value = idFormula
            );

            Db.NonQuery(
                _empresa,
                "DELETE FROM Formulas WHERE idFormula = @idFormula",
                CommandType.Text,
                p => p.Add("@idFormula", SqlDbType.Int).Value = idFormula
            );
        }

        public DataTable getFormulaEmbutido(int idEmbutido)
        {
            string sql =
                "SELECT C.idCorte, C.codigo, C.corte, CPF.porcentaje, '' AS kgs, CPF.agregarAuto " +
                "FROM dbo.Formulas F " +
                "INNER JOIN dbo.CortePorFormula CPF ON F.idFormula = CPF.idFormula " +
                "INNER JOIN dbo.Corte C ON CPF.idCorte = C.idCorte " +
                "WHERE F.idEmbutido = @idEmbutido " +
                "ORDER BY CPF.agregarAuto DESC";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p => p.AddWithValue("@idEmbutido", idEmbutido)
            );
        }

        public DataTable obtenerTiposProducto(bool mostrarTodos)
        {
            string sql = "SELECT tipo FROM TiposProducto";
            sql += mostrarTodos ? " ORDER BY orden, tipo" : " WHERE orden > 0 ORDER BY orden, tipo";

            return Db.DataTable(_empresa, sql, CommandType.Text);
        }

        // =========================
        // ALICUOTAS IVA
        // =========================
        public DataTable obtenerAlicuotasIva(bool mostrarTodos)
        {
            string sql = "SELECT idIva, iva FROM AlicuotasIva";
            sql += mostrarTodos ? " ORDER BY orden" : " WHERE mostrar = 1 ORDER BY orden";

            return Db.DataTable(_empresa, sql, CommandType.Text);
        }

        public Entidades.AlicuotaIva findAlicuotaIvaById(int idIva)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT * FROM AlicuotasIva WHERE idIva = @idIva",
                CommandType.Text,
                dr =>
                {
                    var o = new Entidades.AlicuotaIva();
                    o.IdIva = Convert.ToInt32(dr["idIva"]);
                    o.Iva = Convert.ToInt32(dr["iva"]);
                    o.Orden = Convert.ToInt32(dr["orden"]);
                    o.Mostrar = Convert.ToBoolean(dr["mostrar"]);
                    return o;
                },
                p => p.Add("@idIva", SqlDbType.Int).Value = idIva
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        // =========================
        // EMBUTIDOS
        // =========================
        public Entidades.Embutido findEmbutidoById(int idEmbutido)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT * FROM Embutidos WHERE idEmbutido = @idEmbutido",
                CommandType.Text,
                dr =>
                {
                    var o = new Entidades.Embutido();
                    o.IdEmbutido = Convert.ToInt32(dr["idEmbutido"]);
                    o.FechaEmbutido = Convert.ToDateTime(dr["fechaEmbutido"]);
                    o.Corte = findCorteById(Convert.ToInt32(dr["idCorte"]), true);

                    Datos.Sucursal oSucursalD = new Sucursal(_empresa);
                    o.Sucursal = oSucursalD.findById(Convert.ToInt32(dr["idSucursal"]));

                    o.Observaciones = Convert.ToString(dr["observaciones"]);
                    o.Estado = Convert.ToString(dr["estado"]);
                    o.Creado = Convert.ToDateTime(dr["creado"]);
                    o.Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)dr["actualizado"];

                    Datos.Usuario oUsuarioD = new Usuario(_empresa);
                    o.CreadoPor = string.IsNullOrEmpty(dr["creadoPor"].ToString())
                        ? null
                        : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["creadoPor"]));
                    o.ActualizadoPor = string.IsNullOrEmpty(dr["actualizadoPor"].ToString())
                        ? null
                        : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["actualizadoPor"]));

                    return o;
                },
                p => p.Add("@idEmbutido", SqlDbType.Int).Value = idEmbutido
            );

            if (lista.Count == 0) return null;

            var emb = lista[0];
            emb.CortesEnEmbutido = obtenerCortesEnEmbutido(emb);
            return emb;
        }

        public List<Entidades.CortePorEmbutido> obtenerCortesEnEmbutido(Entidades.Embutido oEmbutidoParam)
        {
            if (oEmbutidoParam == null) throw new ArgumentNullException(nameof(oEmbutidoParam));

            return Db.Reader(
                _empresa,
                "SELECT * FROM CortePorEmbutido WHERE idEmbutido = @idEmbutido",
                CommandType.Text,
                dr =>
                {
                    Entidades.CortePorEmbutido item = new Entidades.CortePorEmbutido();
                    item.IdCorteEmbutido = Convert.ToInt32(dr["idCorteEmbutido"]);
                    item.Embutido = oEmbutidoParam;
                    item.Corte = findCorteById(Convert.ToInt32(dr["idCorte"]), false);
                    item.KgUtilizado = float.Parse(dr["kgUtilizados"].ToString());
                    item.PesoBalanza = Convert.ToBoolean(dr["pesoBalanza"]);
                    return item;
                },
                p => p.Add("@idEmbutido", SqlDbType.Int).Value = oEmbutidoParam.idEmbutido
            );
        }

        public int agregarEmbutido(Entidades.Embutido oEmbutido)
        {
            if (oEmbutido == null) throw new ArgumentNullException(nameof(oEmbutido));

            object obj = Db.Scalar(
                _empresa,
                "agregarEmbutido",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@fechaEmbutido", oEmbutido.fechaEmbutido);
                    p.AddWithValue("@idCorte", oEmbutido.corte.idCorte);
                    p.AddWithValue("@idSucursal", oEmbutido.sucursal.IdSucursal);
                    p.AddWithValue("@creadoPor", oEmbutido.CreadoPor.Id);
                    p.AddWithValue("@observaciones", oEmbutido.observaciones ?? "");
                }
            );

            return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
        }

        public void anularEmbutido(Entidades.Embutido oEmbutidoE)
        {
            if (oEmbutidoE == null) throw new ArgumentNullException(nameof(oEmbutidoE));

            Db.NonQuery(
                _empresa,
                "anularEmbutido",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idEmbutido", oEmbutidoE.idEmbutido);
                    p.AddWithValue("@actualizadoPor", oEmbutidoE.ActualizadoPor.Id);
                }
            );
        }

        public DataTable obtenerCortesPorEmbutidos(Entidades.Embutido oEmbutidoE)
        {
            return Db.DataTable(
                _empresa,
                "obtenerCortesPorEmbutidos",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idEmbutido", oEmbutidoE.idEmbutido)
            );
        }

        public void agregarCortePorEmbutido(Entidades.CortePorEmbutido oCortePorEmbutido)
        {
            if (oCortePorEmbutido == null) throw new ArgumentNullException(nameof(oCortePorEmbutido));

            Db.NonQuery(
                _empresa,
                "agregarCortePorEmbutido",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idEmbutido", oCortePorEmbutido.embutido.idEmbutido);
                    p.AddWithValue("@idCorte", oCortePorEmbutido.corte.idCorte);
                    p.AddWithValue("@kgUtilizados", oCortePorEmbutido.kgUtilizado);
                    p.AddWithValue("@idSucursal", oCortePorEmbutido.embutido.sucursal.IdSucursal);
                    p.AddWithValue("@pesoBalanza", oCortePorEmbutido.PesoBalanza);
                }
            );
        }

        public void actualizarStockEmbutido(DataRow cortePorEmbutido, Entidades.Embutido oEmbutidoE)
        {
            if (cortePorEmbutido == null) throw new ArgumentNullException(nameof(cortePorEmbutido));
            if (oEmbutidoE == null) throw new ArgumentNullException(nameof(oEmbutidoE));

            Db.NonQuery(
                _empresa,
                "actualizarStockEmbutido",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idEmbutido", cortePorEmbutido["idEmbutido"]);
                    p.AddWithValue("@idCorte", cortePorEmbutido["idCorte"]);
                    p.AddWithValue("@kgUtilizados", cortePorEmbutido["kgUtilizados"]);
                    p.AddWithValue("@idSucursal", oEmbutidoE.sucursal.idSucursal);
                }
            );
        }

        // =========================
        // MOVIMIENTO
        // =========================
        public int addOrEditMovimiento(Entidades.Movimiento oMovimientoE)
        {
            if (oMovimientoE == null) throw new ArgumentNullException(nameof(oMovimientoE));

            // Si es alta, tu SP devuelve idMovimiento (por reader antes). Ahora lo pedimos por Scalar.
            if (oMovimientoE.IdMovimiento == 0)
            {
                object obj = Db.Scalar(
                    _empresa,
                    "addOrEditMovimiento",
                    CommandType.StoredProcedure,
                    p =>
                    {
                        p.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
                        p.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
                        p.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
                        p.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
                        p.AddWithValue("@observaciones", oMovimientoE.Observaciones ?? "");
                        p.AddWithValue("@creadoPor", oMovimientoE.CreadoPor.Id);
                    }
                );

                oMovimientoE.IdMovimiento = (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
                return oMovimientoE.IdMovimiento;
            }

            // Si es edición, no devuelve id: solo update.
            Db.NonQuery(
                _empresa,
                "addOrEditMovimiento",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
                    p.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
                    p.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
                    p.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
                    p.AddWithValue("@observaciones", oMovimientoE.Observaciones ?? "");
                    p.AddWithValue("@creadoPor", oMovimientoE.CreadoPor.Id);
                    p.AddWithValue("@actualizadoPor", oMovimientoE.ActualizadoPor.Id);
                }
            );

            return oMovimientoE.IdMovimiento;
        }

        public void modificarMovimiento(Entidades.Movimiento oMovimientoE)
        {
            if (oMovimientoE == null) throw new ArgumentNullException(nameof(oMovimientoE));

            Db.NonQuery(
                _empresa,
                "modificarMovimiento",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
                    p.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
                    p.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
                    p.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
                    p.AddWithValue("@observaciones", oMovimientoE.Observaciones ?? "");
                    p.AddWithValue("@actualizadoPor", oMovimientoE.ActualizadoPor.Id);
                }
            );
        }

        public void eliminarMovimiento(int idMovimiento, Entidades.Usuario oUsuario)
        {
            Db.NonQuery(
                _empresa,
                "eliminarMovimiento",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idMovimiento", idMovimiento);
                    p.AddWithValue("@actualizadoPor", oUsuario.Id);
                }
            );
        }

        public void agregarCortePorMovimiento(Entidades.CortePorMovimiento cortePorMovimiento)
        {
            if (cortePorMovimiento == null) throw new ArgumentNullException(nameof(cortePorMovimiento));

            Db.NonQuery(
                _empresa,
                "agregarCortePorMovimiento",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idMovimiento", cortePorMovimiento.Movimientos.IdMovimiento);
                    p.AddWithValue("@idCorte", cortePorMovimiento.Corte.IdCorte);
                    p.AddWithValue("@cantKg", cortePorMovimiento.CantKg);
                    p.AddWithValue("@cantUnidad", cortePorMovimiento.CantUnidad);
                    p.AddWithValue("@pesoBalanza", cortePorMovimiento.PesoBalanza);
                    p.AddWithValue("@permitirIngreso", cortePorMovimiento.PermitirIngreso);
                }
            );
        }

        public void quitarCortesPorMovimiento(Entidades.Movimiento oMovimientoE)
        {
            Db.NonQuery(
                _empresa,
                "quitarCortesPorMovimiento",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento)
            );
        }

        public DataTable obtenerMovimientos(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            return Db.DataTable(
                _empresa,
                "obtenerMovimientos",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@sucOrigen", sucOrigen ?? "");
                    p.AddWithValue("@sucDestino", sucDestino ?? "");
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@texto", texto ?? "");
                }
            );
        }

        public Entidades.Movimiento cargarMovimiento(int idMovimiento, bool acumulado)
        {
            var lista = Db.Reader(
                _empresa,
                "cargarMovimiento",
                CommandType.StoredProcedure,
                dr =>
                {
                    Entidades.Movimiento oMovimiento = new Entidades.Movimiento();

                    oMovimiento.IdMovimiento = Convert.ToInt32(dr["idMovimiento"]);
                    oMovimiento.FechaMovimiento = Convert.ToDateTime(dr["fechaMovimiento"]);

                    Entidades.Sucursal origen = new Entidades.Sucursal();
                    origen.idSucursal = Convert.ToInt32(dr["idOrigen"]);
                    origen.sucursal = dr["origen"].ToString();
                    oMovimiento.SucursalOrigen = origen;

                    oMovimiento.IdMovOrigen = string.IsNullOrEmpty(dr["idMovOrigen"].ToString()) ? 0 : Convert.ToInt32(dr["idMovOrigen"]);

                    Entidades.Sucursal destino = new Entidades.Sucursal();
                    destino.idSucursal = Convert.ToInt32(dr["idDestino"]);
                    destino.sucursal = dr["destino"].ToString();
                    oMovimiento.SucursalDestino = destino;

                    oMovimiento.Observaciones = dr["observaciones"].ToString();

                    oMovimiento.Creado = Convert.ToDateTime(dr["creado"]);
                    oMovimiento.Actualizado = dr["actualizado"] == DBNull.Value ? null : (DateTime?)dr["actualizado"];

                    Datos.Usuario oUsuarioD = new Usuario(_empresa);
                    oMovimiento.CreadoPor = string.IsNullOrEmpty(dr["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["creadoPor"]));
                    oMovimiento.ActualizadoPor = string.IsNullOrEmpty(dr["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(dr["actualizadoPor"]));

                    return oMovimiento;
                },
                p => p.AddWithValue("@idMovimiento", idMovimiento)
            );

            if (lista.Count == 0) return new Entidades.Movimiento();

            var mov = lista[0];
            mov.ListaCortesPorMov = cargarCortesPorMovimiento(mov.IdMovimiento, acumulado);
            return mov;
        }

        public List<Entidades.CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento, bool acumulado)
        {
            return Db.Reader(
                _empresa,
                "cargarCortesPorMovimiento",
                CommandType.StoredProcedure,
                dr =>
                {
                    Entidades.CortePorMovimiento item = new Entidades.CortePorMovimiento();
                    item.IdCorteMovimiento = Convert.ToInt32(dr["idCorteMovimiento"]);

                    Entidades.Corte corte = new Entidades.Corte();
                    corte.idCorte = Convert.ToInt32(dr["idCorte"]);
                    corte.codigo = Convert.ToInt64(dr["codigo"]);
                    corte.corte = dr["corte"].ToString();
                    item.Corte = corte;

                    item.CantKg = float.Parse(dr["cantKg"].ToString());
                    item.CantUnidad = Convert.ToInt32(dr["cantUnidad"]);

                    if (!acumulado)
                    {
                        item.PesoBalanza = dr["pesoBalanza"] == DBNull.Value ? false : Convert.ToBoolean(dr["pesoBalanza"]);
                        item.PermitirIngreso = dr["permitirIngreso"] == DBNull.Value ? false : Convert.ToBoolean(dr["permitirIngreso"]);
                    }

                    return item;
                },
                p =>
                {
                    p.AddWithValue("@idMovimiento", idMovimiento);
                    p.AddWithValue("@acumulado", acumulado);
                }
            );
        }

        public DataTable obtenerLineasMov(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            return Db.DataTable(
                _empresa,
                "obtenerLineasMov",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@sucOrigen", sucOrigen ?? "");
                    p.AddWithValue("@sucDestino", sucDestino ?? "");
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@texto", texto ?? "");
                }
            );
        }

        public void reiniciarStockReal(int idSucursal)
        {
            Db.NonQuery(
                _empresa,
                "reiniciarStock",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idSucursal", idSucursal)
            );
        }

        public void reiniciarStockTeorico(int idSucursal)
        {
            Db.NonQuery(
                _empresa,
                "reiniciarStockTeorico",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idSucursal", idSucursal)
            );
        }

        public DataTable reporteTeoricoReal(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            return Db.DataTable(
                _empresa,
                "StockTeoricoReal",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                }
            );
        }

        public DateTime fechaUltimoCierreStock_Sucursal(int idSucursal)
        {
            string sql =
                "SELECT TOP 1 fechaCompra " +
                "FROM Compras " +
                "WHERE tipoCompra = 'Cierre Stock' AND idSucursal = @idSucursal " +
                "ORDER BY fechaCompra DESC";

            object obj = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                p => p.AddWithValue("@idSucursal", idSucursal)
            );

            return (obj == null || obj == DBNull.Value) ? DateTime.MinValue : Convert.ToDateTime(obj);
        }

        public DataTable CierreStock(
                int nroCierre,
                string texto,
                int idSucursal,
                DateTime fechaDesde,
                DateTime fechaHasta,
                string conexionSucursal,
                string tipo,
                int idProveedor,
                int idMarca)
        {
            string sp = (nroCierre == 2) ? "StockCierre_2" : "a_CierreStock";

            return Db.DataTable(
                _empresa,
                sp,
                CommandType.StoredProcedure,
                setParams: p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@tipo", tipo ?? "");
                    p.AddWithValue("@idProveedor", idProveedor);
                    p.AddWithValue("@idMarca", idMarca);
                },
                conexionSucursal: string.IsNullOrWhiteSpace(conexionSucursal) ? null : conexionSucursal
            );
        }

        public DataTable acum_Ventas(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
        {
            return Db.DataTable(
                _empresa,
                "Acum_Ventas",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idEmpresa", _empresa.IdEmpresa);
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@tipo", tipo ?? "");
                    p.AddWithValue("@idProveedor", idProveedor);
                    p.AddWithValue("@idMarca", idMarca);
                }
            );
        }

        public DataTable StockIngresoEgreso(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            return Db.DataTable(
                _empresa,
                "StockIngresoEgreso",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                }
            );
        }

        public DataTable TotalPorCortesVendidos(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
        {
            return Db.DataTable(
                _empresa,
                "TotalPorCortesVendidos",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@tipo", tipo ?? "");
                    p.AddWithValue("@idProveedor", idProveedor);
                    p.AddWithValue("@idMarca", idMarca);
                }
            );
        }

        public DataTable ObtenerSerieVentasPorCorte(int idCorte, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idMarca, string agrupacionTemporal)
        {
            const string sql = @"
                SELECT
                    CASE
                        WHEN @agrupacionTemporal = 'dia'
                            THEN DATEADD(day, DATEDIFF(day, 0, v.fechaVenta), 0)
                        ELSE DATEADD(hour, DATEDIFF(hour, 0, v.fechaVenta), 0)
                    END AS BucketFecha,
                    SUM(ISNULL(lv.cantKg, 0) - ISNULL(lv.kgsAjusteTarj, 0)) AS TotalKg,
                    SUM((ISNULL(lv.cantKg, 0) - ISNULL(lv.kgsAjusteTarj, 0)) * ISNULL(lv.precioKg, 0)) AS TotalImporte
                FROM dbo.Ventas v
                INNER JOIN dbo.LineaVenta lv
                    ON lv.idVenta = v.idVenta
                INNER JOIN dbo.Corte c
                    ON c.idCorte = lv.idCorte
                WHERE
                    lv.idCorte = @idCorte
                    AND v.fechaVenta >= @fechaDesde
                    AND v.fechaVenta <= @fechaHasta
                    AND (@idSucursal <= 0 OR v.idSucursal = @idSucursal)
                    AND (@tipo = '' OR c.tipo = @tipo)
                    AND (@idMarca <= 0 OR c.idMarca = @idMarca)
                GROUP BY
                    CASE
                        WHEN @agrupacionTemporal = 'dia'
                            THEN DATEADD(day, DATEDIFF(day, 0, v.fechaVenta), 0)
                        ELSE DATEADD(hour, DATEDIFF(hour, 0, v.fechaVenta), 0)
                    END
                ORDER BY BucketFecha;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.Add("@idCorte", SqlDbType.Int).Value = idCorte;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                    p.Add("@fechaDesde", SqlDbType.DateTime).Value = fechaDesde;
                    p.Add("@fechaHasta", SqlDbType.DateTime).Value = fechaHasta;
                    p.Add("@tipo", SqlDbType.NVarChar, 50).Value = (tipo ?? "").Trim();
                    p.Add("@idMarca", SqlDbType.Int).Value = idMarca;
                    p.Add("@agrupacionTemporal", SqlDbType.NVarChar, 10).Value = (agrupacionTemporal ?? "hora").Trim().ToLowerInvariant();
                }
            );
        }

        public DataTable imprimirTeoricoReal(DataTable dtTeoricoReal, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            if (dtTeoricoReal == null) dtTeoricoReal = new DataTable();

            // Nota: Db.DataTable devuelve un DT nuevo; si vos necesitás "llenar" el que pasás, mejor devolvés el nuevo.
            // Mantengo tu firma: retorno el que devuelve Db (y si querés copiamos al dtTeoricoReal).
            return Db.DataTable(
                _empresa,
                "StockTeoricoReal",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                }
            );
        }

        public DataTable TotalKgsCortePorCompra(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            return Db.DataTable(
                _empresa,
                "a_CierreStock",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                }
            );
        }

        public DataTable TotalMovimientosPorCorte(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            return Db.DataTable(
                _empresa,
                "TotalMovimientosPorCorte",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                }
            );
        }

        public DataTable Balance(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            var dt = Db.DataTable(
                _empresa,
                "BalanceConsFinal_FecDesde_Hasta",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@FechaDesde", fechaDesde);
                    p.AddWithValue("@FechaHasta", fechaHasta.AddDays(1)); // incluir día completo
                }
            );

            if (dt.Columns.Contains("orden"))
                dt.Columns.Remove("orden");

            foreach (DataRow row in dt.Rows)
            {
                string desc = row["Descripcion"]?.ToString() ?? "";

                if (desc.Contains("DETALLE") || desc.Contains("NOTAS") || (desc.Length > 0 && desc[0] == '*'))
                {
                    for (int i = 1; i < dt.Columns.Count; i++)
                        row[i] = DBNull.Value;
                }

                if (desc.Contains("COMPRAS") || desc.Contains("GASTOS"))
                {
                    if (dt.Columns.Contains("Tickets"))
                        row["Tickets"] = DBNull.Value;
                }
            }

            return dt;
        }

        public List<Entidades.ExistenciaStockPorSucursalPlanoVm> ObtenerExistenciaPorSucursalesPlano(
            string texto,
            int idSucursal,
            DateTime? fechaHasta,
            string tipo,
            int idProveedor,
            int idMarca,
            int idCorte,
            bool soloConStock)
        {
            return Db.Reader(
                _empresa,
                "dbo.a_ExistenciaStockPorSucursales",
                CommandType.StoredProcedure,
                dr => new Entidades.ExistenciaStockPorSucursalPlanoVm
                {
                    IdCorte = dr["idCorte"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idCorte"]),
                    Codigo = dr["Codigo"] == DBNull.Value ? 0L : Convert.ToInt64(dr["Codigo"]),
                    Corte = dr["Corte"] == DBNull.Value ? "" : Convert.ToString(dr["Corte"]),
                    IdSucursal = dr["idSucursal"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idSucursal"]),
                    Sucursal = dr["Sucursal"] == DBNull.Value ? "" : Convert.ToString(dr["Sucursal"]),
                    FechaUltimoCierre = dr["FechaUltimoCierre"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["FechaUltimoCierre"]),
                    StockInicial = dr["StockInicial"] == DBNull.Value ? 0f : Convert.ToSingle(dr["StockInicial"]),
                    Compras = dr["Compras"] == DBNull.Value ? 0f : Convert.ToSingle(dr["Compras"]),
                    IngresoElaborado = dr["IngresoElaborado"] == DBNull.Value ? 0f : Convert.ToSingle(dr["IngresoElaborado"]),
                    IngresoStock = dr["IngresoStock"] == DBNull.Value ? 0f : Convert.ToSingle(dr["IngresoStock"]),
                    IngresoMovimiento = dr["IngresoMovimiento"] == DBNull.Value ? 0f : Convert.ToSingle(dr["IngresoMovimiento"]),
                    AjusteStock = dr["AjusteStock"] == DBNull.Value ? 0f : Convert.ToSingle(dr["AjusteStock"]),
                    TotalIngresos = dr["TotalIngresos"] == DBNull.Value ? 0f : Convert.ToSingle(dr["TotalIngresos"]),
                    EgresoStock = dr["EgresoStock"] == DBNull.Value ? 0f : Convert.ToSingle(dr["EgresoStock"]),
                    EgresoMovimiento = dr["EgresoMovimiento"] == DBNull.Value ? 0f : Convert.ToSingle(dr["EgresoMovimiento"]),
                    EgresoElaborado = dr["EgresoElaborado"] == DBNull.Value ? 0f : Convert.ToSingle(dr["EgresoElaborado"]),
                    Ventas = dr["Ventas"] == DBNull.Value ? 0f : Convert.ToSingle(dr["Ventas"]),
                    TotalEgresos = dr["TotalEgresos"] == DBNull.Value ? 0f : Convert.ToSingle(dr["TotalEgresos"]),
                    StockActual = dr["StockActual"] == DBNull.Value ? 0f : Convert.ToSingle(dr["StockActual"]),
                    Promedio = dr["promedio"] == DBNull.Value ? 0f : Convert.ToSingle(dr["promedio"]),
                    PuntoStock = dr["PuntoStock"] == DBNull.Value ? 0f : Convert.ToSingle(dr["PuntoStock"]),
                    EstadoStock = dr["EstadoStock"] == DBNull.Value ? "" : Convert.ToString(dr["EstadoStock"])
                },
                setParams: p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idEmpresa", _empresa.IdEmpresa);
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaHasta", fechaHasta.HasValue ? (object)fechaHasta.Value : DBNull.Value);
                    p.AddWithValue("@tipo", tipo ?? "");
                    p.AddWithValue("@idProveedor", idProveedor);
                    p.AddWithValue("@idMarca", idMarca);
                    p.AddWithValue("@idCorte", idCorte);
                    p.AddWithValue("@soloConStock", soloConStock);
                });
        }

        // =========================
        // TIPOS PRODUCTO
        // =========================
        public DataTable obtenerTiposProductoGrilla(string buscarText)
        {
            string sql = @"
                    SELECT tipo, orden, creado AS Creado, actualizado AS Actualizado, reservadoSistema AS Reservado
                    FROM dbo.TiposProducto
                    WHERE (@buscar IS NULL OR tipo LIKE @buscar)
                    ORDER BY orden, tipo;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                (Action<SqlParameterCollection>)(p =>
                {
                    if (string.IsNullOrWhiteSpace(buscarText))
                        p.Add("@buscar", SqlDbType.NVarChar, 200).Value = DBNull.Value;
                    else
                        p.Add("@buscar", SqlDbType.NVarChar, 200).Value = "%" + buscarText.Trim() + "%";
                })
            );
        }

        public string addOrEditTipoProducto(string tiposProducto, string orden, bool esInsert, string tipoToUpdate)
        {
            // Mantengo tu mensaje
            string mensaje = "";

            using (SqlConnection cn = Db.Open(_empresa))
            {
                if (esInsert)
                {
                    using (SqlCommand cmdDup = new SqlCommand("SELECT COUNT(*) FROM TiposProducto WHERE tipo = @tipo", cn))
                    {
                        cmdDup.CommandType = CommandType.Text;
                        cmdDup.Parameters.AddWithValue("@tipo", tiposProducto);

                        int existe = Convert.ToInt32(cmdDup.ExecuteScalar());
                        if (existe != 0)
                            return "Ya existe un Tipo con el mismo nombre.";
                    }
                }

                string query = esInsert
                    ? "INSERT INTO TiposProducto (tipo, orden, reservadoSistema, creado) VALUES (@tipo, @orden, @reservadoSistema, @creado)"
                    : "UPDATE TiposProducto SET tipo = @tipo, orden = @orden, actualizado = @actualizado WHERE tipo LIKE @tipoToUpdate; " +
                      "UPDATE Corte SET tipo = @tipo WHERE tipo LIKE @tipoToUpdate;";

                using (SqlCommand cmd = new SqlCommand(query, cn))
                {
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@tipo", tiposProducto);
                    cmd.Parameters.AddWithValue("@tipoToUpdate", tipoToUpdate ?? "");
                    cmd.Parameters.AddWithValue("@orden", orden);
                    cmd.Parameters.AddWithValue("@reservadoSistema", false);
                    cmd.Parameters.AddWithValue("@creado", DateTime.Now);
                    cmd.Parameters.AddWithValue("@actualizado", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }
            }

            return mensaje;
        }

        public string eliminarTipoProducto(string tiposProducto)
        {
            using (SqlConnection cn = Db.Open(_empresa))
            {
                using (SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Corte WHERE tipo = @tipo", cn))
                {
                    cmdCheck.CommandType = CommandType.Text;
                    cmdCheck.Parameters.AddWithValue("@tipo", tiposProducto);

                    int existe = Convert.ToInt32(cmdCheck.ExecuteScalar());
                    if (existe != 0)
                        return "Existen Productos/Cortes con el Tipo que quiere eliminar.\n\nPara poder eliminar el Tipo debe cambiar todo los Productos/Cortes asociados a éste.";
                }

                using (SqlCommand cmd = new SqlCommand("DELETE FROM TiposProducto WHERE tipo = @tipo", cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@tipo", tiposProducto);
                    cmd.ExecuteNonQuery();
                }
            }

            return "";
        }

        public long sugerirCodigo(string tipo)
        {
            string query = @"
                SELECT MIN(Codigo + 1) AS CodigoDisponible
                FROM Corte 
                WHERE Tipo = @tipo
                AND NOT EXISTS (
                    SELECT 1 FROM Corte c2 
                    WHERE c2.Codigo = Corte.Codigo + 1 AND c2.Tipo = @tipo
                );";

            object obj = Db.Scalar(
                _empresa,
                query,
                CommandType.Text,
                p => p.AddWithValue("@tipo", tipo)
            );

            return (obj == null || obj == DBNull.Value) ? -1 : Convert.ToInt64(obj);
        }

        public int obtenerNivelCorte(int idCorteMaestro)
        {
            object obj = Db.Scalar(
                _empresa,
                "obtenerNivelCorte",
                CommandType.StoredProcedure,
                p => p.AddWithValue("@idCorteMaestro", idCorteMaestro)
            );

            return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
        }
    }
}
