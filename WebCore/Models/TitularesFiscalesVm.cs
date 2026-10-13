// ViewModels de la pantalla Configuracion > Titulares de facturacion (TitularesFiscalesController).
// Titulares fiscales por empresa y por sucursal (2026-10-09, ver docs/DECISIONS.md).
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebCore.Models
{
    // Una fila del listado: el titular, su consumo frente a los topes en el periodo elegido y el estado de su certificado.
    public class TitularFilaVm
    {
        public Entidades.ConsumoTitular Consumo { get; set; } = new Entidades.ConsumoTitular();
        public Entidades.TitularFiscal Titular => Consumo.Titular;

        // Estado del certificado en el entorno en uso por la empresa (vencimiento visible, en rojo si esta por vencer).
        public AFIP.EstadoCertificado EstadoCertificado { get; set; } = AFIP.EstadoCertificado.SinCertificado;
        public DateTime? VenceCertificado { get; set; }
        public int? DiasRestantes { get; set; }

        // Titular compartido (2026-10-12): cuantas otras empresas tienen este mismo CUIT como titular y, solo para el super-admin,
        // sus nombres. Con OtrasEmpresas > 0 el tope se mide sobre lo facturado SOLO en esta empresa y se avisa.
        public int OtrasEmpresas { get; set; }
        public string NombresOtrasEmpresas { get; set; } = "";

        // Letra que emite: C si no es RI; A o B si lo es.
        public string LetrasQueEmite => Titular.EsRRII ? "A / B" : "C";
    }

    public class TitularesIndexVm
    {
        public bool Soportado { get; set; } = true;
        public int Mes { get; set; }
        public int Anio { get; set; }
        public List<int> Anios { get; set; } = new List<int>();
        public string EntornoEnUso { get; set; } = "Producción";
        public List<TitularFilaVm> Filas { get; set; } = new List<TitularFilaVm>();
        public List<SucursalTitularVm> Sucursales { get; set; } = new List<SucursalTitularVm>();

        // Permisos y empresa: alta y edicion solo del super-admin; el admin solo habilita / deshabilita. IdEmpresaRuta es null cuando se trabaja sobre la empresa propia.
        public bool PuedeGestionar { get; set; }
        public int? IdEmpresaRuta { get; set; }
        public string EmpresaNombre { get; set; } = "";
        // false cuando el super-admin mira otra empresa: sus certificados se leen con la sesion de esa empresa.
        public bool CertificadoDisponible { get; set; } = true;
    }

    // Que titular usa cada sucursal (solo lectura en el listado; se cambia en Mis Sucursales).
    public class SucursalTitularVm
    {
        public int IdSucursal { get; set; }
        public string Nombre { get; set; } = "";
        public string TitularNombre { get; set; } = "";
        public int PuntoVenta { get; set; }
    }

    public class TitularPuntoVentaVm
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = "";
        public bool Habilitado { get; set; }
        public string Origen { get; set; } = "MANUAL";
    }

    // Una fila de la grilla "Punto de venta por sucursal" de la ficha del titular.
    public class TitularSucursalPvVm
    {
        public int IdSucursal { get; set; }
        public string Nombre { get; set; } = "";
        public int? PuntoVentaAsignado { get; set; }
        public bool EsTitularDeLaSucursal { get; set; }
    }

    // Ficha de un titular (alta y edicion).
    public class TitularEditVm
    {
        public int IdTitular { get; set; }
        public bool EsPrincipal { get; set; }
        public bool Habilitado { get; set; } = true;

        // Persona (solo en el alta se elige; despues queda fija: su CUIT es la carpeta del certificado).
        public int IdPersona { get; set; }
        public string PersonaNombre { get; set; } = "";
        public string Cuit { get; set; } = "";
        public string CondicionIva { get; set; } = "";
        public int IdIva { get; set; }

        // Datos fiscales (copiados de la persona; en el principal son los de la empresa y se editan en Administracion del sistema).
        public string RazonSocial { get; set; } = "";
        public string InicioActividad { get; set; } = "";   // yyyy-MM-dd (input type=date)
        public string Iibb { get; set; } = "";

        // Topes informativos (vacio = sin tope) y ventana del tope anual.
        public string LimiteMensual { get; set; } = "";
        public string LimiteAnual { get; set; } = "";
        public string VentanaAnual { get; set; } = Entidades.TitularFiscal.VentanaMovil12Meses;

        public bool TieneCertificado { get; set; }

        // Titularidad cancelada por el super-admin (de solo lectura; ya habia facturado).
        public bool Revocado { get; set; }
        // Nombres de las otras empresas que tienen este CUIT como titular (vacio = no esta en otras: no se avisa en topes).
        public List<string> OtrasEmpresasConCuit { get; set; } = new List<string>();

        public List<TitularPuntoVentaVm> PuntosVenta { get; set; } = new List<TitularPuntoVentaVm>();
        public List<TitularSucursalPvVm> Sucursales { get; set; } = new List<TitularSucursalPvVm>();

        public bool EsAlta => IdTitular <= 0;
        public string EntornoEnUso { get; set; } = "Producción";
        public int? IdEmpresaRuta { get; set; }
        public string EmpresaNombre { get; set; } = "";
        public bool CertificadoDisponible { get; set; } = true;
    }
}
