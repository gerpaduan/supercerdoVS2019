namespace Contratos
{
    // Subconjunto angosto de ICorteRepository (2 de sus 40+ metodos): solo la busqueda exacta
    // de producto por codigo, que es todo lo que necesita Negocio.BarcodeInterpreter. Evita
    // atar el motor de interpretacion de codigos de barra al resto de ICorteRepository
    // (Movimientos, Formulas, Embutidos, reportes de stock, etc.), que no tiene nada que ver
    // con esta feature. Datos.Corte y DatosPostgres.CortePg ya implementan estos 2 metodos con
    // esta firma exacta -- declarar la interfaz solo agrega la conformidad, sin logica nueva.
    public interface ICorteBusquedaSimpleRepository
    {
        Entidades.Corte findCorteByCodigo(long codigo, bool buscarMaestro);
        Entidades.Corte findCorteByCodigoEmpresa(long codigo, int idEmpresa, bool buscarMaestro);
    }
}
