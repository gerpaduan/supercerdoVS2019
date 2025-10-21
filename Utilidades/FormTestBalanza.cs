using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;  // al inicio del archivo

namespace Utilidades
{
    public partial class FormTestBalanza : Form
    {
        string balanza = "";
        int protocolo = 1;//leer protocolo desde Config. -1 si es el original -2 modificado
        public FormTestBalanza()
        {
            InitializeComponent();
        }

        private void FormTestBalanza_Load(object sender, EventArgs e)
        {

            // Cargar puertos disponibles en el ComboBox
            string[] puertos = SerialPort.GetPortNames();
            comboPuertos.Items.Clear();
            comboPuertos.Items.AddRange(puertos);

            // Seleccionar el primero si hay al menos uno
            if (comboPuertos.Items.Count > 0)
            {
                comboPuertos.SelectedIndex = 0;
            }

            comboBalanzas.SelectedIndex = 0;
            balanza = comboBalanzas.Text;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                txtDetalleConfiBalanza.Text = "";
                try
                {
                    if (!BalanzaCom.IsOpen)
                    {
                        BalanzaCom.Open();
                    }

                    if (balanza == "Systel")
                    {
                        //dos byte para lectual actual
                        byte[] miBuffer = new byte[2];
                        miBuffer[0] = 07;
                        miBuffer[1] = 07;
                        BalanzaCom.Write(miBuffer, 0, miBuffer.Length);

                        txtDetalleConfiBalanza.Text = @"Systel \n****Protocolo 1 dos byte para lectual actual***
                                                             byte[] miBuffer = new byte[2];
                                                             miBuffer[0] = 07;
                                                             miBuffer[1] = 07;
                
                                                             ****Protocolo 2 un byte
                                                             byte[] miBuffer = new byte[1];
                                                             miBuffer[0] = 07;
                                                            ---
                                                            BalanzaCom.Write(miBuffer, 0, miBuffer.Length);";

                    }
                    else if (balanza == "Kretz")
                    {
                        //balanza Kretz
                        BalanzaCom.Write("p"); 
                        
                        txtDetalleConfiBalanza.Text = @"Kretz \n
                                                            ---
                                                            BalanzaCom.Write(p)";
                    }
                    else
                    {
                        //balanza Kretz
                        BalanzaCom.Write("p");
                    }

                    //BalanzaCom.Write(miBuffer, 0, miBuffer.Length);
                    txtPesoBalanza.Text = BalanzaCom.ReadExisting();

                    formatearPeso(txtPesoBalanza.Text);
                }
                catch (Exception ex)
                {
                    pesoBalanzaLabel.Text = "error\n" + ex.Message;
                    txtErrores.Text += "ERROR: " + pesoBalanzaLabel.Text;
                    if (BalanzaCom.IsOpen)
                    {
                        BalanzaCom.Close();
                    }
                    timer1.Stop();
                }
            }
            catch (Exception ex)
            {
                pesoBalanzaLabel.Text = "......";
                pesoBalanzaLabel.Text = "error\n" + ex.Message;
                if (BalanzaCom.IsOpen)
                {
                    BalanzaCom.Close();
                }
                //timer1.Stop();
            }
        }

        private void formatearPeso(string texto)
        {
            ///Systel
            if (balanza == "Systel")
            {
                ///Si texto tiene más de 10 caracteres se hace un return
                ///esto evita errores en el formateo del peso de Systel
                ///
                if (texto.Length > 10)
                    return;

                string peso = "";
                char[] nuevoPeso;
                int contar = 0; //cuenta los dígitos usables
                char[] indices = new char[texto.Length];
                bool esNegativo = false; //si es negativo el peso se estable true

                foreach (char letra in texto)
                {
                    if (letra == '-')
                    {
                        indices[contar] = letra;
                        contar++;
                        esNegativo = true;
                    }
                    if (contar == 3 && !esNegativo)
                    {
                        indices[contar] = '.';
                        contar++;
                    }
                    if (contar == 4 && esNegativo)
                    {
                        indices[contar] = '.';
                        contar++;
                    }
                    if (char.IsDigit(letra))
                    {
                        indices[contar] = letra;
                        contar++;
                    }
                }
                nuevoPeso = new char[contar];
                for (int i = 0; i < nuevoPeso.Length; i++)
                {
                    nuevoPeso[i] = indices[i];
                }
                peso = new string(nuevoPeso);

                //Systel tiene el error que en  peso inestable manda 5.000 kgs o 3.000 como estable
                string pesoBalanzaLabel_ = texto.Contains('i') && !texto.Contains("ei") ? peso + " i" : peso;

                if (!txtPesoFormat.Text.Contains("000") && !string.IsNullOrEmpty(txtPesoFormat.Text))
                {
                    txtErrores.Text += "\nPre-Format: " + texto;
                    txtErrores.Text += "\nFormat: " + txtPesoFormat.Text + "\n------\n";
                }

                /// Validación especial
                /// Systel manda 5 ó 3 según pruebas
                /// Entonces si peso no tiene entre 7 ó mas digitos (por si es negativo son mas de 7 digitos) 
                /// se setea valor vacío y se retorna a leer nuevamente el peso
                /// 
                if (pesoBalanzaLabel_.Length < 7)
                {
                    pesoBalanzaLabel.Text = pesoBalanzaLabel_ = "";
                    return;
                }

                pesoBalanzaLabel.Text = pesoBalanzaLabel_;

                if (!pesoBalanzaLabel.Text.Contains("000") && !string.IsNullOrEmpty(pesoBalanzaLabel.Text))
                {
                    txtErrores.Text += "\n******\npesoBalanzaLabel.Text: " + pesoBalanzaLabel.Text + "\n******\n";
                }
            }
            else if (balanza == "Kretz")
            {
                int cantChars = texto.Length;
                pesoBalanzaLabel.Text = cantChars > 0 ? texto.Substring(1, cantChars - 2) : texto;
            }
            else if (true)
            {
                int cantChars = texto.Length;
                pesoBalanzaLabel.Text = cantChars > 0 ? texto.Substring(1, cantChars - 2) : texto;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (BalanzaCom.IsOpen)
            {
                BalanzaCom.Close();
            }
            BalanzaCom.StopBits = comboBalanzas.Text.Equals("Kretz") ? System.IO.Ports.StopBits.Two : System.IO.Ports.StopBits.One;
            BalanzaCom.PortName = comboPuertos.Text;
            timer1.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            if (BalanzaCom.IsOpen)
            {
                BalanzaCom.Close();
            }
        }

        private void comboBalanzas_SelectedIndexChanged(object sender, EventArgs e)
        {
            balanza = comboBalanzas.Text;
        }

        private void FormTestBalanza_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BalanzaCom.IsOpen)
            {
                BalanzaCom.Close();
            }
        }
    }
}
