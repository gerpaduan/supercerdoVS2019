using System;
using System.Collections.Generic;

namespace Entidades
{
    // Titular fiscal de una empresa: la persona (o el CUIT de la propia empresa, en el titular PRINCIPAL) que figura como
    // emisor de las facturas electronicas. Cada titular tiene su CUIT, su certificado ARCA y topes informativos.
    // Titulares fiscales, 2026-10-09 -- ver docs/DECISIONS.md "Titulares fiscales por empresa y por sucursal".
    //
    // Los datos "efectivos" (RazonSocial, Cuit, IdIva, Iibb, InicioActividad, NombreCertificadoPfx) ya vienen resueltos
    // por el repositorio: en el titular principal salen de la tabla empresas; en los demas, de la fila del titular.
    public class TitularFiscal
    {
        public const string VentanaMovil12Meses = "MOVIL12M";
        public const string VentanaCalendario = "CALENDARIO";

        public int IdTitular { get; set; }
        public int IdEmpresa { get; set; }
        public int? IdPersona { get; set; }
        public bool EsPredeterminado { get; set; }
        // Habilitado efectivo: false tambien cuando la titularidad esta revocada (el repositorio lo resuelve asi).
        public bool Habilitado { get; set; }

        // Titularidad cancelada por el super-admin (2026-10-12): el titular queda de solo lectura, no se puede habilitar ni
        // usar para emitir facturas ni notas de credito. Solo existe si ya habia facturado (si no, se borra).
        public bool Revocado { get; set; }
        public DateTime? RevocadoEn { get; set; }

        public long Cuit { get; set; }
        public string RazonSocial { get; set; }
        public int IdIva { get; set; }
        public string CondicionIVA { get; set; }
        public DateTime? InicioActividad { get; set; }
        public long Iibb { get; set; }
        public string NombreCertificadoPfx { get; set; }

        public decimal? LimiteMensual { get; set; }
        public decimal? LimiteAnual { get; set; }
        public string VentanaAnual { get; set; }
        public DateTime Creado { get; set; }

        // Misma semantica que el flag de empresas: hay un nombre de certificado cargado (en cualquier entorno).
        public bool TieneCertificado { get { return !string.IsNullOrWhiteSpace(NombreCertificadoPfx); } }

        // Responsable Inscripto: factura A o B (cond. IVA 2). Monotributo (3) y el resto: factura C.
        public bool EsRRII { get { return IdIva == Persona.codIvaRRII_Afip; } }

        // Lista de puntos de venta de ARCA del titular (solo titulares no principales; el principal usa
        // sucursal.codpuntoventaafip). Se completa solo en las pantallas que la necesitan.
        public List<TitularPuntoVenta> PuntosVenta { get; set; }

        // Sucursales donde este titular es el asignado (solo lectura, para la pantalla de Titulares).
        public List<string> SucursalesQueLoUsan { get; set; }

        public TitularFiscal()
        {
            PuntosVenta = new List<TitularPuntoVenta>();
            SucursalesQueLoUsan = new List<string>();
            VentanaAnual = VentanaMovil12Meses;
        }

        // Nombre para listas y avisos: "Razon social (CUIT)".
        public string NombreCorto
        {
            get { return (RazonSocial ?? "") + (Cuit > 0 ? " (" + Cuit + ")" : ""); }
        }
    }

    // Otra empresa que tiene el mismo CUIT como titular (titular compartido entre empresas, 2026-10-12).
    public class EmpresaConCuit
    {
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; }
        // true si el CUIT es el de la propia empresa (titular principal), false si es un titular agregado.
        public bool EsPrincipal { get; set; }
    }

    // Resultado de revocar una titularidad.
    public class ResultadoRevocacion
    {
        public bool Ok { get; set; }
        public string Mensaje { get; set; }
        // true = nunca facturo en la empresa y se borro la fila; false = ya facturo y queda marcada como revocada.
        public bool Borrado { get; set; }
    }

    // Punto de venta de ARCA de un titular no principal.
    public class TitularPuntoVenta
    {
        public int IdTitular { get; set; }
        public int CodPuntoVentaAfip { get; set; }
        public string Descripcion { get; set; }
        public bool Habilitado { get; set; }
        public string Origen { get; set; }   // "ARCA" | "MANUAL"
    }

    // Punto de venta que un titular no principal usa en una sucursal.
    public class TitularSucursalPuntoVenta
    {
        public int IdTitular { get; set; }
        public int IdSucursal { get; set; }
        public int CodPuntoVentaAfip { get; set; }
    }

    // Lo facturado por un CUIT en una empresa en un periodo (titular compartido entre empresas, 2026-10-12).
    public class FacturadoEmpresa
    {
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; }
        public decimal Importe { get; set; }
    }

    // Facturado de un titular en un periodo, frente a sus topes (pantalla de Titulares, solo informativo).
    public class ConsumoTitular
    {
        public TitularFiscal Titular { get; set; }
        public DateTime MesDesde { get; set; }
        public DateTime MesHasta { get; set; }
        public decimal FacturadoMes { get; set; }
        public DateTime AnualDesde { get; set; }
        public DateTime AnualHasta { get; set; }
        public decimal FacturadoAnual { get; set; }

        // Titular compartido entre empresas: el tope real es de la PERSONA, o sea de lo que su CUIT factura en TODAS las
        // empresas. null = el CUIT no esta en otras empresas (el tope se compara con lo facturado en esta). El desglose por
        // empresa (incluye la propia) lo muestra la pantalla solo al superadmin.
        public decimal? TotalCuitMes { get; set; }
        public decimal? TotalCuitAnual { get; set; }
        public List<FacturadoEmpresa> PorEmpresaMes { get; set; }
        public List<FacturadoEmpresa> PorEmpresaAnual { get; set; }

        public ConsumoTitular()
        {
            PorEmpresaMes = new List<FacturadoEmpresa>();
            PorEmpresaAnual = new List<FacturadoEmpresa>();
        }

        // Porcentaje del tope usado, o null si no hay tope cargado. Con CUIT compartido se calcula sobre el total del CUIT.
        public decimal? PorcentajeMensual { get { return Porcentaje(TotalCuitMes ?? FacturadoMes, Titular == null ? null : Titular.LimiteMensual); } }
        public decimal? PorcentajeAnual { get { return Porcentaje(TotalCuitAnual ?? FacturadoAnual, Titular == null ? null : Titular.LimiteAnual); } }

        private static decimal? Porcentaje(decimal facturado, decimal? limite)
        {
            if (!limite.HasValue || limite.Value <= 0m) return null;
            return Math.Round(facturado * 100m / limite.Value, 1);
        }
    }

    // Titular ofrecido en el modal de factura (selector "Facturar a nombre de") con su punto de venta en la sucursal.
    public class TitularOpcion
    {
        public int IdTitular { get; set; }
        public string Nombre { get; set; }
        public long Cuit { get; set; }
        public bool EsRRII { get; set; }
        public bool EsPredeterminado { get; set; }
        public int PuntoVenta { get; set; }
        public bool TieneCertificado { get; set; }
        public DateTime? VenceCertificado { get; set; }
        // false = se muestra deshabilitado en el selector (sin punto de venta en esta sucursal, sin certificado, vencido).
        public bool Disponible { get; set; }
        public string Motivo { get; set; }
    }

    // Resultado de resolver con que titular y punto de venta se factura una venta.
    public class ResultadoEmisor
    {
        public bool Ok { get; set; }
        // Codigo estructurado para el JS: "TITULAR_NO_DISPONIBLE", "SIN_PUNTO_VENTA", ...
        public string Codigo { get; set; }
        public string Mensaje { get; set; }
        public EmisorFiscal Emisor { get; set; }

        public static ResultadoEmisor Correcto(EmisorFiscal emisor)
        {
            return new ResultadoEmisor { Ok = true, Emisor = emisor };
        }

        public static ResultadoEmisor Error(string codigo, string mensaje)
        {
            return new ResultadoEmisor { Ok = false, Codigo = codigo, Mensaje = mensaje };
        }
    }
}
