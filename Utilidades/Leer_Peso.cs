using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilidades
{
    public partial class Leer_Peso : Form
    {
        string pesoBalanza;

        public string PesoBalanza
        {
            get { return pesoBalanza; }
            set { pesoBalanza = value; }
        }

        internal delegate void MostrarRecepcion(string Texto);

        public Leer_Peso()
        {
            InitializeComponent();
        }

        public void CerrarPuerto()
        {
            if (BasculaCom.IsOpen)
            {
                BasculaCom.Close();
            }
        }
        public void AbrirPuerto()
        {
            //try
            //{
            //    if (!BasculaCom.IsOpen)
            //    {
            //        BasculaCom.Open();
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
            
        }

        public string ObtenerPeso()
        {
            if (!BasculaCom.IsOpen)
            {
                BasculaCom.Open();
            }
            
            //BasculaCom.Open();
            EnviarPeticion();
            return pesoBalanza;

        }

        private void Leer_Peso_Load(object sender, EventArgs e)
        {
            //BasculaCom.Open();
           // AbrirPuerto();        
        }

        internal void EnviarPeticion()
        {
            //try
            //{
                /// Se envía número 5 para la peticion
                byte[] miBuffer = new byte[1];
                miBuffer[0] = 5;
                this.BasculaCom.Write(miBuffer, 0, miBuffer.Length);

              //  MessageBox.Show("EnviarPeticion");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("EnviarPeticion  " + ex.Message);
            //}

        }

        internal void Recibir()
        {
            try
            {
                MostrarRecibidos(BasculaCom.ReadExisting());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Método Recibir  " + ex.Message);
            }
        }
        private void MostrarRecibidos(string texto)
        {
            try
            {
                //Mostrar los bytes recibidos en el Label recibidos

                if (Recibidos.InvokeRequired)
                {
                    //MessageBox.Show("MostrarRecibidos InvokeRequired");
                    MostrarRecepcion delegado = new MostrarRecepcion(MostrarRecibidos);

                    this.Invoke(delegado, new object[] { texto });

                }
                else
                {
                    string peso = "";
                    char[] nuevoPeso;

                    if (texto.Contains("E"))
                    {
                        peso = "Error - " + texto;
                    }

                    else
                    {
                        

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

                       // MessageBox.Show("MostrarRecibidos InvokeRequired else. Peso="+peso);
                    }

                    Recibidos.Text = peso;
                    pesoBalanza = Recibidos.Text;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Método MostrarRecibidos  " + ex.Message);
            }


        }

        private void BasculaCom_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
           // MessageBox.Show("BasculaCom_DataReceived");
            Recibir();
        }
       
    }
}
