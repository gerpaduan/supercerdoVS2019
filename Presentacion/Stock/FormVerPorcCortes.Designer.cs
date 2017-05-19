namespace Presentacion.Stock
{
    partial class FormVerPorcCortes
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.grillaPorcCortes = new System.Windows.Forms.DataGridView();
            this.grillaPromMedias = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.grillaPorcCortes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grillaPromMedias)).BeginInit();
            this.SuspendLayout();
            // 
            // grillaPorcCortes
            // 
            this.grillaPorcCortes.AllowUserToAddRows = false;
            this.grillaPorcCortes.AllowUserToOrderColumns = true;
            this.grillaPorcCortes.AllowUserToResizeRows = false;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.grillaPorcCortes.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
            this.grillaPorcCortes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grillaPorcCortes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.grillaPorcCortes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.NullValue = null;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaPorcCortes.DefaultCellStyle = dataGridViewCellStyle6;
            this.grillaPorcCortes.Location = new System.Drawing.Point(2, 85);
            this.grillaPorcCortes.Name = "grillaPorcCortes";
            this.grillaPorcCortes.ReadOnly = true;
            this.grillaPorcCortes.RowHeadersVisible = false;
            this.grillaPorcCortes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaPorcCortes.Size = new System.Drawing.Size(673, 261);
            this.grillaPorcCortes.TabIndex = 18;
            // 
            // grillaPromMedias
            // 
            this.grillaPromMedias.AllowUserToAddRows = false;
            this.grillaPromMedias.AllowUserToOrderColumns = true;
            this.grillaPromMedias.AllowUserToResizeRows = false;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.grillaPromMedias.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle7;
            this.grillaPromMedias.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.grillaPromMedias.BackgroundColor = System.Drawing.SystemColors.Control;
            this.grillaPromMedias.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.NullValue = null;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.grillaPromMedias.DefaultCellStyle = dataGridViewCellStyle8;
            this.grillaPromMedias.Location = new System.Drawing.Point(2, 1);
            this.grillaPromMedias.Name = "grillaPromMedias";
            this.grillaPromMedias.ReadOnly = true;
            this.grillaPromMedias.RowHeadersVisible = false;
            this.grillaPromMedias.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grillaPromMedias.Size = new System.Drawing.Size(395, 78);
            this.grillaPromMedias.TabIndex = 19;
            // 
            // FormVerPorcCortes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(674, 345);
            this.Controls.Add(this.grillaPromMedias);
            this.Controls.Add(this.grillaPorcCortes);
            this.Name = "FormVerPorcCortes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ver Porcentaje Cortes";
            this.Load += new System.EventHandler(this.FormVerPorcCortes_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grillaPorcCortes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grillaPromMedias)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView grillaPorcCortes;
        private System.Windows.Forms.DataGridView grillaPromMedias;
    }
}