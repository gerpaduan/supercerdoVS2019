using System;

namespace Entidades
{
    // Datos fiscales de QUIEN emite una factura electronica: el titular fiscal (o la propia empresa) con su CUIT,
    // condicion IVA, IIBB, inicio de actividades, punto de venta y domicilio. Un solo lugar desde donde el modal, el
    // servicio de AFIP, el PDF, el ticket y el QR leen al emisor (antes cada uno leia Empresa.* por su cuenta).
    // Titulares fiscales, 2026-10-09 -- ver docs/DECISIONS.md "Titulares fiscales por empresa y por sucursal".
    public class EmisorFiscal
    {
        public int? IdTitular { get; set; }
        public long Cuit { get; set; }
        public string RazonSocial { get; set; }
        public string CondicionIVA { get; set; }
        public bool EsRRII { get; set; }
        public long Iibb { get; set; }
        public DateTime? InicioActividad { get; set; }
        public int PuntoVenta { get; set; }
        public string NombreFantasia { get; set; }
        public bool EsPredeterminado { get; set; }
        public string NombreCertificadoPfx { get; set; }
        // La titularidad fue cancelada por el super-admin: no se puede emitir ni nota de credito con este titular.
        public bool Revocado { get; set; }
        public DateTime? VenceCertificado { get; set; }

        // Domicilio para PDF y ticket. En comprobantes nuevos es la direccion de la sucursal de la venta; en los
        // anteriores a esta funcion (EsLegado) es el domicilio de la empresa, como se imprimia hasta hoy.
        public string Domicilio { get; set; }

        // Domicilio que muestra el modal de factura. Igual a Domicilio salvo en comprobantes legados, donde el modal
        // mostraba solo la direccion de la sucursal.
        public string DomicilioModal { get; set; }

        // true = datos tal cual se leian antes de los titulares fiscales (SQL Server o comprobante sin emisor guardado).
        public bool EsLegado { get; set; }

        public bool TieneCertificado { get { return !string.IsNullOrWhiteSpace(NombreCertificadoPfx); } }

        // Texto que se imprime en "Cond. IVA" (PDF, ticket, modal, recibos, presupuestos). Se DERIVA de la condicion real
        // del emisor y no del texto libre de empresas/titulares.condicioniva: ese texto lo tipeaba una persona ("RRII") o
        // quedaba vacio (monotributistas dados de alta desde una persona), y se imprimia tal cual. 2026-10-12.
        public const string TextoResponsableInscripto = "Responsable Inscripto";
        public const string TextoResponsableMonotributo = "Responsable Monotributo";

        // Regla de la plataforma: Responsable Inscripto (factura A/B) o, si no, Monotributo (factura C).
        public static string TextoCondicionIva(bool esRRII)
        {
            return esRRII ? TextoResponsableInscripto : TextoResponsableMonotributo;
        }

        // Igual, a partir del codigo de condicion IVA de ARCA (2 = RI, 3 = Monotributo). Con cualquier otro codigo no se
        // inventa nada: se usa el texto de respaldo (el cargado a mano), que puede venir vacio.
        public static string TextoCondicionIva(int idIva, string textoRespaldo)
        {
            if (idIva == Persona.codIvaRRII_Afip) return TextoResponsableInscripto;
            if (idIva == Persona.codIvaMonotributo_Afip) return TextoResponsableMonotributo;
            return textoRespaldo ?? "";
        }

        // "Direccion - Localidad" (o solo lo que haya). Mismo formato que ya usaban PDF y ticket con la empresa.
        public static string ArmarDomicilio(string direccion, string localidad)
        {
            string d = (direccion ?? "").Trim();
            string l = (localidad ?? "").Trim();
            if (d.Length == 0) return l;
            return l.Length == 0 ? d : d + " - " + l;
        }

        // Domicilio de comprobantes nuevos: la direccion de la sucursal donde se vendio. Respaldo: el de la empresa
        // (la direccion de la sucursal es opcional y las sucursales nuevas nacen sin ella).
        public static string DomicilioNuevo(Sucursal sucursal, Empresa empresa)
        {
            string deSucursal = sucursal == null ? "" : ArmarDomicilio(sucursal.Direccion, sucursal.Localidad);
            if (!string.IsNullOrWhiteSpace(deSucursal)) return deSucursal;
            return empresa == null ? "" : ArmarDomicilio(empresa.Domicilio, empresa.Ciudad);
        }

        // Comportamiento de SIEMPRE: datos de la empresa tal cual, sin titulares (SQL Server, o factura anterior a esta
        // funcion). El punto de venta es el de la sucursal.
        public static EmisorFiscal Legado(Empresa empresa, Sucursal sucursal)
        {
            return new EmisorFiscal
            {
                IdTitular = null,
                Cuit = empresa == null ? 0 : empresa.Cuit,
                RazonSocial = empresa == null ? "" : empresa.RazonSocialAfip,
                // Derivado del flag esRRII de la empresa (el texto libre empresa.CondicionIVA ya no se imprime).
                CondicionIVA = empresa == null ? "" : TextoCondicionIva(empresa.EsRRII),
                EsRRII = empresa != null && empresa.EsRRII,
                Iibb = empresa == null ? 0 : empresa.Iibb,
                InicioActividad = (empresa == null || empresa.InicioActividad == DateTime.MinValue) ? (DateTime?)null : empresa.InicioActividad,
                PuntoVenta = sucursal == null ? 0 : sucursal.CodPuntoVentaAfip,
                NombreFantasia = empresa == null ? "" : empresa.NombreFantasia,
                EsPredeterminado = true,
                NombreCertificadoPfx = empresa == null ? "" : empresa.NombreCertificado_pfx,
                Domicilio = empresa == null ? "" : ArmarDomicilio(empresa.Domicilio, empresa.Ciudad),
                DomicilioModal = sucursal == null ? "" : (sucursal.Direccion ?? ""),
                EsLegado = true
            };
        }

        // Emisor para una factura NUEVA con titulares fiscales: el titular (principal o no) y su punto de venta.
        public static EmisorFiscal DesdeTitular(TitularFiscal titular, Empresa empresa, Sucursal sucursal, int puntoVenta)
        {
            string domicilio = DomicilioNuevo(sucursal, empresa);
            return new EmisorFiscal
            {
                IdTitular = titular.IdTitular,
                Cuit = titular.Cuit,
                RazonSocial = titular.RazonSocial,
                // Derivado de IdIva (2 = RI, 3 = Monotributo); el texto libre del titular solo es respaldo.
                CondicionIVA = TextoCondicionIva(titular.IdIva, titular.CondicionIVA),
                EsRRII = titular.EsRRII,
                Iibb = titular.Iibb,
                InicioActividad = titular.InicioActividad,
                PuntoVenta = puntoVenta,
                NombreFantasia = empresa == null ? "" : empresa.NombreFantasia,
                EsPredeterminado = titular.EsPredeterminado,
                NombreCertificadoPfx = titular.NombreCertificadoPfx,
                Domicilio = domicilio,
                DomicilioModal = domicilio,
                EsLegado = false
            };
        }

        // Emisor de una factura YA EMITIDA: lo que se guardo en el comprobante. Si no tiene emisor guardado (anterior a
        // esta funcion) se imprime como siempre (Legado), salvo el punto de venta, que es el del comprobante.
        public static EmisorFiscal DesdeFactura(FacturaElectronica factura, Empresa empresa, Sucursal sucursal)
        {
            int ptoVentaFactura = 0;
            if (factura != null && !string.IsNullOrWhiteSpace(factura.PtoVtaAfip))
                int.TryParse(CuitHelper.Normalizar(factura.PtoVtaAfip), out ptoVentaFactura);

            if (factura == null || !factura.TieneEmisorGuardado)
            {
                var legado = Legado(empresa, sucursal);
                if (ptoVentaFactura > 0) legado.PuntoVenta = ptoVentaFactura;
                return legado;
            }

            string domicilio = string.IsNullOrWhiteSpace(factura.DomicilioEmisor)
                ? DomicilioNuevo(sucursal, empresa)
                : factura.DomicilioEmisor;

            return new EmisorFiscal
            {
                IdTitular = factura.IdTitular,
                Cuit = factura.CuitEmisor.Value,
                RazonSocial = factura.RazonSocialEmisor,
                // Se respeta el texto guardado en el comprobante; si quedo vacio (monotributistas emitidos antes de este
                // cambio) se deriva del flag guardado. Limite conocido: el flag no distingue Monotributo de otras
                // condiciones que tambien emiten C, y en ese caso se imprime Monotributo.
                CondicionIVA = string.IsNullOrWhiteSpace(factura.CondicionIvaEmisor)
                    ? TextoCondicionIva(factura.EsRRIIEmisor ?? false)
                    : factura.CondicionIvaEmisor,
                EsRRII = factura.EsRRIIEmisor ?? false,
                Iibb = factura.IibbEmisor ?? 0,
                InicioActividad = factura.InicioActividadEmisor,
                PuntoVenta = ptoVentaFactura > 0 ? ptoVentaFactura : (sucursal == null ? 0 : sucursal.CodPuntoVentaAfip),
                NombreFantasia = empresa == null ? "" : empresa.NombreFantasia,
                EsPredeterminado = false,
                // Una factura ya emitida no necesita certificado para mostrarse; lo necesita la nota de credito
                // (se valida aparte con el titular de origen).
                NombreCertificadoPfx = "",
                Domicilio = domicilio,
                DomicilioModal = domicilio,
                EsLegado = false
            };
        }

        // Copia el emisor a los campos de la factura que se va a guardar (alta). Lo hace GenerarFacturaService al emitir.
        public void CopiarA(FacturaElectronica factura)
        {
            if (factura == null) return;
            factura.IdTitular = IdTitular;
            factura.CuitEmisor = Cuit > 0 ? (long?)Cuit : null;
            factura.RazonSocialEmisor = RazonSocial;
            factura.CondicionIvaEmisor = CondicionIVA;
            factura.EsRRIIEmisor = EsRRII;
            factura.IibbEmisor = Iibb > 0 ? (long?)Iibb : null;
            factura.InicioActividadEmisor = InicioActividad;
            factura.DomicilioEmisor = DomicilioModal;
        }
    }
}
