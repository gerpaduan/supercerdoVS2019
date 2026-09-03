using System;

namespace Entidades
{
    // Configuracion, por empresa y por prefijo EAN (20-29), de como interpretar un codigo de
    // barras interno generado por una balanza: en que posicion/longitud del EAN-13 viene el
    // codigo de producto (PLU) y en cual el valor (precio o cantidad/peso), con cuantos
    // decimales. Un solo formato activo por (IdEmpresa, Prefijo) -- UNIQUE en la base, en
    // SQL Server y en Postgres. Ver Negocio.BarcodeInterpreter y Negocio.FormatoCodigoBarras.
    public class FormatoCodigoBarras
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; }

        // Los 2 primeros digitos del EAN-13 que activan este formato. Rango 20-29.
        public int Prefijo { get; set; }

        // Siempre 13 (EAN-13) -- se guarda igual como columna para no hardcodear el numero
        // en el codigo de interpretacion.
        public int LongitudTotal { get; set; }

        // Posicion 1-based dentro del codigo donde arranca el PLU/codigo de producto.
        public int PosicionCodigo { get; set; }
        public int LongitudCodigo { get; set; }

        // Posicion 1-based dentro del codigo donde arranca el valor (precio o cantidad).
        public int PosicionValor { get; set; }
        public int LongitudValor { get; set; }

        public TipoValorCodigoBarras TipoValor { get; set; }

        // Cuantos decimales tiene el valor extraido (ej. peso en gramos con 3 decimales de kg).
        public int CantidadDecimales { get; set; }

        public bool Activo { get; set; }

        // Se guarda pero sin efecto funcional en la interpretacion (un solo formato activo
        // por prefijo, forzado por UNIQUE) -- solo se usa para ordenar el listado en la UI.
        public int Prioridad { get; set; }

        public DateTime CreadoUtc { get; set; }
        public int? IdUsuarioCreador { get; set; }
        public DateTime? ModificadoUtc { get; set; }
        public int? IdUsuarioModificador { get; set; }
    }
}
