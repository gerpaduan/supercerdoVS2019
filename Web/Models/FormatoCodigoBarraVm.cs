using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    // ViewModels de la pantalla "Codigos de barra" (Configuracion): formatos de codigo interno
    // de balanza (EAN-13, prefijo 20-29) por empresa. Ver Web/Controllers/
    // CodigosBarraController.cs y Negocio/FormatoCodigoBarras.cs.
    public class FormatosCodigoBarraIndexVm
    {
        public FormatosCodigoBarraIndexVm()
        {
            Items = new List<Entidades.FormatoCodigoBarras>();
        }

        public bool PuedeAdministrar { get; set; }
        public List<Entidades.FormatoCodigoBarras> Items { get; set; }
    }

    public class FormatoCodigoBarraEditVm
    {
        public int Id { get; set; }

        // false solo al crear -- una vez guardado, el Prefijo queda de solo lectura (cambiarlo
        // se resuelve dando de baja este formato y creando uno nuevo, ver Negocio/
        // FormatoCodigoBarras.cs).
        public bool EsNuevo { get; set; }

        [Required(ErrorMessage = "Ingresá un nombre para el formato.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Range(20, 29, ErrorMessage = "El prefijo debe estar entre 20 y 29.")]
        [Display(Name = "Prefijo")]
        public int Prefijo { get; set; }

        [Display(Name = "Longitud total")]
        public int LongitudTotal { get; set; } = 13;

        [Range(1, 13, ErrorMessage = "Posición inválida.")]
        [Display(Name = "Posición del código de producto")]
        public int PosicionCodigo { get; set; }

        [Range(1, 13, ErrorMessage = "Longitud inválida.")]
        [Display(Name = "Longitud del código de producto")]
        public int LongitudCodigo { get; set; }

        [Range(1, 13, ErrorMessage = "Posición inválida.")]
        [Display(Name = "Posición del valor")]
        public int PosicionValor { get; set; }

        [Range(1, 13, ErrorMessage = "Longitud inválida.")]
        [Display(Name = "Longitud del valor")]
        public int LongitudValor { get; set; }

        [Display(Name = "Tipo de valor")]
        public Entidades.TipoValorCodigoBarras TipoValor { get; set; }

        [Range(0, 4, ErrorMessage = "La cantidad de decimales debe estar entre 0 y 4.")]
        [Display(Name = "Cantidad de decimales")]
        public int CantidadDecimales { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;
    }
}
