using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilidades;

namespace wsAFIPvs2008
{
    public class EmpresaContextWin : IEmpresaContext
    {
        public int IdEmpresa { get; }

        public EmpresaContextWin(int idEmpresa)
        {
            if (idEmpresa <= 0)
                throw new Exception("IdEmpresa inválido");

            IdEmpresa = idEmpresa;
        }
    }


}
