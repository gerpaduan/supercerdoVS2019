// Modelos de la pantalla "Qué pedir" (reposición sugerida): filtros, filas y resultado.
// Los llena Negocio.ReposicionStock a partir de la existencia por sucursal (stock + punto de stock) y de la
// actividad reciente del producto (ventas + consumo por elaboración). Detalle y reglas:
// docs/11-manual-de-uso/pantallas/Reposicion.Index.md y docs/DECISIONS.md (2026-10-13, "Qué pedir").
// Sin dependencias: solo tipos simples, para poder usarse desde Negocio, WebCore y los tests.
using System;
using System.Collections.Generic;

namespace Entidades
{
    // Por qué un producto aparece en la lista (o por qué no aparece pero se cuenta).
    public enum MotivoReposicion
    {
        // El stock está por debajo del punto de stock cargado: la cantidad a pedir es (punto - stock).
        BajoPuntoStock = 0,

        // No tiene punto de stock cargado, está en cero (o negativo) y se movió dentro de la ventana de actividad:
        // se avisa. La cantidad solo se sugiere si el usuario pidió "días de venta" para estos productos.
        EnCeroConMovimiento = 1,

        // En cero (o negativo) pero sin salidas dentro de la ventana de actividad: NO se avisa (queda solo como contador).
        EnCeroSinMovimiento = 2
    }

    // Parámetros de la consulta. Los valores por defecto son los de la pantalla.
    public class ReposicionFiltroVm
    {
        // Sucursales tildadas. Vacía = todas.
        public List<int> IdsSucursales { get; set; }
        public string Texto { get; set; }
        public string Tipo { get; set; }
        public int IdProveedor { get; set; }
        public int IdMarca { get; set; }

        // Días hacia atrás en que se busca actividad (ventas o consumo por elaboración). Un producto en cero sin
        // salidas en esa ventana no se avisa. Se acota a [MinDiasVentana, MaxDiasVentana] en Negocio.
        public int DiasVentana { get; set; }

        // Para productos SIN punto de stock: días de venta que se quieren cubrir. 0 = no sugerir cantidad (solo avisar).
        public int DiasCoberturaSinPunto { get; set; }

        // true = además de los avisos, listar los que están en cero sin movimiento en la ventana.
        public bool MostrarSinMovimiento { get; set; }

        // "Sugerir puntos de stock": cuántos días de consumo debe cubrir el punto sugerido (se acota a 1-90).
        public int DiasCoberturaPunto { get; set; }

        // "Sugerir puntos de stock": true = listar también los que ya tienen punto cargado (solo para comparar:
        // un punto cargado nunca se pisa).
        public bool IncluirConPunto { get; set; }

        public ReposicionFiltroVm()
        {
            IdsSucursales = new List<int>();
            Texto = "";
            Tipo = "";
            DiasVentana = 30;
            DiasCoberturaSinPunto = 0;
            DiasCoberturaPunto = 7;
        }
    }

    // Una fila = un producto en una sucursal.
    public class ReposicionFilaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public bool Pesable { get; set; }
        public float Promedio { get; set; }

        public float StockActual { get; set; }
        public float PuntoStock { get; set; }
        public MotivoReposicion Motivo { get; set; }
        // Stock < 0: casi siempre es una compra o ingreso sin cargar, o un cierre desactualizado. Se avisa para revisar.
        public bool StockNegativo { get; set; }
        // Está bajo el punto pero el stock que queda alcanza para mucho más que ReposicionStockCalculador.DiasCoberturaPuntoAlto
        // días al ritmo reciente: el punto de stock cargado probablemente quedó viejo o es demasiado alto. Solo informativo.
        public bool PuntoPareceAlto { get; set; }

        // Cantidad sugerida a pedir (ya redondeada hacia arriba). null = no se puede calcular (sin punto y sin días de cobertura).
        public float? Pedir { get; set; }

        // Actividad (ventas + consumo por elaboración) dentro de la ventana.
        public float ConsumoVentana { get; set; }
        public float ConsumoDiario { get; set; }
        // Días que alcanza el stock actual al ritmo de la ventana. null = sin consumo (no se puede estimar).
        public float? CoberturaDias { get; set; }
        public DateTime? UltimaSalida { get; set; }
        public int? DiasSinSalida { get; set; }

        // Stock positivo del mismo producto en las OTRAS sucursales (para resolver con un movimiento en vez de comprar).
        public List<ReposicionStockSucursalVm> OtrasSucursales { get; set; }
        public float StockOtrasSucursales
        {
            get
            {
                float total = 0f;
                foreach (var otra in OtrasSucursales) total += otra.Stock;
                return total;
            }
        }

        public int IdProveedor { get; set; }
        public string Proveedor { get; set; }
        public decimal? UltimoPrecio { get; set; }
        public DateTime? FechaUltimaCompra { get; set; }
        // Pedir x último precio. null si falta alguno de los dos.
        public decimal? ImporteEstimado { get; set; }

        public ReposicionFilaVm()
        {
            Producto = "";
            Sucursal = "";
            Proveedor = "";
            OtrasSucursales = new List<ReposicionStockSucursalVm>();
        }
    }

    // Stock de un producto en otra sucursal.
    public class ReposicionStockSucursalVm
    {
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public float Stock { get; set; }
    }

    // Aviso por sucursal: el stock parte del último cierre; si es viejo, los números pueden estar desactualizados.
    public class ReposicionAvisoCierreVm
    {
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public DateTime? FechaUltimoCierre { get; set; }
        public int? DiasDesdeCierre { get; set; }
        public bool Viejo { get; set; }
    }

    public class ReposicionVm
    {
        public ReposicionFiltroVm Filtro { get; set; }
        // Solo las filas que hay que mostrar (según Filtro.MostrarSinMovimiento).
        public List<ReposicionFilaVm> Filas { get; set; }
        public List<ReposicionAvisoCierreVm> AvisosCierre { get; set; }

        public int CantBajoPuntoStock { get; set; }
        public int CantEnCeroConMovimiento { get; set; }
        public int CantEnCeroSinMovimiento { get; set; }
        // Productos controlados (en las sucursales consultadas) que no tienen punto de stock cargado.
        public int CantSinPuntoStock { get; set; }
        public int CantProductosControlados { get; set; }
        public decimal ImporteEstimadoTotal { get; set; }

        public DateTime FechaCalculo { get; set; }
        public bool ConsultaRealizada { get; set; }
        public string Mensaje { get; set; }

        // Listas para armar los filtros de la pantalla.
        public List<SucursalColumnaStockVm> SucursalesDisponibles { get; set; }
        public List<string> TiposDisponibles { get; set; }
        public List<Persona> ProveedoresDisponibles { get; set; }
        public List<Persona> MarcasDisponibles { get; set; }

        public ReposicionVm()
        {
            Filtro = new ReposicionFiltroVm();
            Filas = new List<ReposicionFilaVm>();
            AvisosCierre = new List<ReposicionAvisoCierreVm>();
            Mensaje = "";
            SucursalesDisponibles = new List<SucursalColumnaStockVm>();
            TiposDisponibles = new List<string>();
            ProveedoresDisponibles = new List<Persona>();
            MarcasDisponibles = new List<Persona>();
        }
    }

    // ---- "Sugerir puntos de stock" ----

    // Una fila = un producto en una sucursal, con el punto de stock que se sugiere cargar.
    public class PuntoSugeridoFilaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public bool Pesable { get; set; }
        public float Promedio { get; set; }

        public float StockActual { get; set; }
        // 0 = sin punto cargado.
        public float PuntoActual { get; set; }

        // Salidas (ventas + consumo por elaboración) dentro de la ventana de consumo.
        public float ConsumoVentana { get; set; }
        public float ConsumoDiario { get; set; }
        // Días de la ventana en que hubo al menos una venta (no cuenta la elaboración): dice si la salida es pareja o a golpes.
        public int DiasConVenta { get; set; }
        public float? CoberturaDias { get; set; }
        public DateTime? UltimaSalida { get; set; }
        public int? DiasSinSalida { get; set; }

        // Consumo diario x días a cubrir, redondeado hacia arriba (el punto de stock es un entero). null = sin consumo: no se sugiere.
        public int? PuntoSugerido { get; set; }

        public int IdProveedor { get; set; }
        public string Proveedor { get; set; }

        // Solo se puede cargar si hoy no tiene punto y hay una sugerencia (un punto ya cargado no se pisa).
        public bool PuedeCargarse
        {
            get { return PuntoActual <= 0f && PuntoSugerido.HasValue; }
        }

        public PuntoSugeridoFilaVm()
        {
            Producto = "";
            Sucursal = "";
            Proveedor = "";
        }
    }

    public class PuntosSugeridosVm
    {
        public ReposicionFiltroVm Filtro { get; set; }
        public List<PuntoSugeridoFilaVm> Filas { get; set; }

        // Filas (producto x sucursal) que llevan stock en las sucursales consultadas.
        public int CantProductosControlados { get; set; }
        public int CantConPunto { get; set; }
        public int CantSinPunto { get; set; }
        // Sin punto y con consumo: se les puede sugerir.
        public int CantConSugerencia { get; set; }
        // Sin punto y sin consumo en la ventana: no se sugiere nada (se ocultan salvo que se pida lo contrario).
        public int CantSinConsumo { get; set; }

        public DateTime FechaCalculo { get; set; }
        public bool ConsultaRealizada { get; set; }
        public string Mensaje { get; set; }
        // El usuario puede guardar puntos (permiso de edición de productos): lo decide el controller.
        public bool PuedeGuardar { get; set; }

        public List<SucursalColumnaStockVm> SucursalesDisponibles { get; set; }
        public List<string> TiposDisponibles { get; set; }
        public List<Persona> ProveedoresDisponibles { get; set; }
        public List<Persona> MarcasDisponibles { get; set; }

        public PuntosSugeridosVm()
        {
            Filtro = new ReposicionFiltroVm();
            Filas = new List<PuntoSugeridoFilaVm>();
            Mensaje = "";
            SucursalesDisponibles = new List<SucursalColumnaStockVm>();
            TiposDisponibles = new List<string>();
            ProveedoresDisponibles = new List<Persona>();
            MarcasDisponibles = new List<Persona>();
        }
    }

    // Un punto a guardar, tal como llega de la pantalla.
    public class PuntoAGuardarVm
    {
        public int IdCorte { get; set; }
        public int IdSucursal { get; set; }
        public int PuntoStock { get; set; }
    }

    public class GuardarPuntosResultadoVm
    {
        public int Guardados { get; set; }
        // Motivo (con el nombre del producto) de cada punto que NO se guardó.
        public List<string> Rechazados { get; set; }
        // Si falló la escritura en la base: lo guardado hasta ahí queda guardado (cada producto se escribe completo o nada).
        public Exception Error { get; set; }

        public GuardarPuntosResultadoVm()
        {
            Rechazados = new List<string>();
        }
    }

    // ---- Datos que devuelve el repositorio (Contratos.IActividadStockRepository) ----

    // Salidas de un producto de stock en una sucursal: ventas y consumo por elaboración, ya llevadas al producto que
    // lleva el stock (jerarquía madre/hija con su factor, igual que el cálculo de existencia).
    public class ActividadStockVm
    {
        public int IdSucursal { get; set; }
        public int IdCorte { get; set; }
        public DateTime? UltimaVenta { get; set; }
        public DateTime? UltimaElaboracion { get; set; }
        // Kg (o unidades) dentro de la ventana pedida.
        public float VentasVentana { get; set; }
        public float ElaboracionVentana { get; set; }
        public int DiasConVentaVentana { get; set; }
    }

    // Proveedor "habitual" de un producto: el de la compra más reciente.
    public class ProveedorHabitualVm
    {
        public int IdCorte { get; set; }
        public int IdProveedor { get; set; }
        public string Proveedor { get; set; }
        public decimal? UltimoPrecio { get; set; }
        public DateTime? FechaUltimaCompra { get; set; }
    }
}
