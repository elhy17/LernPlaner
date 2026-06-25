using MySql.Data.MySqlClient;

namespace App_LernPlaner.Database
{
    public class db_verbindung
    {
        string connectionString = "server=mysql.pb.bib.de;uid=pbt3h24aas;pwd=d7mh9brzLcbm;database=pbt3h24aas_Nexus;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

    }

}

