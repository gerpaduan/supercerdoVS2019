using System;
using System.Collections.Generic;
using Utilidades;

namespace Negocio
{
    public class DispositivoSeguro
    {
        private readonly Contratos.IDispositivoSeguroRepository oDispositivoD;

        public DispositivoSeguro(IEmpresaContext empresa)
        {
            oDispositivoD = new Datos.DispositivoSeguro(empresa);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de
        // IDispositivoSeguroRepository (ej. DatosPostgres.DispositivoSeguroPg).
        public DispositivoSeguro(Contratos.IDispositivoSeguroRepository repositorio)
        {
            oDispositivoD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public List<Entidades.DispositivoSeguro> Listar(int idEmpresa)
        {
            return oDispositivoD.Listar(idEmpresa);
        }

        public void Agregar(Entidades.DispositivoSeguro dispositivo)
        {
            if (dispositivo == null) throw new ArgumentNullException(nameof(dispositivo));
            if (string.IsNullOrWhiteSpace(dispositivo.NumeroSerie))
                throw new ArgumentException("El número de serie es obligatorio.", nameof(dispositivo));

            dispositivo.NumeroSerie = dispositivo.NumeroSerie.Trim();
            dispositivo.CreadoUtc = DateTime.UtcNow;
            oDispositivoD.Agregar(dispositivo);
        }

        public void Eliminar(int id, int idEmpresa)
        {
            oDispositivoD.Eliminar(id, idEmpresa);
        }

        public bool ExisteSerieSegura(string numeroSerie, int idEmpresa)
        {
            return oDispositivoD.ExisteSerieSegura(numeroSerie, idEmpresa);
        }
    }
}
