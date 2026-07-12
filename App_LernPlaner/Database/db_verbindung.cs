using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using MySql.Data.MySqlClient;
namespace App_LernPlaner.Database
{
    public class db_verbindung
    {

        private const string EnvironmentVariableName = "STUDYMASTER_DB";
        private const string ConfigConnectionName = "StudyMasterDb";

        private readonly string _connectionString;

        public db_verbindung()
        {
            _connectionString = LoadConnectionString();
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        private static string LoadConnectionString()
        {
            string? fromEnvironment =
                Environment.GetEnvironmentVariable(EnvironmentVariableName);

            if (!string.IsNullOrWhiteSpace(fromEnvironment))
            {
                return fromEnvironment;
            }

            string? fromConfig =
                ConfigurationManager.ConnectionStrings[ConfigConnectionName]?.ConnectionString;

            if (!string.IsNullOrWhiteSpace(fromConfig))
            {
                return fromConfig;
            }

            throw new InvalidOperationException(
                "Connection String fehlt. Bitte STUDYMASTER_DB als Environment Variable setzen oder StudyMasterDb in App.config eintragen.");
        }


    }

}

