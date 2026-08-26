using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class CortePorFormula
    {
        int idCorteEnFormula;
        Formula formula;
        Corte corteEnFormula;
        float porcentaje;
        bool agregarAuto;
        bool noSumaPeso;

        //parar poder agregar a la grilla
        int idCorte;
        long codigo;
        string corte;

        public int IdCorteEnFormula { get => idCorteEnFormula; set => idCorteEnFormula = value; }
        public Formula Formula { get => formula; set => formula = value; }
        public Corte CorteEnFormula { get => CorteEnFormula1; set => CorteEnFormula1 = value; }
        public Corte CorteEnFormula1 { get => corteEnFormula; set => corteEnFormula = value; }
        public float Porcentaje { get => porcentaje; set => porcentaje = value; }
        public bool AgregarAuto { get => agregarAuto; set => agregarAuto = value; }
        // Modo B del Ajuste de Formula (ingrediente que no suma al peso del elaborado, ej. tripa
        // en una formula de chorizo) -- ver Negocio/Corte.cs, NormalizarFormulaElaborado.
        public bool NoSumaPeso { get => noSumaPeso; set => noSumaPeso = value; }
        public int IdCorte { get => idCorte; set => idCorte = value; }
        public long Codigo { get => codigo; set => codigo = value; }
        public string Corte { get => corte; set => corte = value; }
    }
}
