using System;
using System.Collections.Generic;
using System.Data;

namespace NegocioTests.Fakes
{
    // Fake minimo de IPersonaRepository para los tests de "cuenta corriente reservada": solo los dos
    // metodos de ids reservados/ocultos tienen cuerpo (devuelven lo configurado y registran con que
    // parametros los llamaron). El resto tira NotImplementedException, igual que los demas fakes.
    public sealed class FakePersonaRepository : Contratos.IPersonaRepository
    {
        public HashSet<int> PersonasReservadas { get; set; } = new HashSet<int>();
        public HashSet<int> RegistrosOcultos { get; set; } = new HashSet<int>();

        public int LlamadasIdsOcultos { get; private set; }
        public string UltimaTabla { get; private set; }
        public int UltimoIdUsuario { get; private set; }
        public DateTime UltimoDesde { get; private set; }

        public HashSet<int> idsPersonasReservadas() => PersonasReservadas;

        public HashSet<int> idsRegistrosOcultos(string tabla, int idUsuario, DateTime desde)
        {
            LlamadasIdsOcultos++;
            UltimaTabla = tabla;
            UltimoIdUsuario = idUsuario;
            UltimoDesde = desde;
            return RegistrosOcultos;
        }

        public void addOrEditPersona(Entidades.Persona oPersonaE) => throw new NotImplementedException();
        public int addOrEditPersonaConId(Entidades.Persona oPersonaE) => throw new NotImplementedException();
        public void eliminarPersona(Entidades.Persona oPersonaE) => throw new NotImplementedException();
        public Entidades.Persona findById(int id) => throw new NotImplementedException();
        public DataTable buscarProveedor(string buscarTexto) => throw new NotImplementedException();
        public DataTable buscarPersona(string buscarTexto, bool? marca) => throw new NotImplementedException();
        public DataTable getIva() => throw new NotImplementedException();
        public int existeCuit(string cuit) => throw new NotImplementedException();
        public bool personaTieneCompras_Ventas(int idPersona) => throw new NotImplementedException();
        public DataTable obtenerProveedores() => throw new NotImplementedException();
        public DataTable obtenerProveedoresConCompras() => throw new NotImplementedException();
        public DataTable existenMarcasParecidas(string buscarTexto, int idMarca) => throw new NotImplementedException();
    }
}
