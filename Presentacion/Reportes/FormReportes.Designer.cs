namespace Presentacion.Reportes
{
    partial class FormReportes
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.crystalReportes = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.SuspendLayout();
            // 
            // crystalReportes
            // 
            this.crystalReportes.ActiveViewIndex = -1;
            this.crystalReportes.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.crystalReportes.Location = new System.Drawing.Point(0, 0);
            this.crystalReportes.Name = "crystalReportes";
            this.crystalReportes.SelectionFormula = "";
            this.crystalReportes.Size = new System.Drawing.Size(766, 524);
            this.crystalReportes.TabIndex = 0;
            this.crystalReportes.ViewTimeSelectionFormula = "";
            this.crystalReportes.Load += new System.EventHandler(this.crystalReportViewer1_Load);
            // 
            // FormReportes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(766, 524);
            this.Controls.Add(this.crystalReportes);
            this.Name = "FormReportes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Reportes";
            this.ResumeLayout(false);

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer crystalReportes;
    }
}