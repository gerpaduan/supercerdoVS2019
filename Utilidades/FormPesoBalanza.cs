using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Threading;

namespace Utilidades
{
    public partial class FormPesoBalanza : Form
    {
        string balanza = ConfigurationManager.AppSettings["balanza"].ToString();
        public FormPesoBalanza()
        {
            InitializeComponent();
            BalanzaCom.PortName = ConfigurationManager.AppSettings["puerto"].ToString();
            BalanzaCom.StopBits = balanza.Equals("Kretz") ? System.IO.Ports.StopBits.Two : System.IO.Ports.StopBits.One;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
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
                }
                else
                {
                    //balanza Kretz
                    BalanzaCom.Write("p");
                }

                //BalanzaCom.Write(miBuffer, 0, miBuffer.Length);
                txtPesoBalanza.Text = BalanzaCom.ReadExisting();

                //si peso contiene 000 and e se hace una pausa porque Systel Genera error con peso redonde. Ej: 5.000 kgs
                if (txtPesoBalanza.Text.Contains("000e"))
                {
                    //Thread.Sleep(500); // Pausa por 3000 milisegundos (3 segundos)
                    txtPesoBalanza.Text = BalanzaCom.ReadExisting();
                }

                formatearPeso(txtPesoBalanza.Text);
            }
            catch (Exception ex)
            {
                pesoBalanzaLabel.Text = "error\n" + ex.Message;
                if (BalanzaCom.IsOpen)
                {
                    BalanzaCom.Close();
                }
                timer1.Stop();
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

                pesoBalanzaLabel.Text = texto.Contains('i') && !texto.Contains("ei") ? peso + " i" : peso;
            }
            if (balanza == "Kretz")
            {
                int cantChars = texto.Length;
                pesoBalanzaLabel.Text = cantChars > 0 ? texto.Substring(1, cantChars-2) : texto;
            }
        }
    }
}
