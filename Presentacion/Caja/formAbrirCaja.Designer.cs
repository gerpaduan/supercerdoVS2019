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
            this.pnlBuscar.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtCajaActual
            // 
            this.txtCajaActual.ReadOnly = true;
            this.txtCajaActual.Text = "";
            // 
            // txtGastos
            // 
            this.txtGastos.ReadOnly = true;
            this.txtGastos.Text = "";
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
            this.txtCaja.ReadOnly = true;
            this.txtCaja.Text = "";
            // 
            // btnCerrarCaja
            // 
            this.btnCerrarCaja.Text = "Abrir Caja";
            this.btnCerrarCaja.Click += new System.EventHandler(this.btnCerrarCaja_Click);
            // 
            // formAbrirCaja
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(361, 479);
            this.Name = "formAbrirCaja";
            this.Text = "Abrir Caja";
            this.Load += new System.EventHandler(this.formAbrirCaja_Load);
            this.pnlBuscar.ResumeLayout(false);
            this.pnlBuscar.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
