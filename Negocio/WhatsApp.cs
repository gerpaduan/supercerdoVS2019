using System;
using Entidades;
using Utilidades;

namespace Negocio
{
    public class WhatsApp
    {
        private readonly Datos.WhatsApp _datos;

        public WhatsApp(IEmpresaContext empresa)
        {
            if (empresa == null) throw new ArgumentNullException(nameof(empresa));
            _datos = new Datos.WhatsApp(empresa);
        }

        public ConfiguracionWhatsApp ObtenerOCrearConfiguracion(int? idUsuario)
        {
            return _datos.ObtenerOCrearConfiguracion(idUsuario);
        }

        public ConfiguracionWhatsApp ObtenerConfiguracion()
        {
            return _datos.ObtenerConfiguracion();
        }

        public void GuardarConfiguracion(ConfiguracionWhatsApp configuracion)
        {
            _datos.GuardarConfiguracion(configuracion);
        }
    }
}
