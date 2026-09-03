using System;
using System.Collections.Generic;
using Utilidades;

namespace Negocio
{
    // CRUD de configuracion para la pantalla "Codigos de barra" (formatos de codigo interno de
    // balanza, por empresa y por prefijo EAN 20-29). Valida ANTES de persistir -- ver
    // Web/Controllers/CodigosBarraController.cs. La interpretacion en tiempo real de un codigo
    // escaneado vive en Negocio.BarcodeInterpreter, no aca.
    public class FormatoCodigoBarras
    {
        private const int LongitudEanConPrefijoInterno = 13;
        private const int PosicionMinimaCodigoOValor = 3; // no puede pisar el prefijo (posiciones 1-2)
        private const int CantidadDecimalesMaxima = 4;

        private readonly Contratos.IFormatoCodigoBarrasRepository oFormatoD;

        public FormatoCodigoBarras(IEmpresaContext empresa)
        {
            oFormatoD = new Datos.FormatoCodigoBarras(empresa);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de
        // IFormatoCodigoBarrasRepository (ej. DatosPostgres.FormatoCodigoBarrasPg).
        public FormatoCodigoBarras(Contratos.IFormatoCodigoBarrasRepository repositorio)
        {
            oFormatoD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public List<Entidades.FormatoCodigoBarras> Listar(int idEmpresa)
        {
            return oFormatoD.Listar(idEmpresa);
        }

        public Entidades.FormatoCodigoBarras ObtenerPorId(int id, int idEmpresa)
        {
            return oFormatoD.ObtenerPorId(id, idEmpresa);
        }

        public void Agregar(Entidades.FormatoCodigoBarras formato)
        {
            Validar(formato);
            if (oFormatoD.ExistePrefijo(formato.IdEmpresa, formato.Prefijo, idExcluir: 0))
                throw new ArgumentException($"Ya existe un formato para el prefijo {formato.Prefijo} en esta empresa. Edítelo en lugar de crear uno nuevo.");

            formato.CreadoUtc = DateTime.UtcNow;
            oFormatoD.Agregar(formato);
        }

        public void Actualizar(Entidades.FormatoCodigoBarras formato)
        {
            Validar(formato);
            if (oFormatoD.ExistePrefijo(formato.IdEmpresa, formato.Prefijo, formato.Id))
                throw new ArgumentException($"Ya existe otro formato para el prefijo {formato.Prefijo} en esta empresa.");

            formato.ModificadoUtc = DateTime.UtcNow;
            oFormatoD.Actualizar(formato);
        }

        private static void Validar(Entidades.FormatoCodigoBarras formato)
        {
            if (formato == null) throw new ArgumentNullException(nameof(formato));

            if (string.IsNullOrWhiteSpace(formato.Nombre) || formato.Nombre.Trim().Length > 100)
                throw new ArgumentException("El nombre del formato es obligatorio (máximo 100 caracteres).");

            if (formato.Prefijo < Negocio.BarcodeInterpreter.PrefijoMin || formato.Prefijo > Negocio.BarcodeInterpreter.PrefijoMax)
                throw new ArgumentException($"El prefijo debe estar entre {Negocio.BarcodeInterpreter.PrefijoMin} y {Negocio.BarcodeInterpreter.PrefijoMax} (rango reservado para códigos internos).");

            if (formato.LongitudTotal != LongitudEanConPrefijoInterno)
                throw new ArgumentException($"La longitud total debe ser {LongitudEanConPrefijoInterno} (formato EAN-13).");

            if (formato.PosicionCodigo < PosicionMinimaCodigoOValor || formato.PosicionValor < PosicionMinimaCodigoOValor)
                throw new ArgumentException("La posición del código de producto/valor no puede superponerse con el prefijo (posiciones 1-2).");

            if (formato.LongitudCodigo < 1 || formato.LongitudValor < 1)
                throw new ArgumentException("Las longitudes deben ser mayores a cero.");

            int finCodigo = formato.PosicionCodigo + formato.LongitudCodigo - 1;
            int finValor = formato.PosicionValor + formato.LongitudValor - 1;

            if (finCodigo > formato.LongitudTotal - 1)
                throw new ArgumentException("El código de producto no puede incluir la posición del dígito verificador (posición 13).");

            if (finValor > formato.LongitudTotal - 1)
                throw new ArgumentException("El valor no puede incluir la posición del dígito verificador (posición 13).");

            bool seSuperponen = formato.PosicionCodigo <= finValor && formato.PosicionValor <= finCodigo;
            if (seSuperponen)
                throw new ArgumentException("Los rangos de código de producto y valor no pueden superponerse.");

            if (formato.CantidadDecimales < 0 || formato.CantidadDecimales > CantidadDecimalesMaxima)
                throw new ArgumentException($"La cantidad de decimales debe estar entre 0 y {CantidadDecimalesMaxima}.");
        }
    }
}
