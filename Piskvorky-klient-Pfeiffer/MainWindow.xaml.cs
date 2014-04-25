using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace Piskvorky_klient_Pfeiffer
{
    public partial class MainWindow : Window
    {
        int[,] polePravidla = new int[11, 6];
        public Databaze db;

        private Grid mrizka;
        private TextBox[,] pole;

        string[] pravidlo = new string[10];
        int cisloPravidla = 0;

        int port;
        string ipServer;
        string ipPoslechu;
        int idHrace;

        string zprava;
        string staraZprava = "Vychozi";

        DispatcherTimer casovac;

        int pocetVyher, pocetProher, pocetRemiz = 0;

        bool kolecko = false;
        bool krizek = false;

        string[] hraciPlocha = new string[10];
        string[] pomHraciPlocha = new string[10];
        string celaHraciPlocha, pomCelaHraciPlocha;

        public MainWindow()
        {
            InitializeComponent();
            vykresleniMrizky();
            hlavniProgram();
            hlavniOkno.Width = 220;
            hlavniOkno.Height = 370;
        }

        private void vykresleniMrizky()
        {
            mrizka = new Grid();
            mrizka.Background = Brushes.WhiteSmoke;

            mrizka.Height = 150;
            mrizka.Width = 150;
            mrizka.HorizontalAlignment = HorizontalAlignment.Left;
            mrizka.VerticalAlignment = VerticalAlignment.Top;

            for (int i = 0; i < 5; i++)//nejde parallel
            {
                mrizka.ColumnDefinitions.Add(new ColumnDefinition());
                mrizka.RowDefinitions.Add(new RowDefinition());
            }

            platno.Children.Add(mrizka);
            vykresleniTextboxu();
        }

        private void vykresleniTextboxu()
        {
            pole = new TextBox[5, 5];

            for (int i = 0; i < 5; i++)//nejde parallel, chyba STA
            {
                for (int j = 0; j < 5; j++)
                {
                    pole[i, j] = new TextBox();

                    pole[i, j].TextAlignment = TextAlignment.Center;

                    Grid.SetRow(pole[i, j], j);
                    Grid.SetColumn(pole[i, j], i);
                    mrizka.Children.Add(pole[i, j]);
                }
            }
        }

        private void vytvoreniPravidla() //vytvoření pravidla z textboxu
        {
            try
            {
                pravidlo[cisloPravidla] = "";
            }
            catch (IndexOutOfRangeException)
            {
                //lInfo.Content = "Maximalní počet pravidel je " + cisloPravidla + 1;
                lvUdalosti.Items.Add("Všechny pravidla jsou vytvořeny!");
                return;
            }
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    pravidlo[cisloPravidla] += pole[j, i].Text;
                }
            }

            pravidlo[cisloPravidla] += ":" + tbRadek.Text + ":" + tbSloupec.Text;

            //lInfo.Content = "Bylo vytvořeno pravidlo č. " + (cisloPravidla + 1);
            lvUdalosti.Items.Add("Vytvoření pravidla " + (cisloPravidla + 1));
            cisloPravidla++;
        }

        private void ulozeniPravidelTxt()
        {
            using (StreamWriter sw = new StreamWriter(@"pravidla.txt"))
            {
                for (int i = 0; i < cisloPravidla; i++)
                {
                    sw.WriteLine(pravidlo[i]);
                }
                lvUdalosti.Items.Add(string.Format("Bylo uloženo {0} pravidel", cisloPravidla));
            }
        }

        private void nacteniPravidelTxt()
        {
            using (StreamReader sr = new StreamReader(@"pravidla.txt"))
            {
                for (int i = 0; i < int.Parse(tbPocetPravidel.Text); i++)
                {
                    if ((pravidlo[i] = sr.ReadLine()) != null)
                    {
                        cisloPravidla = i + 1;
                    } 
                    else
                    {
                        break;
                    }
                }
                lvUdalosti.Items.Add(string.Format("Bylo načteno {0} pravidel", cisloPravidla));
            }
        }

        private void pripojeniDatabaze()
        {
            Databaze db = new Databaze(tbDatabaze.Text, tbUser.Text, tbHeslo.Text);      //TODO: zatim nefunkcni ve tride DATABAZE!!!funguje jako localhost

            db.Pripojit();
            db.Odpojit();

            //lInfo.Content = "Připojeno k databázi";
            //tbDatabaze.IsEnabled = false;
            //btnPripojit.Content = "Odpojit";
            //lInfo.Content = "Test připojený proběhl v pořádku";
            lvUdalosti.Items.Add("Test připojení");
        }

        /*private void odpojeniDatabaze()
        {
            Databaze db = new Databaze(tbDatabaze.Text);

            db.Odpojit();

            lInfo.Content = "Odpojeno od databáze";
            tbDatabaze.IsEnabled = true;
            //btnPripojit.Content = "Připojit";
        }*/

        private void pridaniHraceDoDatabaze()
        {
            //string jmeno = "Pfeiffer";

            Databaze db = new Databaze(tbDatabaze.Text, tbUser.Text, tbHeslo.Text);  

            db.Pripojit();
            db.PridatHrace(tbJmenoKlienta.Text, int.Parse(tbPort.Text));
            db.Odpojit();

            idHrace = db.ziskatIdHrace();

            //lInfo.Content = "Jméno hráče bylo uloženo do databáze, ID = " + idHrace;

            btnPridaniPravidla.IsEnabled = true;

            lvUdalosti.Items.Add("Přidáno jméno do db");
            lvUdalosti.Items.Add("ID: " + idHrace);

            poslaniZpravyServeru("Vytvoren");
        }
        
        private void pridaníPravidlaDoDatabaze()
        {
            //string pravidlo = "";

            Databaze db = new Databaze(tbDatabaze.Text, tbUser.Text, tbHeslo.Text);  

            for (int i = 0; i < cisloPravidla; i++)
            {
                db.Pripojit();
                db.PridatPravidlo(pravidlo[i]);
                db.Odpojit();
            }
            //db.PridatPravidlo(pravidlo);
            lvUdalosti.Items.Add("Přidány pravidla do db");
            poslaniZpravyServeru("Pripraven");
        }

        private void poslechSite()
        {
            Sit s = new Sit(ipPoslechu, port, ipServer);

            Task vlaknoPoslouchajiciSit = Task.Factory.StartNew(() => s.Poslouchej());

            casovac.Start();

            //Task vlaknoReakceZpravy = Task.Factory.StartNew(() => reakceNaZpravy(zprava));

            //lInfo.Content = "Klient poslouhá komunikaci se serverem.";
            lvUdalosti.Items.Add("Začátek poslouchání");
        }

        private void poslaniZpravyServeru(string zprava)
        {
            Sit s = new Sit(ipPoslechu, port, ipServer);

            s.Odesli(idHrace + ":" + zprava);

            //lInfo.Content = "Odeslana zprava serveru: " + zprava;
            lvUdalosti.Items.Add("Odeslano serveru: " + zprava);
        }

        private void prohra()
        {
            poslaniZpravyServeru("Prohra");
            pocetProher++;
            lvUdalosti.Items.Add("Prohra");
            lvUdalosti.Items.Add(string.Format("Výhry: {0}, prohry: {1}, remizy: {2}", pocetVyher, pocetProher, pocetRemiz));
            lProhra.Content = pocetProher;
        }

        private void vyhra()
        {
            pocetVyher++;
            lvUdalosti.Items.Add("Vyhra");
            lvUdalosti.Items.Add(string.Format("Výhry: {0}, prohry: {1}, remizy: {2}", pocetVyher, pocetProher, pocetRemiz));
            lVyhra.Content = pocetVyher;
        }

        private void remiza()
        {
            pocetRemiz++;
            lvUdalosti.Items.Add("Remiza");
            lvUdalosti.Items.Add(string.Format("Výhry: {0}, prohry: {1}, remizy: {2}", pocetVyher, pocetProher, pocetRemiz));
            lVyhra.Content = pocetRemiz;
        }

        private void reakceNaZpravy(string novaZprava)
        {
            if (novaZprava != /*"Hrajes")// */staraZprava)// || novaZprava != "Vychozi")
            {
                lvUdalosti.Items.Add("Zpráva: " + novaZprava);
                switch (novaZprava)
                {
                    case "Hrajes":
                        nacteniHraciPlochy();
                        if (kontrolaProhry() == true)
                        {
                            prohra();
                            casovac.Stop();
                        }

                        else
                        {
                            hledaniVhodnehoPravilda();
                        }

                        break;
                    case "Cekas":
                        break;
                    case "Konec":
                        lvUdalosti.Items.Add("Konec hry");
                        break;
                    case "Krizek":
                        krizek = true;
                        kolecko = false;
                        lvUdalosti.Items.Add("Maš křížky");
                        break;
                    case "Kolecka":
                        kolecko = true;
                        krizek = false;
                        lvUdalosti.Items.Add("Maš kolečka");
                        break;
                    case "Vyhra":
                        vyhra(); 
                        break;
                    /*case "Prohra":
                        prohra();
                        break;*/
                    case "Remiza":
                        remiza();
                        break;
                    case "KonecTurnaje":
                        konecTurnaje();
                        break;
                    case "Test odesilani zpravy!":
                        break;
                    default:
                        break;
                }
                staraZprava = novaZprava;
            }
        }

        private void hlavniProgram()
        {
            casovac = new DispatcherTimer();
            casovac.Tick += new EventHandler(casovac_Tick);
            casovac.Interval = new TimeSpan(0, 0, 0, 0, 10);
        }

        void casovac_Tick(object sender, EventArgs e)
        {
            Sit s = new Sit(ipPoslechu, port, ipServer);
            zprava = s.prijataZprava();            
            reakceNaZpravy(zprava);
        }

        private void konecTurnaje()
        {
            //lInfo.Content = "Turnaj byl ukončen.";
            lvUdalosti.Items.Add("Turnaj skončil");
            lvUdalosti.Items.Add(string.Format("Výhry: {0}, prohry: {1}, remizy: {2}", pocetVyher, pocetProher, pocetRemiz));
            casovac.Stop();
        }
        
        private int KarpuvRabinuvAlgoritmus(string xHraciPlocha, string xPravidla)//zjištuje zda jsou stringy stejné
        {
            // string A = "0001100000";
            // string B = "00000";
            ulong hashHraciPlocha = 0;
            ulong hashPrvidla = 0;
            ulong prvocislo = 100007;
            ulong pocetZnaku = 256;

            string pomXpravidla = xPravidla;
            xPravidla = null;

            for (int i = 0; i < pomXpravidla.Length; i++)
            {
                if (pomXpravidla.Substring(i, 1) == "9")
                {
                    xPravidla += xHraciPlocha.Substring(i, 1);
                }
                if (pomXpravidla.Substring(i, 1) == "0")
                {
                    xPravidla +=0; 
                }
                if (pomXpravidla.Substring(i, 1) == "1")
                {
                    xPravidla +=1; 
                }
                if (pomXpravidla.Substring(i, 1) == "2")
                {
                    xPravidla +=2; 
                }

                hashHraciPlocha = (hashHraciPlocha * pocetZnaku + (ulong)xHraciPlocha[i]) % prvocislo;
                hashPrvidla = (hashPrvidla * pocetZnaku + (ulong)xPravidla[i]) % prvocislo;
            }

            if (hashHraciPlocha == hashPrvidla)
            {
                //lvUdalosti.Items.Add(string.Format("XX >>{0}<<{1}", AhraciPlocha.Substring(0, Bpravidla.Length), AhraciPlocha.Substring(Bpravidla.Length)));
                int i = xHraciPlocha.Substring(0, xPravidla.Length).Length;
                return 100;
            }
            
            ulong pow = 1;
            
            for (int k = 1; k <= xPravidla.Length - 1; k++)
                pow = (pow * pocetZnaku) % prvocislo;

            for (int j = 1; j <= xHraciPlocha.Length - xPravidla.Length; j++)
            {
                hashHraciPlocha = (hashHraciPlocha + prvocislo - pow * (ulong)xHraciPlocha[j - 1] % prvocislo) % prvocislo;
                hashHraciPlocha = (hashHraciPlocha * pocetZnaku + (ulong)xHraciPlocha[j + xPravidla.Length - 1]) % prvocislo;
                
                if (hashHraciPlocha == hashPrvidla)
                {
                    if (xHraciPlocha.Substring(j, xPravidla.Length) == xPravidla)
                    {
                        //lvUdalosti.Items.Add(string.Format("{0}>>{1}<<{2}", AhraciPlocha.Substring(0, j), AhraciPlocha.Substring(j, Bpravidla.Length), AhraciPlocha.Substring(j + Bpravidla.Length)));
                        return xHraciPlocha.Substring(0, j).Length;
                    }
                }
            }

            //lvUdalosti.Items.Add("Nenalezeno vhodné pravidlo");
            return -10;
        }

        /*private void kontrolaVyhry(string pravidlo)
        {
            string[] kontrola = pravidlo.Split(':');
            try
            {
                if (kontrola[2] == "1")
                {
                    lvUdalosti.Items.Add("Vyhra");
                    poslaniZpravyServeru("Vyhra");
                }
            }
            catch (IndexOutOfRangeException)
            {
            }
        }*/

        private int SouradniceServeruRadek(string pravidlo, int radek)
        {
            string[] kontrola = pravidlo.Split(':');

            int cislo = int.Parse(kontrola[1]);
            cislo += radek;
            cislo += 1;
            return cislo;           
        }

        private int SouradniceServeruSloupec(string pravidlo, int sloupec)
        {
            string[] kontrola = pravidlo.Split(':');

            int cislo = int.Parse(kontrola[2]);
            cislo += sloupec;
            cislo += 1;
            return cislo;  
        }

        private bool hledaniPravidla1x1()
        {
            int e1 = 10;
            bool exPra = false;

            for (int i = 0; i < 10; i++)
            {
                for (int k = 0; k < 10; k++)
                {
                    e1 = KarpuvRabinuvAlgoritmus(hraciPlocha[i].Substring(k, 1), "0");
                    if (e1 == 100)
                    {
                        int koleckoKrizek = 0;

                        if (krizek == true)
                        {
                            koleckoKrizek = 1;
                        }

                        if (kolecko == true)
                        {
                            koleckoKrizek = 2;
                        }

                        //int kx = sloupec(pravidlo[j], k);
                        //int ix = radek(pravidlo[j], i);

                        poslaniZpravyServeru((i + 1) + "-" + (k + 1) + "-" + koleckoKrizek);
                        exPra = true;

                        return true;
                    }

                    else
                    {
                        exPra = false;
                        //return false;
                    }
                }
            }
            
            if (exPra)
            {
                return true;
            }

            else
            {
                lvUdalosti.Items.Add("Není, kde zapsat");
                poslaniZpravyServeru("Remiza");
                return false;
            }
        }

        private bool hledaniPravidla2x2()
        {

            int e1 = 10, e2 = 20;
            bool exPra = false;

            for (int i = 0; i < 9; i++)
            {
                //hraciPlocha[i];
                for (int j = 0; j < cisloPravidla; j++)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        if (pravidlo[j].Length == 8)// 00 00 : 0 : 0
                        {
                            e1 = KarpuvRabinuvAlgoritmus(hraciPlocha[i].Substring(k, 2), pravidlo[j].Substring(0, 2));
                            e2 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 1].Substring(k, 2), pravidlo[j].Substring(2, 2));

                            if (e1 == 100 && e2 == 100)
                            {
                                int koleckoKrizek = 0;

                                if (krizek == true)
                                {
                                    koleckoKrizek = 1;  
                                }

                                if (kolecko == true)
                                {
                                    koleckoKrizek = 2;
                                }

                                int kx = SouradniceServeruSloupec(pravidlo[j], k);
                                int ix = SouradniceServeruRadek(pravidlo[j], i);

                                lvUdalosti.Items.Add("Nalezeno pravidlo 2x2");
                                lvUdalosti.Items.Add(pravidlo[j]);
                                poslaniZpravyServeru(ix + "-" + kx + "-" + koleckoKrizek);
                                exPra = true;

                                return true;
                            }

                            if (e1 == e2 && e1 > 0)
                            {
                                lvUdalosti.Items.Add("Nalezeno pravidlo 2");
                                lvUdalosti.Items.Add(pravidlo[j]);
                                exPra = true;

                                return true;
                            }

                            else
                            {
                                exPra = false;
                                //return false;
                            }
                        }
                    }
                }
            }

            if (exPra)
            {
                return true;
            }

            else
            {
                //lvUdalosti.Items.Add("Nebylo nalezeno pravidlo 2x2");
                return false;
            }
        }

        private bool hledaniPravidla3x3()
        {

            int e1 = 10, e2 = 20, e3 = 30;
            bool exPra = false;

            for (int i = 0; i < 8; i++)
            {
                //hraciPlocha[i];
                for (int j = 0; j < cisloPravidla; j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if (pravidlo[j].Length == 13)// 000 000 000 : 0 : 0
                        {
                            e1 = KarpuvRabinuvAlgoritmus(hraciPlocha[i].Substring(k, 3), pravidlo[j].Substring(0, 3));
                            e2 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 1].Substring(k, 3), pravidlo[j].Substring(3, 3));
                            e3 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 2].Substring(k, 3), pravidlo[j].Substring(6, 3));

                            if (e1 == 100 && e2 == 100 && e3 == 100) 
                            {
                                int koleckoKrizek = 0;

                                if (krizek == true)
                                {
                                    koleckoKrizek = 1;
                                }

                                if (kolecko == true)
                                {
                                    koleckoKrizek = 2;
                                }

                                int kx = SouradniceServeruSloupec(pravidlo[j], k);
                                int ix = SouradniceServeruRadek(pravidlo[j], i);

                                lvUdalosti.Items.Add("Nalezeno pravidlo 3x3");
                                lvUdalosti.Items.Add(pravidlo[j]);
                                poslaniZpravyServeru(ix + "-" + kx + "-" + koleckoKrizek);
                                exPra = true;

                                return true;
                            }

                            if (e1 == e2 && e2 == e3 && e1 > 0)
                            {
                                lvUdalosti.Items.Add("Nalezeno pravidlo 2");
                                lvUdalosti.Items.Add(pravidlo[j]);
                                exPra = true;

                                return true;
                            }

                            else
                            {
                                exPra = false;
                            }
                        }
                    }
                }
            }

            if (exPra)
            {
                return true;
            }

            else
            {
                //lvUdalosti.Items.Add("Nebylo nalezeno pravidlo 3x3");
                //poslaniZpravyServeru("Neni pravidlo");
                return false;
            }
        }

        private bool hledaniPravidla4x4()
        {
            int e1 = 10, e2 = 20, e3 = 30, e4 = 40;
            bool exPra = false;

            for (int i = 0; i < 7; i++)
            {
                //hraciPlocha[i];
                for (int j = 0; j < cisloPravidla; j++)
                {
                    if (pravidlo[j].Length == 20)// 0000 0000 0000 0000 : 0 : 0
                    {

                        for (int k = 0; k < 7; k++)
                        {
                            e1 = KarpuvRabinuvAlgoritmus(hraciPlocha[i].Substring(k, 4), pravidlo[j].Substring(0, 4));
                            e2 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 1].Substring(k, 4), pravidlo[j].Substring(4, 4));
                            e3 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 2].Substring(k, 4), pravidlo[j].Substring(8, 4));
                            e4 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 3].Substring(k, 4), pravidlo[j].Substring(12, 4));

                            if (e1 == 100 && e2 == 100 && e3 == 100 && e4 == 100)
                            {
                                int koleckoKrizek = 0;

                                if (krizek == true)
                                {
                                    koleckoKrizek = 1;
                                }

                                if (kolecko == true)
                                {
                                    koleckoKrizek = 2;
                                }

                                int kx = SouradniceServeruSloupec(pravidlo[j], k);
                                int ix = SouradniceServeruRadek(pravidlo[j], i);

                                lvUdalosti.Items.Add("Nalezeno pravidlo 4x4");
                                lvUdalosti.Items.Add(pravidlo[j]);
                                poslaniZpravyServeru(ix + "-" + kx + "-" + koleckoKrizek);
                                exPra = true;

                                return true;
                            }

                            if (e1 == e2 && e2 == e3 && e3 == e4 && e1 > 0)
                            {
                                lvUdalosti.Items.Add("Nalezeno pravidlo 2");
                                lvUdalosti.Items.Add(pravidlo[j]);
                                exPra = true;

                                return true;
                            }

                            else
                            {
                                exPra = false;
                            }
                        }
                    }
                }
            }

            if (exPra)
            {
                return true;
            }

            else
            {
                //lvUdalosti.Items.Add("Nebylo nalezeno pravidlo 4x4");
                //poslaniZpravyServeru("Neni pravidlo");
                return false;
            }
        }

        private bool hledaniPravidla5x5()
        {
            int e1 = 10, e2 = 20, e3 = 30, e4 = 40, e5 = 50;
            bool exPra = false;           

            for (int i = 0; i < 6; i++)
            {
                //hraciPlocha[i];
                for (int j = 0; j < cisloPravidla; j++)
                {
                    if (pravidlo[j].Length == 29) // 00000 00000 00000 00000 00000 : 0 : 0
                    {
                        for (int k = 0; k < 6; k++)
                        {
                            e1 = KarpuvRabinuvAlgoritmus(hraciPlocha[i].Substring(k, 5), pravidlo[j].Substring(0, 5));
                            e2 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 1].Substring(k, 5), pravidlo[j].Substring(5, 5));
                            e3 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 2].Substring(k, 5), pravidlo[j].Substring(10, 5));
                            e4 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 3].Substring(k, 5), pravidlo[j].Substring(15, 5));
                            e5 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 4].Substring(k, 5), pravidlo[j].Substring(20, 5));
                            
                            if (e1 == 100 && e2 == 100 && e3 == 100 && e4 == 100 && e5 == 100)
                            {
                                int koleckoKrizek = 0;

                                if (krizek == true)
                                {
                                    koleckoKrizek = 1;
                                }

                                if (kolecko == true)
                                {
                                    koleckoKrizek = 2;
                                }

                                int kx = SouradniceServeruSloupec(pravidlo[j], k);
                                int ix = SouradniceServeruRadek(pravidlo[j], i);

                                lvUdalosti.Items.Add("Nalezeno pravidlo");
                                lvUdalosti.Items.Add(pravidlo[j]);
                                poslaniZpravyServeru(ix + "-" + kx + "-" + koleckoKrizek);
                                exPra = true;

                                return true;
                            }

                            if (e1 == e2 && e2 == e3 && e3 == e4 && e4 == e5 && e1 > 0)
                            {
                                lvUdalosti.Items.Add("Nalezeno pravidlo 2");
                                lvUdalosti.Items.Add(pravidlo[j]);
                                exPra = true;

                                return true;
                            }

                            else
                            {
                                exPra = false;
                            }
                        }
                    }
                }
            }

            if (exPra)
            {
                return true;
            }

            else
            {
                //lvUdalosti.Items.Add("Nebylo nalezeno pravidlo 5x5");
                //poslaniZpravyServeru("Neni pravidlo");
                return false;
            }
        }

        private void hledaniVhodnehoPravilda()
        {
            //bool existencePravidla = false;

            if (hledaniPravidla5x5())
            {
                //kontrolaVyhry();
            }

            else if (hledaniPravidla4x4())
            {
                //kontrolaVyhry();
            }

            else if (hledaniPravidla3x3())
            {
                //kontrolaVyhry();
            }

            else if (hledaniPravidla2x2())
            {
                //kontrolaVyhry();
            }

            else if (hledaniPravidla1x1())
            {
                //kontrolaVyhry();
            }
        }

        private void nacteniHraciPlochy()
        {
            Databaze db = new Databaze(tbDatabaze.Text, tbUser.Text, tbHeslo.Text);

            db.Pripojit();
            db.nacistHraciPlochy();
            celaHraciPlocha = db.ziskatHraciPlochu();
            db.Odpojit();

            if (kolecko == true)
            {
                pomCelaHraciPlocha = celaHraciPlocha;
                celaHraciPlocha = null;
                for (int j = 0; j < pomCelaHraciPlocha.Length; j++)
                {
                    string pom = pomCelaHraciPlocha.Substring(j, 1);
                    if (pom == "0")
                    {
                        celaHraciPlocha += 0;
                    }
                    if (pom == "1")
                    {
                        celaHraciPlocha += 2;
                    }
                    if (pom == "2")
                    {
                        celaHraciPlocha += 1;
                    }
                }
            }
            try
            {
                hraciPlocha[0] = celaHraciPlocha.Substring(0, 10);
                hraciPlocha[1] = celaHraciPlocha.Substring(10, 10);
                hraciPlocha[2] = celaHraciPlocha.Substring(20, 10);
                hraciPlocha[3] = celaHraciPlocha.Substring(30, 10);
                hraciPlocha[4] = celaHraciPlocha.Substring(40, 10);
                hraciPlocha[5] = celaHraciPlocha.Substring(50, 10);
                hraciPlocha[6] = celaHraciPlocha.Substring(60, 10);
                hraciPlocha[7] = celaHraciPlocha.Substring(70, 10);
                hraciPlocha[8] = celaHraciPlocha.Substring(80, 10);
                hraciPlocha[9] = celaHraciPlocha.Substring(90, 10);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("Chyba načtení hrací plochy: \n\n"+ex.Message,"Chyba",MessageBoxButton.OK,MessageBoxImage.Error);
            }

            for (int i = 0; i < 10; i++)
            {
                lvUdalosti.Items.Add(hraciPlocha[i]);
            }
        }
        
        private bool konecHryRadek()
        {
            int e1 = 10;//, e2 = 20, e3 = 30, e4 = 40, e5 = 50;
            bool exPra = false;

            for (int i = 0; i < 10; i++)
            {
                    for (int k = 0; k < 6; k++)
                    {
                        e1 = KarpuvRabinuvAlgoritmus(hraciPlocha[i].Substring(k, 5), "22222");

                        if (e1 == 100)
                        {
                            exPra = true;
                            return true;
                        }

                        else
                        {
                            exPra = false;
                        }
                }
            }

            if (exPra)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        private bool konecHrySloupec()
        {
            int e1 = 10, e2 = 20, e3 = 30, e4 = 40, e5 = 50;
            bool exPra = false;

            for (int i = 0; i < 6; i++)
            {
                for (int k = 0; k < 10; k++)
                {
                    e1 = KarpuvRabinuvAlgoritmus(hraciPlocha[i].Substring(k, 1), "2");
                    e2 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 1].Substring(k, 1), "2");
                    e3 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 2].Substring(k, 1), "2");
                    e4 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 3].Substring(k, 1), "2");
                    e5 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 4].Substring(k, 1), "2");

                    if (e1 == 100 && e2 == 100 && e3 == 100 && e4 == 100 && e5 == 100)
                    {
                        exPra = true;
                        return true;
                    }

                    else
                    {
                        exPra = false;
                    }
                }
            }

            if (exPra)
            {
                return true;
            }

            else
            {
                return false;
            }
        }
        
        private bool konecHrySikmo()
        {
            int e1 = 10, e2 = 20, e3 = 30, e4 = 40, e5 = 50;
            bool exPra = false;

            for (int i = 0; i < 6; i++)
            {
                for (int k = 0; k < 6; k++)
                {
                    e1 = KarpuvRabinuvAlgoritmus(hraciPlocha[i].Substring(k, 1), "2");
                    e2 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 1].Substring(k + 1, 1), "2");
                    e3 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 2].Substring(k + 2, 1), "2");
                    e4 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 3].Substring(k + 3, 1), "2");
                    e5 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 4].Substring(k + 4, 1), "2");

                    if (e1 == 100 && e2 == 100 && e3 == 100 && e4 == 100 && e5 == 100)
                    {
                        exPra = true;
                        return true;
                    }

                    else
                    {
                        exPra = false;
                    }
                }
            }

            if (exPra)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        private bool konecHrySikmo2()
        {
            int e1 = 10, e2 = 20, e3 = 30, e4 = 40, e5 = 50;
            bool exPra = false;

            for (int i = 0; i < 6; i++)
            {
                for (int k = 0; k < 6; k++)
                {
                    e1 = KarpuvRabinuvAlgoritmus(hraciPlocha[i].Substring(k + 4, 1), "2");
                    e2 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 1].Substring(k + 3, 1), "2");
                    e3 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 2].Substring(k + 2, 1), "2");
                    e4 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 3].Substring(k + 1, 1), "2");
                    e5 = KarpuvRabinuvAlgoritmus(hraciPlocha[i + 4].Substring(k, 1), "2");

                    if (e1 == 100 && e2 == 100 && e3 == 100 && e4 == 100 && e5 == 100)
                    {
                        exPra = true;
                        return true;
                    }

                    else
                    {
                        exPra = false;
                    }
                }
            }

            if (exPra)
            {
                return true;
            }

            else
            {
                return false;
            }
        }

        private bool kontrolaProhry()
        {
            if (konecHryRadek())
            {
                //poslaniZpravyServeru("Prohra");
                return true;
            }

            else if (konecHrySloupec())
            {
                //poslaniZpravyServeru("Prohra");
                return true;
            }

            else if (konecHrySikmo())
            {
                //poslaniZpravyServeru("Prohra");
                return true;
            }

            else if (konecHrySikmo2())
            {
                //poslaniZpravyServeru("Prohra");
                return true;
            }
            else
            {
                return false;
            }
        }
                
        //TLAČÍTKA

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {            
            nacteniHraciPlochy();
            //kontrolaVyhry();
            hledaniVhodnehoPravilda();
        }

        private void btnVytvoreniPravidla_Click(object sender, RoutedEventArgs e)
        {
            vytvoreniPravidla();
            //cbViteznePravidlo.IsChecked = false;
        }

        private void btnUlozeniPravidel_Click(object sender, RoutedEventArgs e)
        {
            ulozeniPravidelTxt();
        }

        private void btnNacteniPravidel_Click(object sender, RoutedEventArgs e)
        {
            nacteniPravidelTxt();
        }

        private void btnPripojit_Click(object sender, RoutedEventArgs e)
        {
            pripojeniDatabaze();
        }

        private void btnPripojeniServer_Click(object sender, RoutedEventArgs e)
        {
            poslechSite();
        }
        
        private void btnPridatHraceDoDatabaze_Click(object sender, RoutedEventArgs e)
        {
            pridaniHraceDoDatabaze();
        }
        
        private void btnPridaniPravidla_Click(object sender, RoutedEventArgs e)
        {
            pridaníPravidlaDoDatabaze();
        }
        
        private void tbPocetPravidel_TextChanged(object sender, TextChangedEventArgs e)
        {
            int pom;
            try
            {
                pom = int.Parse(tbPocetPravidel.Text);
            }

            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Chyba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            catch (OverflowException ex)
            {
                MessageBox.Show(ex.Message, "Chyba!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (pom > 10)
            {
                try
                {
                    pravidlo = new string[pom];
                }
                catch (OutOfMemoryException ex)
                {
                    MessageBox.Show(ex.Message, "Chyba!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //lInfo.Content = "Maximální počet pravidel je " + pom;
                lvUdalosti.Items.Add("Změna max počtu pravidel na " + pom);
            }
        }

        private void tbServer_TextChanged(object sender, TextChangedEventArgs e)
        {
            ipServer = tbServer.Text;
        }

        private void tbPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                port = int.Parse(tbPort.Text);
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.Message, "Chyba!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (OverflowException ex)
            {
                MessageBox.Show(ex.Message, "Chyba!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnKonec_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult odpoved = MessageBox.Show("Opravdu chcete ukončit program.", "Ukončení", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (odpoved == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private void btnSpustit_Click(object sender, RoutedEventArgs e)
        {
            tbPocetPravidel.Text = "100";
            pripojeniDatabaze();
            //odpojeniDatabaze();
            hlavniProgram();
            poslechSite();
            pridaniHraceDoDatabaze();
            nacteniPravidelTxt();
            pridaníPravidlaDoDatabaze();
        }

        private void cbZobrazeniUdalosti_Checked(object sender, RoutedEventArgs e)
        {
            hlavniOkno.Width = 630;
        }

        private void cbZobrazeniUdalosti_Unchecked(object sender, RoutedEventArgs e)
        {
            hlavniOkno.Width = 220;
        }

        private void cbTvorbaPravidel_Checked(object sender, RoutedEventArgs e)
        {
            hlavniOkno.Height = 750;
        }

        private void cbTvorbaPravidel_Unchecked(object sender, RoutedEventArgs e)
        {
            hlavniOkno.Height = 370;
        }

        private void btnZprava_Click(object sender, RoutedEventArgs e)
        {
            poslaniZpravyServeru(btZpravaProServer.Text);
        }

        private void tbIpPoslechu_TextChanged(object sender, TextChangedEventArgs e)
        {
            ipPoslechu = tbIpPoslechu.Text;
        }
    }
}