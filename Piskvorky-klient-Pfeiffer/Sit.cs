using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Piskvorky_klient_Pfeiffer
{
    class Sit
    {
        string ipAdresaPoslechu;
        string ipAdresaServeru;
        int cisloPortu;
        static string zprava = "Vychozi";

        public Sit(string ipPoslechu, int port, string ipServeru)
        {
            ipAdresaPoslechu = ipPoslechu;
            ipAdresaServeru = ipServeru;
            cisloPortu = port;
        }

        public void Poslouchej()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(ipAdresaPoslechu), cisloPortu);
            TcpClient client = null;
            try
            {
                while (true)
                {

                    listener.Start();
                    client = listener.AcceptTcpClient();
                    NetworkStream clientStream = client.GetStream();
                    StreamReader reader = new StreamReader(clientStream);
                    zprava = reader.ReadToEnd();
                    //MessageBox.Show("Klient prijal: " + obsahZpravy);

                    //zprava = obsahZpravy;

                    //MainWindow.prijateZpravy(obsahZpravy);

                    //ROZKODOVAT SI TYP ZPRAVY
                    //Prichozi: Hrajes , Kolecko, Krizek
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nastala chyba v poslechu \n\n" + ex.Message);
            }
            finally
            {
                if (client != null)
                {
                    client.Close();
                }
                listener.Stop();
            }
        }

        public void Odesli(string data)
        {
            TcpClient client = null;
            try
            {
                client = new TcpClient(ipAdresaServeru, 5000);      //adresa serveru
                Stream strm = client.GetStream();
                StreamWriter writer = new StreamWriter(strm);
                writer.Write(data);
                writer.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nastala chyba v odesilani \n\n" + ex.Message);

            }
            finally
            {
                if (client != null)
                    client.Close();

            }
        }

        internal string prijataZprava()
        {
            return zprava;
        }
    }
}
