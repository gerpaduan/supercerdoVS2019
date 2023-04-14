namespace Presentacion.Caja
{
    partial class formAbrirCaja
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // txtCajaActual
            // 
            this.txtCajaCierre.ReadOnly = true;
            this.txtCajaCierre.Text = "";
            // 
            // txtEgresosCaja
            // 
            this.txtEgresosCaja.ReadOnly = true;
            this.txtEgresosCaja.Text = "";
            // 
            // txtVentas
            // 
            this.txtVentas.ReadOnly = true;
            this.txtVentas.Text = "";
            // 
            // txtDiferencia
            // 
            this.txtDiferencia.ReadOnly = true;
            this.txtDiferencia.Text = "";
            // 
            // txtImporteRetirado
            // 
            this.txtImporteRetirado.ReadOnly = true;
            this.txtImporteRetirado.Text = "";
            // 
            // txtCaja
            // 
            this.txtCajaInicioSiguiente.ReadOnly = true;
            this.txtCajaInicioSiguiente.Text = "";
            // 
            // btnCerrarCaja
            // 
            this.btnCerrarCaja.Text = "&Abrir Caja";
            // 
            // formAbrirCaja
            // 
            this.Name = "formAbrirCaja";
            this.Text = "Abrir Caja";
            this.Load += new System.EventHandler(this.formAbrirCaja_Load);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
