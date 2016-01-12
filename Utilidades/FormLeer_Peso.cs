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
    public partial class FormLeer_Peso : Form
    {
        private static FormLeer_Peso _singleton;
        SerialPort BasculaCom;
        string lineas = "Se capturó \r\n";
        int contador = 0;
        private FormLeer_Peso()
        {
            InitializeComponent();
            BasculaCom = new SerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, StopBits.One);
            txtVelocidadTimer.Text = timer1.Interval.ToString();
            timer1.Start();
        }

        public static FormLeer_Peso CrearLeerPeso()
        {            
            bool isDisposed = (_singleton != null && _singleton.IsDisposed) ? true : false;
            if (isDisposed || _singleton == null)
            {
                _singleton = new FormLeer_Peso();
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
            //if (!BasculaCom.IsOpen)
            //{
            //    BasculaCom.Open();
            //}
            
            ////BasculaCom.Open();
            //EnviarPeticion();
            return pesoBalanza;

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //AbrirPuerto();

            //byte[] miBuffer = new byte[1];
            //miBuffer[0] = 5;
            //BasculaCom.Write(miBuffer, 0, miBuffer.Length);
            //MostrarRecibidos(BasculaCom.ReadExisting());
            string balanza = "";
            try
            {
                
                
                contador++;
                lineas = lineas + "\r\nInicio:  " + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond;
                //lineas = lineas +"\r\n"+ ("1*"+_singleton.BasculaCom.);
                AbrirPuerto();
                lineas = lineas +"\r\n"+ ("2*" + BasculaCom.IsOpen.ToString());
                byte[] miBuffer = new byte[1];
                miBuffer[0] = 5;
                BasculaCom.Write(miBuffer, 0, miBuffer.Length);
                string readExisting = BasculaCom.ReadExisting();
                lineas = lineas + "\r\n datos balanza" + " " + BasculaCom.ToString() + " " + BasculaCom.PortName + " / readExis " + BasculaCom.ReadExisting();
                //MostrarRecibidos(BasculaCom.ReadExisting(c));
                //formatearPeso(readExisting);
                this.MostrarRecibidos(readExisting);
                //CerrarPuerto();
                balanza = "BasculaCom.BaseStream  " + BasculaCom.BaseStream.ToString() + "  ||  " +
                    "BasculaCom.BaudRate  " + BasculaCom.BaudRate.ToString() + "  ||  " +
                    "BasculaCom.BreakState  " + BasculaCom.BreakState.ToString() + "  ||  " +
                    "BasculaCom.BytesToRead  " + BasculaCom.BytesToRead.ToString() + "  ||  " +
                    "BasculaCom.DataBits  " + BasculaCom.DataBits.ToString() + "  ||  " +
                    "BasculaCom.ReadBufferSize  " + BasculaCom.ReadBufferSize.ToString() + "  ||  " +
                    "BasculaCom.PortName  " + BasculaCom.PortName.ToString() + "  ||  ";
                textBox1.Text = balanza;

                lineas = lineas + "\r\n" + ("3*" + BasculaCom.IsOpen.ToString());
                lineas = lineas + "\r\nFIN:  " + DateTime.Now.Minute + ":" + DateTime.Now.Second + ":" + DateTime.Now.Millisecond + "\r\n";
                //CerrarPuerto();
                //if (contador < 4)
                //{
                //    timer1.Stop();
                //    MessageBox.Show(lineas);
                //    timer1.Start();
                //    contador = 0;                    
                //}
                
                //pesoBalanza = DateTime.Now.Millisecond.ToString();
                //txtPesoBalanza.Text = pesoBalanza;
            }
            catch (ObjectDisposedException ex)
            {
                balanza = "BasculaCom.BaseStream  " + BasculaCom.BaseStream.ToString() + "  ||  " +
                    "BasculaCom.BaudRate  " + BasculaCom.BaudRate.ToString() + "  ||  " +
                    "BasculaCom.BreakState  " + BasculaCom.BreakState.ToString() + "  ||  " +
                    "BasculaCom.BytesToRead  " + BasculaCom.BytesToRead.ToString() + "  ||  " +
                    "BasculaCom.DataBits  " + BasculaCom.DataBits.ToString() + "  ||  " +
                    "BasculaCom.ReadBufferSize  " + BasculaCom.ReadBufferSize.ToString() + "  ||  " +
                    "BasculaCom.PortName  " + BasculaCom.PortName.ToString() + "  ||  ";
                textBox1.Text = balanza;
                timer1.Stop();
                CerrarPuerto();
                //BasculaCom = new SerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, StopBits.One);
                pesoBalanza = null;// ex.Message;
                _singleton = null;
                MessageBox.Show(ex.Message);
            } 
            catch (Exception ex)
            {
                //throw Exception ;
                timer1.Stop();
                CerrarPuerto();
                //BasculaCom = new SerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, StopBits.One);
                pesoBalanza = null;// ex.Message;
                _singleton = null;
                MessageBox.Show("No se pudo leer el peso de la balanza.\r\n" + ex.Message + "\r\n\r\n Más detalles:\r\n" + ex.HelpLink + 
                    " / " + ex.InnerException + " / " + ex.Source + " / " + ex.StackTrace +
                    " / " + ex.TargetSite + " / " + ex.Data, "Balanza", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void formatearPeso(string texto) 
        {
            lineas = lineas + "\r\n formatearPeso(texto): " + texto;
            //string peso = "";
            char[] nuevoPeso;

            if (texto.Contains("E"))
            {
                pesoBalanza = "Error - " + texto;
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

                pesoBalanza = new string(nuevoPeso);
                txtPesoBalanza.Text = pesoBalanza;
            }

        }
        private void MostrarRecibidos(string texto)
        {
            try
            {
                lineas = lineas + "\r\n MostrarRecibido(texto): " + texto;
                //Mostrar los bytes recibidos en el Label recibidos
                //BasculaCom.InvokeRequired
                if (Recibidos.InvokeRequired)
                {
                    lineas = lineas +"\r\n"+ ("MostrarRecibidos InvokeRequired");

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

                        lineas = lineas +"\r\n"+ ("MostrarRecibidos InvokeRequired else. Peso=" + peso);
                    }

                    Recibidos.Text = peso;
                    txtPesoBalanza.Text = peso;
                    pesoBalanza = Recibidos.Text;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Método MostrarRecibidos  " + ex.Message);
            }
        }

        private void Invoke(MostrarRecepcion delegado, object[] p)
        {
            throw new NotImplementedException();
        }

        private void BasculaCom_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            lineas = lineas + "\r\n" + ("BasculaCom_DataReceived(,SerialDataReceivedEventArgs e)): "+ e.ToString());
            Recibir();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                CerrarPuerto();
                txtVelocidadTimer.ReadOnly = false;
                btnTimer.Text = "Start";
                pesoBalanza = "";
            }
            else
            {
                timer1.Interval = Convert.ToInt32(txtVelocidadTimer.Text);
                timer1.Start();
                txtVelocidadTimer.ReadOnly = true;
                btnTimer.Text = "Stop";
            }
        }

        private void FormLeer_Peso_FormClosed(object sender, FormClosedEventArgs e)
        {
            pesoBalanza = "0";
            //_singleton = null;
            //_singleton.IsDisposed;
        }

        private void txtNuevo_Click(object sender, EventArgs e)
        {
            
        }      
       
    }
}
