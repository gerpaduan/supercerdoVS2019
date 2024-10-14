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
        public int IdCorte { get => idCorte; set => idCorte = value; }
        public long Codigo { get => codigo; set => codigo = value; }
        public string Corte { get => corte; set => corte = value; }
    }
}
