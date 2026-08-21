namespace NegocioTests.Fakes
{
    // Registra si Completar()/Dispose() se llamaron, para verificar el contrato real que debe
    // cumplir cada wrapper agregarX/AddOrEditX en Negocio (ver Contratos/IUnitOfWork.cs): sobre
    // exito, Completar() antes de Dispose(); sobre excepcion, Dispose() sin Completar() previo
    // (equivale al rollback implicito de UnitOfWorkPg.Dispose cuando no se llamo Completar()).
    public sealed class FakeUnitOfWork : Contratos.IUnitOfWork
    {
        public bool CompletarLlamado { get; private set; }
        public bool DisposeLlamado { get; private set; }

        public void Completar() => CompletarLlamado = true;

        public void Dispose() => DisposeLlamado = true;
    }
}
