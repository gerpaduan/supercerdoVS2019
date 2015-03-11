using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace Utilidades
{
    public class Leer_Peso
    {
        //BasculaCom.PortName = "COM1";
        //BasculaCom.BaudRate = 9600;
        //BasculaCom.Parity = System.IO.Ports.Parity.None;
        //BasculaCom.StopBits = StopBits.One;
        SerialPort BasculaCom = new SerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, StopBits.One);
        Label Recibidos = new Label();
        string pesoBalanza = "";
        internal delegate void MostrarRecepcion(string Texto);
        
        public void AbrirPuerto()
        {
            //try
            //{
            if (!BasculaCom.IsOpen)
            {
                BasculaCom.Open();
            }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Exception en AbrirPuerto() \n" + ex.Message);
            //}
        }

        public void CerrarPuerto()
        {            
            //try
            //{
            if (BasculaCom.IsOpen)
            {
                BasculaCom.Close();
            }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Exception en CerrarPuerto() \n" + ex.Message);
            //}
        }

        public string ObtenerPeso()
        {
            //try
            //{
            AbrirPuerto();

            byte[] miBuffer = new byte[1];
            miBuffer[0] = 5;
            BasculaCom.Write(miBuffer, 0, miBuffer.Length);
            MostrarRecibidos(BasculaCom.ReadExisting());
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Exception en ObtenerPeso \n" + ex.Message);
            //}
            return pesoBalanza;
        }

        internal void Recibir()
        {
            //al recibir de la bascula los bytesToRead indicara
            //un valor superior a 0
            //MessageBox.Show("  Recibir()");
            try
            {
                MostrarRecibidos(BasculaCom.ReadExisting());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Recibir  \n" + ex.Message);
            }
        }

        private void MostrarRecibidos(string texto)
        {
            try
            {
                //Mostrar los bytes recibidos en el Label recibidos
                //BasculaCom.InvokeRequired
                if (Recibidos.InvokeRequired)
                {
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
                    }
                    pesoBalanza = peso;
                    Recibidos.Text = peso;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception en MostrarRecibidos() \n" + ex.Message);
            }
        }

        private void Invoke(MostrarRecepcion delegado, object[] p)
        {
            throw new NotImplementedException();
        }

        private void BasculaCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Recibir();
        }
    }
}
