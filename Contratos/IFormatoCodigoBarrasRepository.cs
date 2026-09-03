using System.Collections.Generic;

namespace Contratos
{
    // CRUD de configuracion de codigos de barra internos (balanza, prefijo EAN 20-29) por
    // empresa. Consumido por Negocio.FormatoCodigoBarras (pantalla de configuracion) y por
    // Negocio.BarcodeInterpreter (ObtenerActivoPorPrefijo, en la busqueda del POS).
    public interface IFormatoCodigoBarrasRepository
    {
        List<Entidades.FormatoCodigoBarras> Listar(int idEmpresa);
        Entidades.FormatoCodigoBarras ObtenerPorId(int id, int idEmpresa);
        Entidades.FormatoCodigoBarras ObtenerActivoPorPrefijo(int idEmpresa, int prefijo);
        bool ExistePrefijo(int idEmpresa, int prefijo, int idExcluir);
        void Agregar(Entidades.FormatoCodigoBarras formato);
        void Actualizar(Entidades.FormatoCodigoBarras formato);
    }
}
