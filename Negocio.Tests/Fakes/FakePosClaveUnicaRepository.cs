using System;
using System.Collections.Generic;
using Entidades;

namespace NegocioTests.Fakes
{
    // Fake en memoria de IPosClaveUnicaRepository con la misma regla que el indice unico real: un clientId repetido tira
    // ClaveDuplicadaException. ClaveGanadoraConcurrente simula que otra subida simultanea grabo la clave entre el chequeo
    // previo y el INSERT de esta (la carrera que cubre el catch de agregarVentaConClave).
    public sealed class FakePosClaveUnicaRepository : Contratos.IPosClaveUnicaRepository
    {
        public Dictionary<Guid, PosClaveUnica> Claves { get; } = new Dictionary<Guid, PosClaveUnica>();
        public PosClaveUnica ClaveGanadoraConcurrente { get; set; }
        public int Inserciones { get; private set; }
        public Contratos.IUnitOfWork UltimoUnitOfWork { get; private set; }

        public PosClaveUnica ObtenerPorClientId(Guid clientId)
        {
            return Claves.TryGetValue(clientId, out var clave) ? clave : null;
        }

        public void Insertar(PosClaveUnica clave, Contratos.IUnitOfWork unitOfWork = null)
        {
            UltimoUnitOfWork = unitOfWork;
            if (ClaveGanadoraConcurrente != null && !Claves.ContainsKey(ClaveGanadoraConcurrente.ClientId))
                Claves[ClaveGanadoraConcurrente.ClientId] = ClaveGanadoraConcurrente;

            if (Claves.ContainsKey(clave.ClientId))
                throw new Contratos.ClaveDuplicadaException(clave.ClientId, null);

            Inserciones++;
            Claves[clave.ClientId] = clave;
        }

        public List<PosClaveUnica> ListarVentasOfflineEnRango(int idUsuario, int idSucursal, DateTime desde, DateTime hasta)
        {
            var lista = new List<PosClaveUnica>();
            foreach (var c in Claves.Values)
                if (c.Tipo == PosClaveUnica.TipoVenta && c.Origen == PosClaveUnica.OrigenOffline && c.IdUsuario == idUsuario
                    && c.IdSucursal == idSucursal && c.FechaLocal >= desde && c.FechaLocal <= hasta)
                    lista.Add(c);
            return lista;
        }
    }
}
