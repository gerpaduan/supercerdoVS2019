using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja Datos.Parametros completo (5/5 metodos). Primera vez que se toca este archivo --
    // es la capa de datos detras de Negocio.Parametros (implementa IParametrosContext, usado en
    // toda la app para leer parametros de configuracion por empresa). ObtenerValor/SetValor no
    // tienen caller real hoy (solo Negocio.Parametros usa ObtenerGrid/GuardarGrid/
    // ObtenerDiccionario) -- se migran igual por completitud de la interfaz. Ver docs/DECISIONS.md.
    public interface IParametrosRepository
    {
        DataTable ObtenerGrid();
        void GuardarGrid(DataTable dtParametros);
        Dictionary<string, string> ObtenerDiccionario();
        string ObtenerValor(string nombreParametro);
        void SetValor(string nombreParametro, string valor);
    }
}
