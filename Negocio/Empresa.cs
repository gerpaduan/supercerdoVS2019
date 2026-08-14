using Utilidades;

namespace Negocio
{
    // Capa de negocio de Empresa. Wrapper fino sobre Datos.Empresa, mismo patron que
    // Negocio/Sucursal.cs -- toda la validacion de "quien puede editar" vive en el controller
    // (Web/Controllers/EmpresaController.cs), no aca.
    public class Empresa
    {
        private readonly Datos.Empresa oEmpresaD;

        public Empresa(IEmpresaContext empresa)
        {
            oEmpresaD = new Datos.Empresa(empresa);
        }

        public Entidades.Empresa findById(int idEmpresa)
        {
            return oEmpresaD.findById(idEmpresa);
        }

        public void ActualizarDatosBasicos(Entidades.Empresa oEmpresaE)
        {
            oEmpresaD.ActualizarDatosBasicos(oEmpresaE);
        }
    }
}
