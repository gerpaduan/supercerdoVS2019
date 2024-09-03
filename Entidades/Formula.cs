using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Formula
    {
        int idFormula;
        Corte embutido;
        private DateTime? creado;
        private DateTime? actualizado;
        private Usuario creadoPor;
        private Usuario actualizadoPor;
        List<CortePorFormula> listaCortesEnFormula = new List<CortePorFormula>();

        public int IdFormula { get => idFormula; set => idFormula = value; }
        public Corte Embutido { get => embutido; set => embutido = value; }
        public DateTime? Creado { get => creado; set => creado = value; }
        public DateTime? Actualizado { get => actualizado; set => actualizado = value; }
        public Usuario CreadoPor { get => creadoPor; set => creadoPor = value; }
        public Usuario ActualizadoPor { get => actualizadoPor; set => actualizadoPor = value; }
        public List<CortePorFormula> ListaCortesEnFormula { get => listaCortesEnFormula; set => listaCortesEnFormula = value; }
    }
}
