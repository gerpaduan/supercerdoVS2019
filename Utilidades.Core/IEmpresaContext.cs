// TODO(claude): copia temporal desde Utilidades/IEmpresaContext.cs para desbloquear el spike
// de migracion a ASP.NET Core (ver docs/DECISIONS.md) sin tocar Utilidades.csproj, que tiene
// cambios sin commitear de otra sesion en paralelo. Cuando ese trabajo se commitee, reconciliar:
// mover (no duplicar) este archivo a este proyecto y que Utilidades.csproj referencie
// Utilidades.Core en su lugar, segun el plan real de extraccion.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public interface IEmpresaContext
    {
        int IdEmpresa { get; }
    }
}
