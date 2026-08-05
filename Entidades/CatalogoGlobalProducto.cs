using System;

namespace Entidades
{
    /// <summary>
    /// Producto del catalogo global (compartido entre todas las empresas), leido desde
    /// dbo.CatalogoGlobalProducto. Nunca tiene un idEmpresa real -- es la fuente que se
    /// clona hacia Entidades.Corte cuando una empresa da de alta un producto desde el
    /// catalogo global (ver ProductosController.ClonarProductoGlobal).
    /// </summary>
    public class CatalogoGlobalProducto
    {
        private bool ingresoRapidoEmbutido;
        private bool enCierreStock;
        private float promedio;
        private bool habilitado;
        private int idAlicuotaIva;
        private float alicuotaIva;
        private bool pesable;
        private int nivel;
        private int puntoStock;
        bool presentacion;

        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string CorteDesc { get; set; }
        public string Tipo { get; set; }
        public float Porcentaje { get; set; }
        public int Independiente { get; set; }
        public float PrecioKg { get; set; }
        public float PrecioKgReferencia { get; set; }
        public float PorcentajeHueso { get; set; }
        public float DesvioEstandar { get; set; }
        public DateTime Creado { get; set; }
        public DateTime? Actualizado { get; set; }
        public int IdAlicuotaIva { get => idAlicuotaIva; set => idAlicuotaIva = value; }
        public float AlicuotaIva { get => alicuotaIva; set => alicuotaIva = value; }
        public bool Pesable { get => pesable; set => pesable = value; }
        public int Nivel { get => nivel; set => nivel = value; }
        public int PuntoStock { get => puntoStock; set => puntoStock = value; }
        public Persona Marca { get; set; }
        public string MarcaNombre => Marca?.RazonSocial ?? "";

        public bool IngresoRapidoEmbutido { get => ingresoRapidoEmbutido; set => ingresoRapidoEmbutido = value; }
        public bool EnCierreStock { get => enCierreStock; set => enCierreStock = value; }
        public bool Habilitado { get => habilitado; set => habilitado = value; }
        public float Promedio { get => promedio; set => promedio = value; }

        /// <summary>Corte maestro dentro del propio catalogo global (autorreferencia por idCorteMaestro).</summary>
        public CatalogoGlobalProducto CorteMaestro { get; set; }
        public string CorteMaestroNombre => CorteMaestro?.CorteDesc ?? "";

        public bool Presentacion { get => presentacion; set => presentacion = value; }

        // Mismo criterio que Entidades.Corte: si porcentajeDesperdicio (aca PorcentajeHueso)
        // es mayor a 100, la fila representa una presentacion en vez de un corte simple
        // (Corte Maestro no admite valor mayor a 100 en ese campo).
        public bool EsPresentacion(float porcentajeDesperdicio) => porcentajeDesperdicio > 100;

        public float getCantPresentacion(float porcDesperdicio)
        {
            float porcentaje = 100f;
            return (porcentaje + porcDesperdicio) / 100;
        }
    }
}
