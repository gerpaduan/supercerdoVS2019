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
        Label Recibidos = new Label();
        string pesoBalanza = "";
        SerialPort BasculaCom = new SerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, StopBits.One);



        internal delegate void MostrarRecepcion(string Texto);


        public void AbrirPuerto()
        {
            try
            {
                //BasculaCom.PortName = "COM1";//+ txtNroPuerto.Text.Trim();

                //BasculaCom.BaudRate = 9600;

                //BasculaCom.Parity = System.IO.Ports.Parity.None;

                //BasculaCom.StopBits = StopBits.One;

                if (!BasculaCom.IsOpen)
                {
                    //ComunicacionBalanza();
                    BasculaCom.Open();
                    MessageBox.Show("abre puerto en AbrirPuerto()   ");
                }
                MessageBox.Show("ultima linea try en AbrirPuerto()   ");
            }
            catch (Exception ex)
            {

                MessageBox.Show("Exception en AbrirPuerto()   " + ex.Message);
            }
        }

        public void CerrarPuerto()
        {
            if (BasculaCom.IsOpen)
            {
                BasculaCom.Close();
            }
        }

        public string ObtenerPeso()
        {
            try
            {
                AbrirPuerto();
                MessageBox.Show("Ingreso al Try");
                //enviar una p
                byte[] miBuffer = new byte[1];
                miBuffer[0] = 5;
                BasculaCom.Write(miBuffer, 0, miBuffer.Length);
                MessageBox.Show("ReadExisting()  " + BasculaCom.ReadExisting());
                MostrarRecibidos(BasculaCom.ReadExisting());
            }
            catch (Exception ex)
            {

                MessageBox.Show("ObtenerPeso  EXception  ->  " + ex.Message);

            }
            return pesoBalanza;
        }

        internal void Recibir()
        {

            //al recibir de la bascula los bytesToRead indicara

            //un valor superior a 0


            MessageBox.Show("  Recibir()");
            try
            {
                //byte[] byteRecibidos= StrToByteArray(BasculaCom.ReadExisting());
                //lblCantBytes.Text = byteRecibidos.Length.ToString();
                MostrarRecibidos(BasculaCom.ReadExisting());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Recibir  " + ex.Message);
            }
        }



        private void MostrarRecibidos(string texto)
        {
            MessageBox.Show("  MostrarRecibidos - Try - Con parametro string = " + texto + "  <- param");
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
                    //byte[] p = new byte[] { 49, 50, 51, 52, 53, 54 };  // Simulamos un byte[] con la codificacion ASCII de la cadena 123456
                    //System.Text.Encoding encoding = System.Text.Encoding.ASCII;
                    //this.Recibidos.Text = texto;
                    //string texto = encoding.GetString(p); // Tenemos la cade 123456

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
                MessageBox.Show("MostrarRecibidos  " + ex.Message);
            }


        }

        private void Invoke(MostrarRecepcion delegado, object[] p)
        {
            throw new NotImplementedException();
        }

        private void BasculaCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            MessageBox.Show("BasculaCom_DataReceived  ");
            Recibir();
        }


    }
}
