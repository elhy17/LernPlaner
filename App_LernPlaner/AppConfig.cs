namespace App_LernPlaner
{
    /// <summary>
    /// Zentraler Schalter zwischen Mock-Daten (Entwicklung ohne DB-Zugriff)
    /// und der echten MySQL-Datenbank (mysql.pb.bib.de).
    /// </summary>
    public static class AppConfig
    {
        public static bool UseMockData = false;
    }
}
