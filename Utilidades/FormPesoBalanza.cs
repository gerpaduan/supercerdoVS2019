using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Utilidades
{
    public partial class FormPesoBalanza : Form
    {
        public FormPesoBalanza()
        {
            InitializeComponent();
            BalanzaCom.PortName = ConfigurationManager.AppSettings["puerto"].ToString();
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
                //dos byte para lectual actual
                byte[] miBuffer = new byte[2];
                miBuffer[0] = 07;
                miBuffer[1] = 07;
                BalanzaCom.Write(miBuffer, 0, miBuffer.Length);
                txtPesoBalanza.Text = BalanzaCom.ReadExisting();
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
            
            pesoBalanzaLabel.Text = texto.Contains('i') ? peso + " i" : peso;
        }
    }
}
