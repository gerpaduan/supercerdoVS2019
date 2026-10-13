using System;
using System.Collections.Generic;
using System.Linq;

namespace NegocioTests.Fakes
{
    // Repositorio de titulares fiscales en memoria para los tests (sin base de datos).
    public sealed class FakeTitularFiscalRepository : Contratos.ITitularFiscalRepository
    {
        public readonly List<Entidades.TitularFiscal> Titulares = new List<Entidades.TitularFiscal>();
        public readonly List<Entidades.TitularPuntoVenta> PuntosVenta = new List<Entidades.TitularPuntoVenta>();
        public readonly List<Entidades.TitularSucursalPuntoVenta> Asignados = new List<Entidades.TitularSucursalPuntoVenta>();
        // idSucursal -> idTitular (la sucursal no existe en este fake: solo su asignacion).
        public readonly Dictionary<int, int?> SucursalTitular = new Dictionary<int, int?>();
        public readonly Dictionary<int, string> NombresSucursal = new Dictionary<int, string>();
        public readonly Dictionary<string, decimal> Facturas = new Dictionary<string, decimal>();
        private int _proximoId = 1;

        public Entidades.TitularFiscal AgregarPrincipal(int idEmpresa, long cuit, string razon = "Empresa SA")
        {
            var t = new Entidades.TitularFiscal
            {
                IdTitular = _proximoId++, IdEmpresa = idEmpresa, EsPredeterminado = true, Habilitado = true,
                Cuit = cuit, RazonSocial = razon, IdIva = 3, CondicionIVA = "Monotributo", NombreCertificadoPfx = "certif.pfx"
            };
            Titulares.Add(t);
            return t;
        }

        public Entidades.TitularFiscal AgregarNoPrincipal(int idEmpresa, long cuit, int idIva, string razon = "Titular SRL", bool certificado = true)
        {
            var t = new Entidades.TitularFiscal
            {
                IdTitular = _proximoId++, IdEmpresa = idEmpresa, IdPersona = 100 + _proximoId, Habilitado = true,
                Cuit = cuit, RazonSocial = razon, IdIva = idIva, CondicionIVA = idIva == 2 ? "Responsable Inscripto" : "Monotributo",
                InicioActividad = new DateTime(2020, 1, 1), Iibb = 123,
                NombreCertificadoPfx = certificado ? "certificado.pfx" : ""
            };
            Titulares.Add(t);
            return t;
        }

        public List<Entidades.TitularFiscal> Listar(int idEmpresa, bool soloHabilitados)
        {
            return Titulares.Where(t => t.IdEmpresa == idEmpresa && (!soloHabilitados || (t.Habilitado && !t.Revocado)))
                .OrderByDescending(t => t.EsPredeterminado).ThenBy(t => t.IdTitular).ToList();
        }

        public Entidades.TitularFiscal Obtener(int idTitular) { return Titulares.FirstOrDefault(t => t.IdTitular == idTitular); }

        public Entidades.TitularFiscal ObtenerPrincipal(int idEmpresa) { return Titulares.FirstOrDefault(t => t.IdEmpresa == idEmpresa && t.EsPredeterminado); }

        public int Guardar(Entidades.TitularFiscal titular)
        {
            if (titular.IdTitular <= 0)
            {
                titular.IdTitular = _proximoId++;
                titular.Habilitado = true;
                Titulares.Add(titular);
            }
            else
            {
                var existente = Obtener(titular.IdTitular);
                existente.LimiteMensual = titular.LimiteMensual;
                existente.LimiteAnual = titular.LimiteAnual;
                existente.VentanaAnual = titular.VentanaAnual;
            }
            return titular.IdTitular;
        }

        public void SetHabilitado(int idTitular, bool habilitado) { Obtener(idTitular).Habilitado = habilitado; }

        // Solo cuenta la empresa de la sesion (IdEmpresaSesion): un CUIT puede ser titular en varias empresas.
        public int IdEmpresaSesion = 1;

        public bool ExisteCuit(long cuit, int idTitularExcluir)
        {
            return Titulares.Any(t => t.IdEmpresa == IdEmpresaSesion && t.Cuit == cuit && t.IdTitular != idTitularExcluir);
        }

        public List<string> SucursalesAsignadas(int idTitular)
        {
            var principal = Titulares.First(t => t.IdTitular == idTitular).EsPredeterminado;
            return NombresSucursal
                .Where(s => SucursalTitular.ContainsKey(s.Key)
                    ? SucursalTitular[s.Key] == idTitular
                    : principal)
                .Select(s => s.Value).ToList();
        }

        public int? ObtenerIdTitularDeSucursal(int idSucursal)
        {
            int? valor;
            return SucursalTitular.TryGetValue(idSucursal, out valor) ? valor : null;
        }

        public void AsignarTitularSucursal(int idSucursal, int idTitular, int? codPuntoVenta)
        {
            SucursalTitular[idSucursal] = idTitular;
            if (codPuntoVenta.HasValue) GuardarPuntoVentaSucursal(idTitular, idSucursal, codPuntoVenta);
        }

        public int? ObtenerPuntoVentaAsignado(int idTitular, int idSucursal)
        {
            var a = Asignados.FirstOrDefault(x => x.IdTitular == idTitular && x.IdSucursal == idSucursal);
            return a == null ? (int?)null : a.CodPuntoVentaAfip;
        }

        public List<Entidades.TitularSucursalPuntoVenta> ListarPuntosVentaAsignados(int idTitular)
        {
            return Asignados.Where(a => a.IdTitular == idTitular).ToList();
        }

        public void GuardarPuntoVentaSucursal(int idTitular, int idSucursal, int? codPuntoVenta)
        {
            Asignados.RemoveAll(a => a.IdTitular == idTitular && a.IdSucursal == idSucursal);
            if (codPuntoVenta.HasValue)
                Asignados.Add(new Entidades.TitularSucursalPuntoVenta { IdTitular = idTitular, IdSucursal = idSucursal, CodPuntoVentaAfip = codPuntoVenta.Value });
        }

        public List<Entidades.TitularPuntoVenta> ListarPuntosVenta(int idTitular) { return PuntosVenta.Where(p => p.IdTitular == idTitular).ToList(); }

        public void GuardarPuntoVenta(int idEmpresa, Entidades.TitularPuntoVenta puntoVenta)
        {
            PuntosVenta.RemoveAll(p => p.IdTitular == puntoVenta.IdTitular && p.CodPuntoVentaAfip == puntoVenta.CodPuntoVentaAfip);
            PuntosVenta.Add(puntoVenta);
        }

        public void EliminarPuntoVenta(int idTitular, int codPuntoVenta)
        {
            PuntosVenta.RemoveAll(p => p.IdTitular == idTitular && p.CodPuntoVentaAfip == codPuntoVenta);
        }

        public Entidades.TitularPuntoVenta AgregarPuntoVenta(int idTitular, int cod, bool habilitado = true)
        {
            var pv = new Entidades.TitularPuntoVenta { IdTitular = idTitular, CodPuntoVentaAfip = cod, Habilitado = habilitado, Origen = "MANUAL" };
            PuntosVenta.Add(pv);
            return pv;
        }

        // Facturado: clave "idTitular|yyyy-MM" con el neto del mes (simplificacion de test).
        public decimal Facturado(int idTitular, DateTime desde, DateTime hasta)
        {
            decimal total = 0m;
            for (var d = new DateTime(desde.Year, desde.Month, 1); d <= hasta; d = d.AddMonths(1))
            {
                decimal valor;
                if (Facturas.TryGetValue(idTitular + "|" + d.ToString("yyyy-MM"), out valor)) total += valor;
            }
            return total;
        }

        // Total del CUIT por empresa: suma lo facturado (clave "idTitular|yyyy-MM") de los titulares con ese CUIT, por empresa.
        public List<Entidades.FacturadoEmpresa> FacturadoCuitPorEmpresa(long cuit, DateTime desde, DateTime hasta)
        {
            return Titulares.Where(t => t.Cuit == cuit)
                .GroupBy(t => t.IdEmpresa)
                .Select(g => new Entidades.FacturadoEmpresa
                {
                    IdEmpresa = g.Key,
                    Nombre = "Empresa " + g.Key,
                    Importe = g.Sum(t => Facturado(t.IdTitular, desde, hasta))
                })
                .OrderBy(x => x.IdEmpresa).ToList();
        }

        // ---- Certificado por CUIT (clave "cuit|PROD" / "cuit|HOMO") ----
        public readonly Dictionary<string, string> ClavesPorCuit = new Dictionary<string, string>();

        public string ObtenerClaveCuit(long cuit, bool homologacion)
        {
            string valor;
            return ClavesPorCuit.TryGetValue(cuit + "|" + (homologacion ? "HOMO" : "PROD"), out valor) ? valor : null;
        }

        public void GuardarClaveCuit(long cuit, bool homologacion, string nombreCertificado, string claveProtegida)
        {
            ClavesPorCuit[cuit + "|" + (homologacion ? "HOMO" : "PROD")] = claveProtegida;
        }

        // ---- Titular compartido entre empresas / revocacion ----
        // Titulares (idTitular) con alguna factura emitida: TieneFacturas.
        public readonly HashSet<int> TitularesConFacturas = new HashSet<int>();

        public List<Entidades.EmpresaConCuit> OtrasEmpresasConCuit(long cuit, int idEmpresaPropia)
        {
            return Titulares.Where(t => t.Cuit == cuit && t.IdEmpresa != idEmpresaPropia && !t.Revocado)
                .Select(t => new Entidades.EmpresaConCuit { IdEmpresa = t.IdEmpresa, Nombre = "Empresa " + t.IdEmpresa, EsPrincipal = t.EsPredeterminado })
                .ToList();
        }

        public int EmpresaAdministradoraDeCuit(long cuit)
        {
            var primero = Titulares.Where(t => t.Cuit == cuit && !t.Revocado)
                .OrderByDescending(t => t.EsPredeterminado).ThenBy(t => t.IdTitular).FirstOrDefault();
            return primero == null ? 0 : primero.IdEmpresa;
        }

        public List<Entidades.EmpresaConCuit> OtrasEmpresasConPuntoVenta(long cuit, int codPuntoVenta, int idEmpresaPropia)
        {
            return PuntosVenta
                .Where(p => p.CodPuntoVentaAfip == codPuntoVenta && p.Habilitado)
                .Select(p => Obtener(p.IdTitular))
                .Where(t => t != null && t.Cuit == cuit && t.IdEmpresa != idEmpresaPropia && !t.Revocado)
                .Select(t => new Entidades.EmpresaConCuit { IdEmpresa = t.IdEmpresa, Nombre = "Empresa " + t.IdEmpresa })
                .ToList();
        }

        public bool TieneFacturas(int idTitular) { return TitularesConFacturas.Contains(idTitular); }

        public void Revocar(int idTitular, bool borrar, int idUsuario)
        {
            // Las sucursales que lo usaban vuelven al principal (asignacion NULL) y se borran sus puntos de venta por sucursal.
            foreach (var clave in SucursalTitular.Where(s => s.Value == idTitular).Select(s => s.Key).ToList())
                SucursalTitular[clave] = null;
            Asignados.RemoveAll(a => a.IdTitular == idTitular);

            if (borrar)
            {
                PuntosVenta.RemoveAll(p => p.IdTitular == idTitular);
                Titulares.RemoveAll(t => t.IdTitular == idTitular);
                return;
            }
            var titular = Obtener(idTitular);
            titular.Revocado = true;
            titular.RevocadoEn = DateTime.Now;
            titular.Habilitado = false;
        }

        public void Reponer(int idTitular)
        {
            var titular = Obtener(idTitular);
            titular.Revocado = false;
            titular.RevocadoEn = null;
            titular.Habilitado = false;
        }
    }
}
