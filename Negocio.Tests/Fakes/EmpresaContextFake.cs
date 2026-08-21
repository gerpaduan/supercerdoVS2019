using Utilidades;

namespace NegocioTests.Fakes
{
    public sealed class EmpresaContextFake : IEmpresaContext
    {
        public EmpresaContextFake(int idEmpresa) => IdEmpresa = idEmpresa;
        public int IdEmpresa { get; }
    }
}
