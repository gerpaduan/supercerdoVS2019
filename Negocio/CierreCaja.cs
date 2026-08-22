using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Utilidades;

namespace Negocio
{
    public class CierreCaja
    {
        Negocio.Venta oVentaN;

        // oCierreD: TODO Datos.CierreCaja migrado a la interfaz -- puede ser SQL Server o
        // Postgres. Desde la Etapa 10 (cambiarSucursalCaja/obtenerPreviewCambioSucursalCaja)
        // ya no queda ningun metodo fuera de ICierreCajaRepository, asi que esta clase volvio
        // a tener un solo campo de datos (antes de la Etapa 10 existia ademas
        // oCierreDSqlServer, exclusivo para esos 2 metodos). Ver docs/DECISIONS.md.
        private readonly Contratos.ICierreCajaRepository oCierreD;
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        // Repo de Sucursal usado SOLO para hidratar CierreCaja.Sucursal en convertDatatableToList.
        // Optativo, default null -- si no se inyecta, cae a Datos.Sucursal (SQL Server), mismo
        // patron que Negocio.Usuario.ObtenerSucursalRepo(). Gap real encontrado probando POS
        // contra Postgres (2026-08-21): sin esto, la sucursal del cierre de caja siempre se
        // resolvia contra SQL Server aunque oCierreD fuera Postgres. Ver docs/DECISIONS.md.
        private readonly Contratos.ISucursalRepository _sucursalRepo;

        // Constructor existente: SIN CAMBIOS de comportamiento.
        public CierreCaja(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;_param = param;
            oCierreD = new Datos.CierreCaja(empresa, param);
            oVentaN = new Negocio.Venta(empresa, param);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de ICierreCajaRepository
        // (ej. DatosPostgres.CierreCajaPg). ventaN y sucursalRepositorio son opcionales (default
        // null -> SQL Server, mismo comportamiento de siempre) -- mismo gap que el ya cerrado en
        // NegocioFactory.CrearCompra/CrearVenta (colaborador interno sin cablear al motor
        // inyectado). obtenerTotalVentas es el unico metodo de Negocio.Venta que este oVentaN
        // usa, asi que NegocioFactory arma un Venta minimo (solo su propio repo, sin
        // ctaCteN/cierreCajaN/personaN) en vez de llamar CrearVenta -- evita el ciclo
        // CrearVenta->CrearCierreCaja->CrearVenta ya documentado en Negocio/Venta.cs.
        public CierreCaja(Contratos.ICierreCajaRepository repositorio, IEmpresaContext empresa, IParametrosContext param = null, Negocio.Venta ventaN = null, Contratos.ISucursalRepository sucursalRepositorio = null)
        {
            _empresa = empresa; _param = param;
            oCierreD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            oVentaN = ventaN ?? new Negocio.Venta(empresa, param);
            _sucursalRepo = sucursalRepositorio;
        }

        private Contratos.ISucursalRepository ObtenerSucursalRepo()
        {
            return _sucursalRepo ?? new Datos.Sucursal(_empresa);
        }

        public Entidades.CierreCaja findByIdOrLast(Entidades.CierreCaja oCierre, Entidades.CierreCaja.tipoBusqueda tipoBusqueda, string texto)
        {
            DataTable dtCierreCaja = findCierreCaja(oCierre, tipoBusqueda, texto, null);
            List<Entidades.CierreCaja> listCierreCaja = convertDatatableToList(dtCierreCaja);
            Entidades.CierreCaja cierreCaja = listCierreCaja.Count > 0 ? listCierreCaja[0] : null;
            return cierreCaja;
        }

        private List<Entidades.CierreCaja> convertDatatableToList(DataTable dtCierreCaja)
        {
            List<Entidades.CierreCaja> listCierreCaja = new List<Entidades.CierreCaja>();
            Entidades.CierreCaja oCierreE = null;
            if (dtCierreCaja.Rows.Count > 0)
            {
                Entidades.Sucursal oSucursalE = ObtenerSucursalRepo().findById(Convert.ToInt32(dtCierreCaja.Rows[0]["idSucursal"]));

                foreach (DataRow drCierreCaja in dtCierreCaja.Rows)
                {
                    oCierreE = new Entidades.CierreCaja();
                    oCierreE.Id = Convert.ToInt32(drCierreCaja["id"]);
                    oCierreE.Sucursal = oSucursalE;
                    oCierreE.UsuarioInicio = new Entidades.Usuario
                    {
                        Id = Convert.ToInt32(drCierreCaja["usuarioInicio"]),
                        Nombre = ObtenerValorString(drCierreCaja, "vendedor", ObtenerValorString(drCierreCaja, "Iniciada_Por")),
                        User = ObtenerValorString(drCierreCaja, "vendedorUsuario")
                    };
                    oCierreE.UsuarioCierre = new Entidades.Usuario
                    {
                        Id = ObtenerValorInt(drCierreCaja, "usuarioCierre"),
                        Nombre = ObtenerValorString(drCierreCaja, "Cerrada_Por")
                    };
                    oCierreE.FechaHoraInicio = Convert.ToDateTime(drCierreCaja["fechaHoraInicio"]);
                    oCierreE.FechaHoraCierre = string.IsNullOrEmpty(drCierreCaja["fechaHoraCierre"].ToString()) ? (DateTime?)null : Convert.ToDateTime(drCierreCaja["fechaHoraCierre"].ToString());
                    oCierreE.CajaInicio = string.IsNullOrEmpty(drCierreCaja["cajaInicio"].ToString()) ? (float?)null : float.Parse(drCierreCaja["cajaInicio"].ToString());
                    oCierreE.Ventas = string.IsNullOrEmpty(drCierreCaja["ventas"].ToString()) ? (float?)null : float.Parse(drCierreCaja["ventas"].ToString());
                    oCierreE.EgresosCaja = string.IsNullOrEmpty(drCierreCaja["gastos"].ToString()) ? (float?)null : float.Parse(drCierreCaja["gastos"].ToString());
                    oCierreE.CajaCierre = string.IsNullOrEmpty(drCierreCaja["cajaCierre"].ToString()) ? (float?)null : float.Parse(drCierreCaja["cajaCierre"].ToString());
                    oCierreE.Diferencia = string.IsNullOrEmpty(drCierreCaja["diferencia"].ToString()) ? (float?)null : float.Parse(drCierreCaja["diferencia"].ToString());
                    oCierreE.CajaInicioSiguiente = string.IsNullOrEmpty(drCierreCaja["cajaInicioSiguiente"].ToString()) ? (float?)null : float.Parse(drCierreCaja["cajaInicioSiguiente"].ToString());
                    oCierreE.ImporteRetirado = string.IsNullOrEmpty(drCierreCaja["importeRetirado"].ToString()) ? (float?)null : float.Parse(drCierreCaja["importeRetirado"].ToString());

                    listCierreCaja.Add(oCierreE);
                }
            }
            return listCierreCaja;
        }

        private static int ObtenerValorInt(DataRow row, string columna, int valorDefault = 0)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return valorDefault;

            return Convert.ToInt32(row[columna]);
        }

        private static string ObtenerValorString(DataRow row, string columna, string valorDefault = "")
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columna) || row[columna] == DBNull.Value)
                return valorDefault;

            return Convert.ToString(row[columna]);
        }

        public DataTable findCierreCaja(Entidades.CierreCaja oCierre, Entidades.CierreCaja.tipoBusqueda tipoBusqueda, string texto, DateTime? fechaDesde)
        {
            return oCierreD.findCierreCaja(oCierre, tipoBusqueda, texto, fechaDesde);
        }

        public void addOrEditCierreCaja(Entidades.CierreCaja oCierreE)
        {
            oCierreD.addOrEditCierreCaja(oCierreE);
        }
        public Entidades.CierreCaja.ResultadoOperacion addOrEditCierreCaja_Result(Entidades.CierreCaja oCierreE)
        {

            var result = new Entidades.CierreCaja.ResultadoOperacion();
            try
            {
                addOrEditCierreCaja(oCierreE);

                result.Ok = true;
                result.Mensaje = "Cierre de caja registrado correctamente.";
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.Mensaje = ex.Message; // o un mensaje personalizado
            }

            return result;
        }

        public DataTable findCierreCajaMultiples(List<Entidades.CierreCaja> listaCierreCaja)
        {
            return oCierreD.findCierreCajaMultiples(listaCierreCaja);
        }

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaInicioCaja, DateTime? fechaCierreCaja)
        {
            return oVentaN.obtenerTotalVentas(idVendedor, idSucursal, fechaInicioCaja, fechaCierreCaja);
        }

        #region TipoEgresoCaja
        public DataTable obtenerTiposEgresoCaja(string buscarText, int idTipoEgreso)
        {
            return oCierreD.obtenerTiposEgresoCaja(buscarText, idTipoEgreso);
        }
        public void addOrEditTipoEgreso(int id, string tipoEgresoCaja, bool esGasto)
        {
            oCierreD.addOrEditTipoEgreso(id, tipoEgresoCaja,esGasto);
        }
        public void eliminarTipoEgreso(int id)
        {
            oCierreD.eliminarTipoEgreso(id);
        }

        public int getIdEgresoCajaPorCompra()
        {
            //DataTable tiposEgresos = obtenerTiposEgresoCaja();
            int idTipoEgreso = Entidades.EgresoCaja.idCompraEgresoCaja;
            //foreach (DataRow row in tiposEgresos.Rows)
            //{
            //    if (!string.IsNullOrEmpty(row["esCompra"].ToString()) && !row["esCompra"].ToString().Equals("0"))
            //        idTipoEgreso = Convert.ToInt32(row["id"].ToString());
            //}
            return idTipoEgreso;
        }

        public DataTable obtenerEgresosCaja(int idSucursal, int idUsuario, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCierreD.obtenerEgresosCaja(idSucursal, idUsuario, idTipoEgresoCaja, texto, fechaDesde, fechaHasta);
        }

        public DataTable obtenerEgresosCajaGastosBalance(int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCierreD.obtenerEgresosCajaGastosBalance(idSucursal, fechaDesde, fechaHasta);
        }

        public DataTable obtenerGastosAgrupadosBalance(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal)
        {
            return oCierreD.obtenerGastosAgrupadosBalance(fechaDesde, fechaHasta, idSucursal);
        }

        public Entidades.EgresoCaja addOrEditEgresoCaja(Entidades.EgresoCaja oEgresoCaja, Contratos.IUnitOfWork unitOfWork = null)
        {
            return oCierreD.addOrEditEgresoCaja(oEgresoCaja, unitOfWork);
        }

        public Entidades.EgresoCaja getEgresoCajaById(int idEgresoCaja)
        {
            Entidades.EgresoCaja oEgresoCaja = oCierreD.getEgresoCajaById(idEgresoCaja);

            if (oEgresoCaja != null)
            {
                Negocio.Usuario oUserN = new Usuario(_empresa);

                oEgresoCaja.CreadoPorUser = oUserN.getUserById(oEgresoCaja.CreadoPor);
                oEgresoCaja.ActualizadoPorUser = oUserN.getUserById(oEgresoCaja.ActualizadoPor);
            }

            return oEgresoCaja;
        }

        public List<Entidades.EgresoCaja> getEgresosCajaByIds(List<int> ids)
        {
            return oCierreD.getEgresosCajaByIds(ids);
        }

        public Entidades.EgresoCaja findEgresoCajaByTablaYId(string tabla, int tablaID)
        {
            return oCierreD.findEgresoCajaByTablaYId(tabla, tablaID);
        }

        public float getMontoEgresosCajaVendedor(Entidades.CierreCaja oCierreE)
        {
            return  oCierreD.getMontoEgresosCajaVendedor(oCierreE);
        }

        public DataTable getEgresosCajaVendedor(Entidades.CierreCaja oCierreE)
        {
            return oCierreD.getEgresosCajaVendedor(oCierreE);
        }

        public Contratos.CambioSucursalCajaPreview obtenerPreviewCambioSucursalCaja(Entidades.CierreCaja cierreCaja, int idSucursalNueva)
        {
            return oCierreD.obtenerPreviewCambioSucursalCaja(cierreCaja, idSucursalNueva);
        }

        public Contratos.CambioSucursalCajaResultado cambiarSucursalCaja(Entidades.CierreCaja cierreCaja, int idSucursalNueva, int idUsuarioEjecutor, string usuarioEjecutor)
        {
            return oCierreD.cambiarSucursalCaja(cierreCaja, idSucursalNueva, idUsuarioEjecutor, usuarioEjecutor);
        }

        public bool validarCajaAbiertaVendedor(DateTime fechaHoraRegistro, Entidades.Sucursal oSucursalE, Entidades.Usuario oUsuario)
        {
            bool resp = true;
            Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
            oCierreE.Sucursal = oSucursalE;
            oCierreE.UsuarioInicio = oUsuario;
            // Antes creaba una Negocio.CierreCaja nueva (siempre SQL Server) en vez de usar
            // this -- bug real encontrado en el testeo profundo de escritura (2026-08-20, ver
            // docs/DECISIONS.md): en modo Postgres, validarCajaAbiertaVendedor terminaba
            // consultando la caja abierta contra el motor equivocado.
            oCierreE = findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            if (oCierreE == null || !oCierreE.UsuarioCierre.Id.Equals(0) || oCierreE.FechaHoraInicio > fechaHoraRegistro || fechaHoraRegistro > DateTime.Now)
            {
                resp = false;
                //MessageBox.Show("La fecha y hora del egreso de caja (" + Utilidades.Util_Form.fechaFormato24Horas(txtFechaEgresoCaja.Value) + ") debe ser mayor a la fecha de apertura de caja (" +
                //Utilidades.Util_Form.fechaFormato24Horas(oCierreE.FechaHoraInicio) + ")",
                //    "Mensaje de Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);

            }

            return resp;
        }
        #endregion
    }
}
