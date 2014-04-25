using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Piskvorky_klient_Pfeiffer
{
    public class Databaze
    {
        private string ip;
        private string login;
        private string password;

        static int id;

        string hraciPlocha = null;// = new string[10];

        SqlConnection datovePripojeni = new SqlConnection();

        public Databaze(string ipadresa, string user, string heslo)
        {
            ip = ipadresa;
            login = user;
            password = heslo;
            ip += "\\SQL2008R2";
        }

        public void Pripojit()
        {
            try
            {
                SqlConnectionStringBuilder konfigurace = new SqlConnectionStringBuilder();
                konfigurace.DataSource = ip;//+"\SQL2008R2";   //".\\SQLExpress";
                konfigurace.InitialCatalog = "Piskvorky";
                konfigurace.UserID = login;
                konfigurace.Password = password;
                //konfigurace.IntegratedSecurity = true;
                datovePripojeni.ConnectionString = konfigurace.ConnectionString;
                datovePripojeni.Open();
            }
            catch (SqlException e)
            {
                MessageBox.Show(String.Format("Chyba při vstupu do DB: \n\n{0}", e.Message));
                return;
            }
        }

        public void PridatHrace(string jmenoHrace, int cisloPortu)
        {
            try
            {
                SqlCommand datovyPrikaz = new SqlCommand();
                datovyPrikaz.Connection = datovePripojeni;
                datovyPrikaz.CommandType = CommandType.Text;
                datovyPrikaz.CommandText = "Insert Into Hrac(id,pocet_bodu,jmeno,port) output inserted.id values (2,0,@JmenoHrace,@CisloPortu)";

                SqlParameter param = new SqlParameter("@JmenoHrace", SqlDbType.VarChar, 20);
                param.Value = jmenoHrace;
                datovyPrikaz.Parameters.Add(param);

                SqlParameter param2 = new SqlParameter("@CisloPortu", SqlDbType.Int);
                param2.Value = cisloPortu;
                datovyPrikaz.Parameters.Add(param2);

                //  datovyPrikaz.ExecuteNonQuery();
                if (id == 0)
                {
                    id = (int)datovyPrikaz.ExecuteScalar();    //RIKA ID VLOZENEHO HRACE   
                }

                //MessageBox.Show("Id hráče: " + id, "ID");
            }
            catch (SqlException e)
            {
                MessageBox.Show("Chyba vložení do DB: \n\n" + e.Message);
                return;

            }
            finally
            {
                datovePripojeni.Close();
            }

        }
        public void Odpojit()
        {
            datovePripojeni.Close();
        }

        public void PridatPravidlo(string p)
        {
            try
            {

                SqlCommand datovyPrikaz = new SqlCommand();
                datovyPrikaz.Connection = datovePripojeni;
                datovyPrikaz.CommandType = CommandType.Text;
                datovyPrikaz.CommandText = "Insert Into Pravidlo(hrac_id,pravidlo) values (@i,@p)";

                SqlParameter param = new SqlParameter("@p", SqlDbType.VarChar, 60);
                param.Value = p;
                datovyPrikaz.Parameters.Add(param);

                SqlParameter param2 = new SqlParameter("@i", SqlDbType.Int);
                param2.Value = id;
                datovyPrikaz.Parameters.Add(param2);

                datovyPrikaz.ExecuteNonQuery();

            }
            catch (SqlException e)
            {
                MessageBox.Show(string.Format("Chyba vložení do DB: \n\n{0}", e.Message));
                return;

            }
            finally
            {
                datovePripojeni.Close();
            }
        }

        public void nacistHraciPlochy()//int idHraciPlocha)
        {
            try
            {
                SqlCommand datovyPrikaz = new SqlCommand();
                datovyPrikaz.Connection = datovePripojeni;
                datovyPrikaz.CommandType = CommandType.Text;
                datovyPrikaz.CommandText = "select hra from hraciPlocha";// whatever where id = @Id";
                //"SELECT TOP 10 [id], [hra] FROM [Piskvorky].[dbo].[hraciPlocha]";
                //"Insert Into Hrac(pocet_bodu,jmeno) output inserted.id values (0,@JmenoHrace)";

                /*SqlParameter param = new SqlParameter("@Id",SqlDbType.Int);
                param.Value = idHraciPlocha;
                datovyPrikaz.Parameters.Add(param);*/

                //  datovyPrikaz.ExecuteNonQuery();
                //idHraciPlocha = (int)datovyPrikaz.ExecuteScalar(); 
                //hraciPlocha[idHraciPlocha] = (datovyPrikaz.ExecuteScalar().ToString());
                hraciPlocha = (datovyPrikaz.ExecuteScalar().ToString());
            }
            catch (SqlException e)
            {
                MessageBox.Show(string.Format("Chyba načtení z DB: \n\n{0}", e.Message));
                return;
            }
            finally
            {
                datovePripojeni.Close();
            }
        }

        internal string ziskatHraciPlochu()//(int x)
        {
            return hraciPlocha;//[x];
        }

        internal int ziskatIdHrace()
        {
            return id;
        }
    }
}
