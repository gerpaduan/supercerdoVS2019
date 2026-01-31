using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilidades;
using System.Web;

namespace Web
{
    public class EmpresaContextWeb : IEmpresaContext
    {
        public int IdEmpresa
        {
            get
            {
                var session = HttpContext.Current?.Session;

                if (session == null || session["IdEmpresa"] == null)
                    throw new Exception("Empresa no inicializada en sesión");

                return (int)session["IdEmpresa"];
            }
        }
    }

}
