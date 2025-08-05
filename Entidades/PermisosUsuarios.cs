using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class PermisosUsuarios
    {
        public int IdUsuario { get; set; }

        public int IdForm { get; set; }

        public int DiasPermitidosVer { get; set; } = -1;

        public int DiasPermitidosEditar { get; set; } = -1;

        public bool SoloRegistrosPropios { get; set; } = false;
    }
}
