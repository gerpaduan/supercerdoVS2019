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
        string receta;
        private DateTime? creado;
        private DateTime? actualizado;
        private Usuario creadoPor;
        private Usuario actualizadoPor;
        private bool ajustarUnidad;
        List<CortePorFormula> listaCortesEnFormula = new List<CortePorFormula>();

        public int IdFormula { get => idFormula; set => idFormula = value; }
        public Corte Embutido { get => embutido; set => embutido = value; }
        public DateTime? Creado { get => creado; set => creado = value; }
        public DateTime? Actualizado { get => actualizado; set => actualizado = value; }
        public Usuario CreadoPor { get => creadoPor; set => creadoPor = value; }
        public Usuario ActualizadoPor { get => actualizadoPor; set => actualizadoPor = value; }
        public List<CortePorFormula> ListaCortesEnFormula { get => listaCortesEnFormula; set => listaCortesEnFormula = value; }
        public string Receta { get => receta; set => receta = value; }
        // Modo A del Ajuste de Formula: si esta activo (o el elaborado es de Ingreso Rapido,
        // comportamiento preexistente), la fila de ajuste se calcula para que la formula sume
        // exactamente 1 unidad. Ver Negocio/Corte.cs, NormalizarFormulaElaborado.
        public bool AjustarUnidad { get => ajustarUnidad; set => ajustarUnidad = value; }
    }
}
