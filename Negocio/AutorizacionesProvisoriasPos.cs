using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Negocio
{
    // Autorizaciones provisorias del POS (2026-11-03, ver docs/DECISIONS.md "POS: autorizacion provisoria de otro
    // usuario"): cuando el operador no tiene un permiso, otro usuario que SI lo tiene puede autorizarle esa accion
    // con su usuario y clave, solo hasta que se cierre la ventana de la accion (guardar, cerrar el modal, terminar la
    // venta). Es el mismo patron que FormulaSecretaElevaciones: logica pura sobre un string que el controller guarda
    // en ISession, sin HttpContext ni reloj global, para poder probarla en Negocio.Tests.
    //
    // Cada autorizacion se identifica por (pestana de POS, accion, registro): la pestana evita que valga en otra
    // pestana o en otra cuenta; el registro (id de venta o de pago, 0 si la accion no es sobre un registro) evita que
    // una autorizacion para modificar una venta sirva para otra. Guarda solo ids y el vencimiento, nunca al usuario.
    // Formato del estado: "posInstanceId,accion,idRegistro,idAutorizador,idAuditoria,expiraTicksUtc;..."
    public static class AutorizacionesProvisoriasPos
    {
        public const string AccionModificarVenta = "ModificarVenta";
        public const string AccionBonificar = "Bonificar";
        public const string AccionCtaCte = "CtaCte";
        public const string AccionPago = "Pago";
        public const string AccionMisVentas = "MisVentas";

        private const char SeparadorEntradas = ';';
        private const char SeparadorCampos = ',';

        // Acciones que se pueden autorizar (el cliente manda el texto: se valida contra esta lista).
        public static bool EsAccionValida(string accion) =>
            accion == AccionModificarVenta || accion == AccionBonificar || accion == AccionCtaCte
            || accion == AccionPago || accion == AccionMisVentas;

        public sealed class Autorizacion
        {
            public string PosInstanceId;
            public string Accion;
            public int IdRegistro;
            public int IdAutorizador;
            public int IdAuditoria;
            public long ExpiraTicksUtc;
        }

        // Agrega (o renueva) la autorizacion de esa accion sobre ese registro. No toca las demas. Las ya vencidas se
        // descartan (el llamador las cierra en la auditoria con ExtraerVencidas antes de llamar aca).
        public static string Registrar(string estado, string posInstanceId, string accion, int idRegistro,
            int idAutorizador, int idAuditoria, DateTime ahoraUtc, TimeSpan duracion)
        {
            if (!EsClaveValida(posInstanceId) || !EsAccionValida(accion))
                throw new ArgumentException("Pestana o accion invalida.");

            var entradas = Parsear(estado, ahoraUtc, out _);
            entradas.RemoveAll(e => Coincide(e, posInstanceId, accion, idRegistro));
            entradas.Add(new Autorizacion
            {
                PosInstanceId = posInstanceId,
                Accion = accion,
                IdRegistro = idRegistro,
                IdAutorizador = idAutorizador,
                IdAuditoria = idAuditoria,
                ExpiraTicksUtc = ahoraUtc.Add(duracion).Ticks
            });
            return Serializar(entradas);
        }

        // La autorizacion vigente (no vencida) de esa accion sobre ese registro, o null.
        public static Autorizacion Vigente(string estado, string posInstanceId, string accion, int idRegistro, DateTime ahoraUtc)
        {
            if (!EsClaveValida(posInstanceId)) return null;
            return Parsear(estado, ahoraUtc, out _).FirstOrDefault(e => Coincide(e, posInstanceId, accion, idRegistro));
        }

        // Quita autorizaciones de una pestana. accion null = todas las de la pestana (al cerrar el POS);
        // idRegistro null = de cualquier registro de esa accion. idsAuditoria = las filas de auditoria que hay que cerrar.
        public static string Revocar(string estado, string posInstanceId, string accion, int? idRegistro,
            DateTime ahoraUtc, out List<int> idsAuditoria)
        {
            var entradas = Parsear(estado, ahoraUtc, out _);
            var quitar = entradas.Where(e => e.PosInstanceId == posInstanceId
                && (accion == null || e.Accion == accion)
                && (!idRegistro.HasValue || e.IdRegistro == idRegistro.Value)).ToList();

            idsAuditoria = quitar.Select(e => e.IdAuditoria).ToList();
            entradas.RemoveAll(e => quitar.Contains(e));
            return Serializar(entradas);
        }

        // Saca del estado las ya vencidas y devuelve sus filas de auditoria para cerrarlas como "vencido".
        public static string ExtraerVencidas(string estado, DateTime ahoraUtc, out List<int> idsAuditoriaVencidas)
        {
            var vigentes = Parsear(estado, ahoraUtc, out var vencidas);
            idsAuditoriaVencidas = vencidas.Select(e => e.IdAuditoria).ToList();
            return Serializar(vigentes);
        }

        private static bool Coincide(Autorizacion e, string posInstanceId, string accion, int idRegistro) =>
            e.PosInstanceId == posInstanceId && e.Accion == accion && e.IdRegistro == idRegistro;

        // El id de pestana viaja en el estado separado por comas: no puede traer separadores ni estar vacio.
        private static bool EsClaveValida(string posInstanceId) =>
            !string.IsNullOrWhiteSpace(posInstanceId)
            && posInstanceId.IndexOf(SeparadorCampos) < 0 && posInstanceId.IndexOf(SeparadorEntradas) < 0;

        // Estado corrupto o manipulado: se ignora la entrada mala (nunca se asume una autorizacion).
        private static List<Autorizacion> Parsear(string estado, DateTime ahoraUtc, out List<Autorizacion> vencidas)
        {
            var vigentes = new List<Autorizacion>();
            vencidas = new List<Autorizacion>();
            if (string.IsNullOrEmpty(estado)) return vigentes;

            long ahora = ahoraUtc.Ticks;
            foreach (var texto in estado.Split(SeparadorEntradas))
            {
                var partes = texto.Split(SeparadorCampos);
                if (partes.Length != 6) continue;

                if (!EsClaveValida(partes[0]) || !EsAccionValida(partes[1])) continue;
                if (!int.TryParse(partes[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int idRegistro)) continue;
                if (!int.TryParse(partes[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int idAutorizador)) continue;
                if (!int.TryParse(partes[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int idAuditoria)) continue;
                if (!long.TryParse(partes[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out long expira)) continue;
                if (idAutorizador <= 0) continue;

                var entrada = new Autorizacion
                {
                    PosInstanceId = partes[0], Accion = partes[1], IdRegistro = idRegistro,
                    IdAutorizador = idAutorizador, IdAuditoria = idAuditoria, ExpiraTicksUtc = expira
                };
                if (expira <= ahora) vencidas.Add(entrada); else vigentes.Add(entrada);
            }
            return vigentes;
        }

        private static string Serializar(List<Autorizacion> entradas)
        {
            return string.Join(SeparadorEntradas.ToString(), entradas.Select(e =>
                e.PosInstanceId + SeparadorCampos + e.Accion + SeparadorCampos
                + e.IdRegistro.ToString(CultureInfo.InvariantCulture) + SeparadorCampos
                + e.IdAutorizador.ToString(CultureInfo.InvariantCulture) + SeparadorCampos
                + e.IdAuditoria.ToString(CultureInfo.InvariantCulture) + SeparadorCampos
                + e.ExpiraTicksUtc.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
