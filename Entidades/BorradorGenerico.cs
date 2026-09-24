using System;

namespace Entidades
{
    // Formulario en curso (Compras/Stock/Movimientos/Embutidos) guardado en el servidor mientras el
    // operador lo arma (tabla borradorgenerico, solo Postgres -- ver docs/DECISIONS.md "Borradores de
    // Compras/Stock/Movimientos/Embutidos"). Analogo generico de Entidades.VentaBorrador: una sola
    // tabla reusada por los 4 modulos, discriminada por Modulo. Las lineas viajan en Payload (JSON
    // especifico de cada modulo, opaco para esta clase y para Negocio).
    public class BorradorGenerico
    {
        public const string EstadoActiva = "ACTIVA";
        public const string EstadoFinalizada = "FINALIZADA";
        public const string EstadoDescartada = "DESCARTADA";

        public const string ModuloCompra = "COMPRA";
        public const string ModuloStock = "STOCK";
        public const string ModuloMovimiento = "MOVIMIENTO";
        public const string ModuloEmbutidoCarga = "EMBUTIDO_CARGA";
        public const string ModuloEmbutidoRapido = "EMBUTIDO_RAPIDO";

        public static readonly string[] ModulosValidos =
        {
            ModuloCompra, ModuloStock, ModuloMovimiento, ModuloEmbutidoCarga, ModuloEmbutidoRapido
        };

        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdSucursal { get; set; }
        public string Modulo { get; set; }

        // Id del registro que se esta editando (compra/movimiento/embutido); null/0 = alta nueva. En
        // Embutidos-Carga, donde editar es anular+recrear, es el id del original que se va a reemplazar.
        public int? IdRegistro { get; set; }

        // Operador real (para la cuenta compartida de produccion, quien se autorizo con su clave);
        // IdUsuarioSesion es la cuenta con la que se inicio sesion.
        public int IdOperador { get; set; }
        public string NombreOperador { get; set; }
        public int IdUsuarioSesion { get; set; }

        // Token del formulario generado por el navegador: identifica este borrador aunque cambie de pestana.
        public Guid ClientId { get; set; }

        // 1 linea armada por el JS del modulo (ej. "Prov: Frigorífico X — 3 líneas — $45.200"): evita
        // que el listado tenga que interpretar el payload, que tiene una forma distinta por modulo.
        public string Resumen { get; set; }
        public int CantLineas { get; set; }

        // JSON del formulario (forma especifica de cada modulo). Se vacia al finalizar/descartar.
        public string Payload { get; set; }

        public string Estado { get; set; }

        // Id de lo creado al confirmar (idcompra/idmovimiento/idembutido).
        public int? IdResultado { get; set; }

        public DateTime Creado { get; set; }
        public DateTime UltimoLatido { get; set; }
        public DateTime? Actualizado { get; set; }
        public DateTime? Finalizado { get; set; }

        // Segundos desde el ultimo latido segun el reloj de la BASE (now() - ultimolatido). Se calcula en
        // la consulta para no depender de que el reloj del servidor web coincida con el de la base.
        public int SegundosSinLatido { get; set; }

        // "Interrumpido" no se guarda: es un borrador ACTIVO cuyo ultimo latido es mas viejo que el
        // umbral (minutos). Asi no hace falta un proceso en segundo plano que lo marque.
        public bool EstaInterrumpida(int minutosSinLatido)
        {
            return Estado == EstadoActiva && SegundosSinLatido >= minutosSinLatido * 60;
        }
    }

    // Resultado de guardar (upsert) un borrador desde el navegador.
    public enum ResultadoGuardarBorradorGenerico
    {
        Guardado = 0,

        // Ya estaba FINALIZADA o DESCARTADA: el navegador debe abandonar ese borrador.
        YaCerrado = 1,

        // El clientId existe pero pertenece a otro operador o sucursal: no se toca.
        Ajeno = 2
    }

    // Evento de un borrador generico (append-only, tabla borradorgenericoevento).
    public class BorradorGenericoEvento
    {
        public const string TipoCierrePestana = "CIERRE_PESTANA";
        public const string TipoLogout = "LOGOUT";
        public const string TipoRecuperada = "RECUPERADA";
        public const string TipoDescartada = "DESCARTADA";

        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdBorrador { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }

        // Quien hizo la accion (en RECUPERADA/DESCARTADA puede ser distinto del dueno del borrador).
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Detalle { get; set; }

        // Datos del borrador al que pertenece el evento (los completa la consulta con un JOIN).
        public string Modulo { get; set; }
        public string NombreOperador { get; set; }
        public string Resumen { get; set; }
        public int CantLineas { get; set; }
    }
}
