using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Entidades;

namespace WebCore.Services
{
    // Cuenta corriente reservada (docs/DECISIONS.md, 2026-10-01).
    //
    // Una persona con Persona.CtaCteReservada = true tiene sus movimientos (ventas, compras,
    // cobros, pagos y movimientos de cta cte) visibles solo para un usuario "autorizado" (admin o
    // con el permiso formCtasCtes). Cualquier otro usuario es "restringido": sigue pudiendo
    // venderle/cobrarle/pagarle, pero solo ve lo que cargo el mismo desde la apertura de su
    // caja. Este servicio concentra esa regla para que los controllers no la dupliquen:
    //   - ObtenerRestriccion: null si el usuario es autorizado; si no, {operador, apertura de caja}.
    //   - IdsOcultos / RegistroOculto: ids de registros que el restringido no debe ver.
    //   - PersonaReservada: si una persona es reservada.
    // Todo se cachea por request (instancia scoped).
    public interface ICtaCteReservadaService
    {
        // true si el usuario de sesion NO es admin ni tiene formCtasCtes.
        bool EsRestringido { get; }

        // null si el usuario es autorizado (no hay nada que ocultar). Si es restringido devuelve
        // el operador y la hora de apertura de SU caja abierta (SinCajaAbierta si no tiene).
        // `operador` es quien carga los registros: en la cuenta compartida de produccion es el
        // operador resuelto por step-up; si es null se usa el usuario de sesion.
        RestriccionCtaCteReservada ObtenerRestriccion(Usuario operador = null);

        // Ids de los registros de `tabla` (RestriccionCtaCteReservada.Tabla*) que el usuario
        // restringido no puede ver. Vacio si es autorizado.
        HashSet<int> IdsOcultos(string tabla, Usuario operador = null);

        // true si el registro `idRegistro` de `tabla` esta oculto para el usuario actual.
        bool RegistroOculto(string tabla, int idRegistro, Usuario operador = null);

        // true si la persona es "cuenta corriente reservada" (con independencia del usuario).
        bool PersonaReservada(int idPersona);

        // true si el usuario restringido no debe ver el saldo/estado de cuenta de esta persona.
        bool SaldoOculto(int idPersona);

        // true si hay que avisar en un listado que algunas cuentas estan reservadas: el usuario es
        // restringido y existe al menos una persona reservada. No cuenta lo que realmente se oculto
        // (revelaria cuantos movimientos hay) y asi el aviso no depende del rango de fechas ni de la
        // busqueda AJAX de la pantalla.
        bool MostrarAvisoEnListados { get; }
    }

    public sealed class CtaCteReservadaService : ICtaCteReservadaService
    {
        private readonly IUsuarioSesionService _sesion;
        private readonly Negocio.Persona _personaN;
        private readonly Negocio.Usuario _usuarioN;
        private readonly Negocio.Sucursal _sucursalN;
        private readonly Negocio.CierreCaja _cierreN;

        private bool? _esRestringido;
        private HashSet<int> _personasReservadas;
        private readonly Dictionary<string, HashSet<int>> _idsOcultosPorTabla = new Dictionary<string, HashSet<int>>();
        private readonly Dictionary<int, RestriccionCtaCteReservada> _restriccionPorOperador = new Dictionary<int, RestriccionCtaCteReservada>();

        private readonly IHttpContextAccessor _http;
        private readonly ILogger<CtaCteReservadaService> _log;

        public CtaCteReservadaService(IUsuarioSesionService sesion, IHttpContextAccessor http, ILogger<CtaCteReservadaService> log)
        {
            _sesion = sesion;
            _http = http;
            _log = log;
            var empresa = sesion.Empresa;
            var param = sesion.Parametros;
            _personaN = Infrastructure.NegocioFactory.CrearPersona(empresa, param);
            _usuarioN = Infrastructure.NegocioFactory.CrearUsuario(empresa, param);
            _sucursalN = Infrastructure.NegocioFactory.CrearSucursal(empresa, param);
            _cierreN = Infrastructure.NegocioFactory.CrearCierreCaja(empresa, param);
        }

        public bool EsRestringido
        {
            get
            {
                if (_esRestringido == null)
                {
                    var user = _sesion.UsuarioActual;
                    // tienePermiso devuelve true para Admin de entrada (Negocio/Usuario.cs).
                    _esRestringido = !_usuarioN.tienePermiso(user, Permisos.Finanza.VerCtasCtes, DateTime.Today, -1);
                }
                return _esRestringido.Value;
            }
        }

        public RestriccionCtaCteReservada ObtenerRestriccion(Usuario operador = null)
        {
            if (!EsRestringido) return null;

            var quien = operador ?? ResolverOperadorEfectivo();
            if (_restriccionPorOperador.TryGetValue(quien.Id, out var cacheada)) return cacheada;

            var restriccion = new RestriccionCtaCteReservada
            {
                IdUsuario = quien.Id,
                Desde = ObtenerAperturaCajaAbierta(quien) ?? RestriccionCtaCteReservada.SinCajaAbierta
            };
            _restriccionPorOperador[quien.Id] = restriccion;
            return restriccion;
        }

        public HashSet<int> IdsOcultos(string tabla, Usuario operador = null)
        {
            var restriccion = ObtenerRestriccion(operador);
            if (restriccion == null) return new HashSet<int>();

            // La clave incluye al operador: con la cuenta compartida de produccion pueden
            // convivir varios operadores en la misma sesion.
            string clave = tabla + "|" + restriccion.IdUsuario;
            if (!_idsOcultosPorTabla.TryGetValue(clave, out var ids))
            {
                ids = _personaN.idsRegistrosOcultos(tabla, restriccion);
                _idsOcultosPorTabla[clave] = ids;
            }
            return ids;
        }

        public bool RegistroOculto(string tabla, int idRegistro, Usuario operador = null)
        {
            if (!EsRestringido) return false;
            return IdsOcultos(tabla, operador).Contains(idRegistro);
        }

        public bool PersonaReservada(int idPersona)
        {
            return PersonasReservadas().Contains(idPersona);
        }

        public bool MostrarAvisoEnListados => EsRestringido && PersonasReservadas().Count > 0;

        private HashSet<int> PersonasReservadas()
        {
            if (_personasReservadas == null) _personasReservadas = _personaN.idsPersonasReservadas();
            return _personasReservadas;
        }

        public bool SaldoOculto(int idPersona)
        {
            return EsRestringido && PersonaReservada(idPersona);
        }

        // Quien carga los registros. Un usuario normal es el de sesion. En la cuenta compartida de
        // produccion el operador real se autentica por step-up y queda en Session
        // (OperadorPOS_<posInstanceId> / OperadorModulo_<modulo>, ver VentasController y
        // CajasController): si hay exactamente UN operador distinto en la sesion se usa ese; si hay
        // cero o varios (ambiguo) se cae al usuario compartido, cuyo id no coincide con ningun
        // creador -> falla cerrado (no ve nada reservado).
        private Usuario ResolverOperadorEfectivo()
        {
            var usuarioSesion = _sesion.UsuarioActual;
            if (!usuarioSesion.EsUsuarioProduccion) return usuarioSesion;

            var session = _http.HttpContext?.Session;
            if (session == null) return usuarioSesion;

            var operadores = new Dictionary<int, Usuario>();
            foreach (var clave in session.Keys.Where(k => k.StartsWith("OperadorPOS_", StringComparison.Ordinal)
                                                      || k.StartsWith("OperadorModulo_", StringComparison.Ordinal)))
            {
                var json = session.GetString(clave);
                if (string.IsNullOrEmpty(json)) continue;

                try
                {
                    var operador = JsonSerializer.Deserialize<Usuario>(json);
                    if (operador != null && operador.Id > 0) operadores[operador.Id] = operador;
                }
                catch (JsonException ex)
                {
                    // Un operador ilegible no cuenta: queda el usuario compartido (falla cerrado).
                    _log.LogWarning(ex, "Operador ilegible en Session ({Clave}); se ignora para cuenta corriente reservada.", clave);
                }
            }

            return operadores.Count == 1 ? operadores.Values.First() : usuarioSesion;
        }

        // Hora de apertura de la caja abierta del operador (misma busqueda que
        // FinanzasController/VentasController.ObtenerCajaAbiertaUsuario: ultima caja de su
        // sucursal + usuario, abierta si todavia no tiene usuario de cierre). null si no tiene.
        private DateTime? ObtenerAperturaCajaAbierta(Usuario operador)
        {
            if (operador == null || operador.IdSucursal == 0) return null;

            var sucursal = operador.Sucursal != null && operador.Sucursal.IdSucursal == operador.IdSucursal
                ? operador.Sucursal
                : _sucursalN.findById(operador.IdSucursal);
            if (sucursal == null || sucursal.IdSucursal == 0) return null;

            var cierre = _cierreN.findByIdOrLast(
                new CierreCaja { Sucursal = sucursal, UsuarioInicio = operador },
                CierreCaja.tipoBusqueda.FindLast, "");

            bool abierta = cierre != null
                && !cierre.FechaHoraCierre.HasValue
                && (cierre.UsuarioCierre == null || cierre.UsuarioCierre.Id == 0);
            return abierta ? cierre.FechaHoraInicio : null;
        }
    }
}
