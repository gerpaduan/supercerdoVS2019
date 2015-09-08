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
    public class SingletonLeerPeso
    {
        private static SingletonLeerPeso _singleton;
        SerialPort BasculaCom;
        string lineas = "Se capturó \r\n";
        private static System.Timers.Timer timer1 = new System.Timers.Timer(300);
        Label Recibidos = new Label();
        Util_Form util_form = new Util_Form();
        private SingletonLeerPeso()
        {
            BasculaCom = new SerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, StopBits.One);
            timer1.Enabled = true;
            timer1.Elapsed += OnTimedEvent;
        }

        public static SingletonLeerPeso CrearLeerPeso()
        {            
            if (_singleton == null)
            {
                _singleton = new SingletonLeerPeso();
            }
            return _singleton;
        }
        string pesoBalanza;
        public string PesoBalanza
        {
            get { return pesoBalanza; }
            set { pesoBalanza = value; }
        }

        internal delegate void MostrarRecepcion(string Texto);

        public void CerrarPuerto()
        {
            if (BasculaCom.IsOpen)
            {
                BasculaCom.Close();
            }
        }
        public void AbrirPuerto()
        {
            if (!BasculaCom.IsOpen)
            {
                BasculaCom.Open();
            }
        }

        public string ObtenerPeso()
        {
            return pesoBalanza;

        }
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                AbrirPuerto();
                byte[] miBuffer = new byte[1];
                miBuffer[0] = 5;
                BasculaCom.Write(miBuffer, 0, miBuffer.Length);
                string readExisting = BasculaCom.ReadExisting();
                this.MostrarRecibidos(readExisting);
            }
            catch (Exception ex)
            {
                Utilidades.Util_Form.errorBalanza(ex.Message);
                //throw Exception ;
                timer1.Enabled = false;
                CerrarPuerto();
                //BasculaCom = new SerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, StopBits.One);
                pesoBalanza = null;
                _singleton = null;
            }
        }
        private void MostrarRecibidos(string texto)
        {
                //Mostrar los bytes recibidos en el Label recibidos
                //BasculaCom.InvokeRequired
                if (Recibidos.InvokeRequired)
                {
                    lineas = lineas + "\r\n" + ("MostrarRecibidos InvokeRequired");

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
                }
        }

        private void Invoke(MostrarRecepcion delegado, object[] p)
        {
            throw new NotImplementedException();
        }
    }
}
