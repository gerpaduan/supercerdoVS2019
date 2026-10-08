// Lineas de texto de los tickets termicos (58 / 80 mm) de los comprobantes de pago/cobro, de egreso de caja y del
// conteo de cierre del cajero.
// UNA sola fuente de texto: la usan tanto el payload JSON para el agente ESC/POS (formatoEscPos = true, con
// negritas incrustadas) como el HTML del fallback por navegador (formatoEscPos = false, texto plano), asi no
// pueden desalinearse (mismo patron que PuntosExpendioController.ConstruirLineasTicketPuntoExpendio).
// Layout del egreso: replica el que imprime el WinForms (Presentacion/Caja/formAddOrEditEgresoCaja.cs
// imprimirTicket) + el bloque de conteo de efectivo. Ver docs/DECISIONS.md (2026-10-05).
using System;
using System.Collections.Generic;
using System.Globalization;
using WebCore.Models;

namespace WebCore.Services
{
    public static class ComprobanteTickets
    {
        // Mismo criterio que el resto de los tickets de WebCore: 58 mm = 32 caracteres, 80 mm = 43.
        public static int NormalizarMm(int mm) => mm == 58 ? 58 : 80;
        public static int CaracteresPorLinea(int mm) => NormalizarMm(mm) == 58 ? 32 : 43;

        // Linea en blanco que no se pierde en la termica (NBSP, igual que el resto de los tickets).
        private const string Blanco = " ";

        private static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        // ---------- helpers de formato (publicos: los usa el test) ----------

        public static string Truncar(string texto, int ancho)
        {
            texto = texto ?? "";
            return texto.Length > ancho ? texto.Substring(0, ancho) : texto;
        }

        public static string Centrar(string texto, int ancho)
        {
            texto = Truncar(texto, ancho);
            int izquierda = Math.Max(0, (ancho - texto.Length) / 2);
            return new string(' ', izquierda) + texto;
        }

        // "izquierda ........ derecha" en una sola linea de `ancho` caracteres (la derecha manda: no se corta).
        public static string Extremos(string izquierda, string derecha, int ancho)
        {
            derecha = Truncar(derecha, ancho);
            izquierda = Truncar(izquierda, Math.Max(0, ancho - derecha.Length - 1));
            return izquierda + new string(' ', Math.Max(1, ancho - izquierda.Length - derecha.Length)) + derecha;
        }

        // Parte `texto` (puede tener saltos de linea) en lineas de hasta `ancho` caracteres. Corta en el
        // ultimo espacio cuando puede; las continuaciones se sangran con `sangria` espacios.
        public static List<string> Envolver(string texto, int ancho, int sangria = 0)
        {
            var resultado = new List<string>();
            if (string.IsNullOrWhiteSpace(texto)) return resultado;

            foreach (string parrafo in texto.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
            {
                string resto = parrafo.TrimEnd();
                if (resto.Length == 0) continue;

                bool primera = true;
                while (resto.Length > 0)
                {
                    int disponible = Math.Max(1, ancho - (primera ? 0 : sangria));
                    string prefijo = primera ? "" : new string(' ', sangria);

                    if (resto.Length <= disponible)
                    {
                        resultado.Add(prefijo + resto);
                        break;
                    }

                    int corte = resto.LastIndexOf(' ', disponible);
                    if (corte <= 0) corte = disponible; // palabra mas larga que la linea: se parte
                    resultado.Add(prefijo + resto.Substring(0, corte).TrimEnd());
                    resto = resto.Substring(corte).TrimStart();
                    primera = false;
                }
            }

            return resultado;
        }

        // ---------- pago / cobro ----------

        public static List<string> ConstruirLineasPago(ReciboPagoVm vm, int mm, bool formatoEscPos)
        {
            int ancho = CaracteresPorLinea(mm);
            var pago = vm.Pago;
            bool esPago = pago.AProveedor;
            var lineas = new List<string>();

            string Neg(string linea) => formatoEscPos ? EscPosFormato.Negrita(linea) : linea;

            string empresa = vm.Empresa != null
                ? (!string.IsNullOrWhiteSpace(vm.Empresa.NombreFantasia) ? vm.Empresa.NombreFantasia : vm.Empresa.RazonSocialAfip)
                : "";
            if (!string.IsNullOrWhiteSpace(empresa)) lineas.Add(Neg(Centrar(empresa, ancho)));

            lineas.Add(Neg(Centrar(esPago ? "RECIBO DE PAGO" : "RECIBO DE COBRO", ancho)));
            lineas.Add(Truncar("Doc. no valido como factura", ancho));
            lineas.Add("");
            lineas.Add(Truncar("Nro Recibo: " + (pago.NroRecibo ?? ""), ancho));
            lineas.Add(Truncar("Fecha: " + pago.Fecha.ToString("dd/MM/yyyy HH:mm", CulturaAr), ancho));
            if (pago.Sucursal != null && !string.IsNullOrWhiteSpace(pago.Sucursal.SucursalNombre))
                lineas.Add(Truncar("Sucursal: " + pago.Sucursal.SucursalNombre, ancho));
            lineas.Add(new string('-', ancho));

            // Pago: la persona RECIBIO el dinero. Cobro: la persona lo ENTREGO.
            string rotuloPersona = esPago ? "Recibio: " : "Entrego: ";
            lineas.AddRange(Envolver(rotuloPersona + (pago.Persona != null ? pago.Persona.RazonSocial : ""), ancho, 3)
                .ConvertAll(l => Neg(l)));
            lineas.Add(Truncar("Forma de pago: " + (pago.FormaPago ?? ""), ancho));
            lineas.Add(Neg(Extremos("IMPORTE", "$ " + pago.Importe.ToString("N2", CulturaAr), ancho)));

            AgregarBloque(lineas, "Observaciones:", pago.Observaciones, ancho);
            AgregarBloque(lineas, "Conteo de efectivo:", pago.ConteoBilletes, ancho);

            if (pago.CreadoPor != null && !string.IsNullOrWhiteSpace(pago.CreadoPor.Nombre))
            {
                lineas.Add("");
                lineas.Add(Truncar("Usuario: " + pago.CreadoPor.Nombre, ancho));
            }

            AgregarFirma(lineas, ancho, esPago ? "Firma quien recibe: " : "Firma quien entrega: ");
            return lineas;
        }

        // ---------- egreso de caja ----------

        // Mismo orden y rotulos que el ticket del WinForms; el conteo de efectivo va despues del detalle.
        public static List<string> ConstruirLineasEgreso(ReciboEgresoVm vm, int mm, bool formatoEscPos)
        {
            int ancho = CaracteresPorLinea(mm);
            var egreso = vm.Egreso;
            var lineas = new List<string>();

            string Neg(string linea) => formatoEscPos ? EscPosFormato.Negrita(linea) : linea;

            lineas.Add(Centrar("Egreso Caja", ancho));
            lineas.Add("");
            lineas.Add(Truncar("Sucursal: " + (vm.SucursalNombre ?? ""), ancho));
            lineas.Add(Truncar("Vendedor: " + (vm.VendedorNombre ?? ""), ancho));
            lineas.Add(Truncar("Id: " + egreso.Id, ancho));
            lineas.Add(Truncar("Fecha: " + egreso.Fecha.ToString("dd/MM/yyyy  HH:mm:ss", CulturaAr), ancho));
            lineas.Add(new string('-', 20));
            lineas.AddRange(Envolver("Tipo: " + (egreso.TipoEgresoCaja ?? ""), ancho, 3).ConvertAll(l => Neg(l)));
            lineas.AddRange(Envolver("Descripcion: " + (egreso.Descripcion ?? ""), ancho, 3));
            lineas.Add(Neg(Truncar("Monto: $ " + egreso.Monto.ToString("N2", CulturaAr), ancho)));

            if (!string.IsNullOrWhiteSpace(egreso.Detalle))
                lineas.AddRange(Envolver("Detalle: " + egreso.Detalle, ancho, 3));

            AgregarBloque(lineas, "Conteo de efectivo:", egreso.ConteoBilletes, ancho);

            lineas.Add(Truncar("Creado: " + (egreso.Creado ?? egreso.Fecha).ToString("dd/MM/yyyy  HH:mm:ss", CulturaAr), ancho));
            if (egreso.Actualizado.HasValue)
                lineas.Add(Truncar("Modif.: " + egreso.Actualizado.Value.ToString("dd/MM/yyyy  HH:mm:ss", CulturaAr), ancho));

            // Igual que el WinForms: unas lineas en blanco al final para poder cortar el papel.
            for (int i = 0; i < 3; i++) lineas.Add(Blanco);
            return lineas;
        }

        // ---------- conteo de cierre del cajero ----------

        // Comprobante que el cajero se lleva / entrega junto con el efectivo. Es informativo: el cierre oficial lo
        // confirma el encargado al cerrar la caja (por eso lo aclara el pie del ticket).
        public static List<string> ConstruirLineasConteoCierre(ReciboConteoCierreVm vm, int mm, bool formatoEscPos)
        {
            int ancho = CaracteresPorLinea(mm);
            var cierre = vm.Cierre;
            var lineas = new List<string>();

            string Neg(string linea) => formatoEscPos ? EscPosFormato.Negrita(linea) : linea;

            string empresa = vm.Empresa != null
                ? (!string.IsNullOrWhiteSpace(vm.Empresa.NombreFantasia) ? vm.Empresa.NombreFantasia : vm.Empresa.RazonSocialAfip)
                : "";
            if (!string.IsNullOrWhiteSpace(empresa)) lineas.Add(Neg(Centrar(empresa, ancho)));

            lineas.Add(Neg(Centrar("CONTEO DE CIERRE DE CAJA", ancho)));
            lineas.Add(Truncar("Doc. no valido como factura", ancho));
            lineas.Add("");
            lineas.Add(Truncar("Caja Nro: " + cierre.Id, ancho));
            lineas.Add(Truncar("Sucursal: " + (vm.SucursalNombre ?? ""), ancho));
            lineas.Add(Truncar("Cajero: " + (vm.CajeroNombre ?? ""), ancho));
            if (cierre.FechaHoraInicio.HasValue)
                lineas.Add(Truncar("Apertura: " + cierre.FechaHoraInicio.Value.ToString("dd/MM/yyyy HH:mm", CulturaAr), ancho));
            if (cierre.FechaConteoCajero.HasValue)
                lineas.Add(Truncar("Conteo: " + cierre.FechaConteoCajero.Value.ToString("dd/MM/yyyy HH:mm", CulturaAr), ancho));
            lineas.Add(new string('-', ancho));

            decimal importe = Convert.ToDecimal(cierre.CajaCierreCajero ?? 0f);
            lineas.Add(Neg(Extremos("EFECTIVO CONTADO", "$ " + importe.ToString("N2", CulturaAr), ancho)));

            AgregarBloque(lineas, "Conteo de efectivo:", cierre.ConteoBilletesCajero, ancho);

            lineas.Add("");
            lineas.AddRange(Envolver("Conteo informativo: el cierre oficial lo confirma el encargado.", ancho));

            AgregarFirma(lineas, ancho, "Firma cajero: ");
            return lineas;
        }

        // ---------- bloques comunes ----------

        // "Titulo:" y debajo el texto multilinea (observaciones, conteo). Sin texto no agrega nada.
        private static void AgregarBloque(List<string> lineas, string titulo, string texto, int ancho)
        {
            if (string.IsNullOrWhiteSpace(texto)) return;

            lineas.Add("");
            lineas.Add(titulo);
            lineas.AddRange(Envolver(texto, ancho, 2));
        }

        private static void AgregarFirma(List<string> lineas, int ancho, string rotulo)
        {
            lineas.Add(Blanco);
            lineas.Add(Blanco);
            lineas.Add(Blanco);
            lineas.Add(rotulo + new string('_', Math.Max(4, ancho - rotulo.Length)));
            lineas.Add(Blanco);
            lineas.Add(Blanco);
        }
    }
}
