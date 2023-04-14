[1mdiff --git a/Presentacion/Caja/formEgresosCaja.cs b/Presentacion/Caja/formEgresosCaja.cs[m
[1mindex 780c61e..d28829c 100644[m
[1m--- a/Presentacion/Caja/formEgresosCaja.cs[m
[1m+++ b/Presentacion/Caja/formEgresosCaja.cs[m
[36m@@ -14,6 +14,7 @@[m [mnamespace Presentacion.Caja[m
     {[m
         protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();[m
         protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();[m
[32m+[m[32m        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();[m
 [m
         protected Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();[m
         protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();[m
[36m@@ -31,12 +32,22 @@[m [mnamespace Presentacion.Caja[m
             this.Text += Utilidades.Conexion.getSucursalConexion();[m
             DateTime today = DateTime.Today;[m
             fechaHasta.Value = today.AddDays(1).AddSeconds(-1);[m
[31m-            fechaDesde.Value = today.AddDays(-8); [m
[32m+[m[32m            fechaDesde.Value = today.AddDays(-8);[m
[32m+[m[32m            cargarComboUsuario();[m
             cargarSucursal();[m
             cargarTiposEgresoCaja();[m
             cargarGrilla();[m
         }[m
 [m
[32m+[m
[32m+[m[32m        private void cargarComboUsuario()[m
[32m+[m[32m        {[m
[32m+[m[32m            comboUsuario.DataSource = oUsuarioN.obtenerUsuariosConTodos();[m
[32m+[m[32m            comboUsuario.DisplayMember = "nombre";[m
[32m+[m[32m            comboUsuario.ValueMember = "id";[m
[32m+[m[32m            comboUsuario.SelectedIndex = 0;[m
[32m+[m[32m        }[m
[32m+[m
         private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)[m
         {[m
             if (!comboSucursal.ValueMember.Equals(""))[m
[36m@@ -59,11 +70,14 @@[m [mnamespace Presentacion.Caja[m
             grillaEgresosCaja.Columns["Creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";[m
             grillaEgresosCaja.Columns["Actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";[m
 [m
[32m+[m[32m            int cantItems = 0;[m
             decimal total = 0;[m
             foreach (DataGridViewRow row in grillaEgresosCaja.Rows)[m
             {[m
[32m+[m[32m                cantItems++;[m
                 total = total + Convert.ToDecimal(row.Cells["monto"].Value.ToString());[m
             }[m
[32m+[m[32m            txtItems.Text = cantItems.ToString();[m
             txtTotalS.Text = total.ToString("F2");[m
         }[m
 [m
