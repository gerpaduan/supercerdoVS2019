using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Entidades;

namespace NegocioTests.Fakes
{
    // Implementacion en memoria de ICuentaCorrienteRepository, solo para tests unitarios de
    // Negocio.CuentaCorriente -- ningun metodo toca SQL Server ni Postgres.
    //
    // getMovCtaCteBy/addOrEditMovCtaCte replican la semantica EXACTA de
    // DatosPostgres.CuentaCorrientePg (verificada leyendo su SQL real): TablaAndId busca el
    // ULTIMO registro por Id descendente para esa Tabla+IdTabla (sin filtrar por QuitadoCtaCta:
    // ese filtro lo aplica crearMovCtaCte, no el repositorio); addOrEditMovCtaCte inserta
    // (Id==0, asigna un Id nuevo autoincremental) o actualiza (Id!=0) in-place.
    //
    // El resto de los metodos de la interfaz no participan de crearMovCtaCte y quedan sin
    // implementar (NotImplementedException) -- si un test futuro los necesita, se implementan
    // ahi, no antes.
    public sealed class FakeCuentaCorrienteRepository : Contratos.ICuentaCorrienteRepository
    {
        public List<MovCtaCte> Movimientos { get; } = new List<MovCtaCte>();
        private int _nextId = 1;

        public Contratos.IUnitOfWork IniciarUnitOfWork() => null;

        public MovCtaCte getMovCtaCteBy(int id, MovCtaCte.tablas tabla, int idTabla, MovCtaCte.getBy getBy, Contratos.IUnitOfWork unitOfWork = null)
        {
            // Devuelve una COPIA, nunca la referencia guardada en Movimientos: una lectura real
            // de ADO.NET siempre materializa un objeto nuevo por fila. crearMovCtaCte muta el
            // objeto que recibe (oMovCtaCte.Id = 0, etc.) -- si fuera la misma referencia, esa
            // mutacion corromperia el registro ya persistido antes de "reinsertarlo".
            MovCtaCte encontrado = getBy == MovCtaCte.getBy.Id
                ? Movimientos.FirstOrDefault(m => m.Id == id)
                : Movimientos
                    .Where(m => m.Tabla == tabla.ToString() && m.IdTabla == idTabla)
                    .OrderByDescending(m => m.Id)
                    .FirstOrDefault();

            return encontrado == null ? null : Clonar(encontrado);
        }

        public MovCtaCte addOrEditMovCtaCte(MovCtaCte oMovCtaCteE, Contratos.IUnitOfWork unitOfWork = null)
        {
            if (oMovCtaCteE == null) throw new ArgumentNullException(nameof(oMovCtaCteE));

            if (oMovCtaCteE.Id == 0)
            {
                oMovCtaCteE.Id = _nextId++;
                // Guarda una copia -- igual que un INSERT real, cambios futuros al objeto
                // pasado por el caller no deben mutar el registro ya persistido.
                Movimientos.Add(Clonar(oMovCtaCteE));
            }
            else
            {
                int index = Movimientos.FindIndex(m => m.Id == oMovCtaCteE.Id);
                if (index >= 0)
                    Movimientos[index] = Clonar(oMovCtaCteE);
                else
                    Movimientos.Add(Clonar(oMovCtaCteE));
            }

            return oMovCtaCteE;
        }

        private static MovCtaCte Clonar(MovCtaCte origen) => new MovCtaCte
        {
            Id = origen.Id,
            Persona = origen.Persona,
            Fecha = origen.Fecha,
            Tabla = origen.Tabla,
            IdTabla = origen.IdTabla,
            NroDoc = origen.NroDoc,
            Detalle = origen.Detalle,
            Tipo = origen.Tipo,
            Importe = origen.Importe,
            Sucursal = origen.Sucursal,
            Creado = origen.Creado,
            CreadoPor = origen.CreadoPor,
            Actualizado = origen.Actualizado,
            ActualizadoPor = origen.ActualizadoPor,
            QuitadoCtaCta = origen.QuitadoCtaCta,
        };

        public DataTable obtenerCtasCtes(string txtBusqueda, int? idPersona, string ordenSaldo = "DESC") => throw new NotImplementedException();
        public DataTable obtenerResumenDashboard() => throw new NotImplementedException();
        public DataTable getCtaCteByIdPersona(int idPersona, DateTime fechaDesde) => throw new NotImplementedException();
        public DataTable obtenerCheques(string texto, DateTime fechaDesde, DateTime fechaHasta, bool soloPropios, string estado) => throw new NotImplementedException();
        public Cheque getChequePorIDorNro(int id, string nroCheque) => throw new NotImplementedException();
        public List<Cheque> getChequesPorPago(int idPago, bool conPagos = true, Contratos.IUnitOfWork unitOfWork = null) => throw new NotImplementedException();
        public bool AddOrEditCheque(Cheque oCheque) => throw new NotImplementedException();
        public bool EliminarCheque(int id) => throw new NotImplementedException();
        public bool resetearChequesAsignados(int idPago, Contratos.IUnitOfWork unitOfWork = null) => throw new NotImplementedException();
        public List<string> getBancos() => throw new NotImplementedException();
        public int getUltimoIdPago() => throw new NotImplementedException();
        public Pago addOrEditPago(Pago oPagoE, Contratos.IUnitOfWork unitOfWork = null) => throw new NotImplementedException();
        public void eliminarPago(Pago oPagoE) => throw new NotImplementedException();
        public DataTable obtenerPagos(string texto, DateTime fechaDesde, DateTime fechaHasta) => throw new NotImplementedException();
        public DataTable obtenerTotalesPagosBalance(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal) => throw new NotImplementedException();
        public DataTable obtenerUltimosPagosDashboard(int cantidad) => throw new NotImplementedException();
        public DataTable obtenerChequesPendientesDashboard(int cantidad, DateTime fechaActual) => throw new NotImplementedException();
        public Pago getPagoById(int idPago, bool conCheques = true) => throw new NotImplementedException();
    }
}
