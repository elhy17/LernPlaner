using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace App_LernPlaner.Services
{
    /// <summary>FA-34: Noten als CSV/Excel-kompatible Datei exportieren.</summary>
    public class ExportService
    {
        public void ExportGradesToCsv(string filePath, string taskTitle, IEnumerable<(string StudentName, string Grade, string Feedback, DateTime SubmittedAt)> rows)
        {
            AppendGradesToCsv(filePath, taskTitle, rows, includeHeader: true, append: false);
        }

        // includeHeader=false und append=true erlauben es, mehrere Aufgaben nacheinander in dieselbe Datei zu schreiben
        // (siehe StatistikPage: eine Zeile pro Aufgabe/Schüler über alle Klassen des Lehrers).
        public void AppendGradesToCsv(string filePath, string taskTitle, IEnumerable<(string StudentName, string Grade, string Feedback, DateTime SubmittedAt)> rows, bool includeHeader, bool append = true)
        {
            var sb = new StringBuilder();
            if (includeHeader)
                sb.AppendLine("Aufgabe;Schüler;Note;Feedback;Abgegeben am");

            foreach (var row in rows)
                sb.AppendLine($"{Csv(taskTitle)};{Csv(row.StudentName)};{Csv(row.Grade)};{Csv(row.Feedback)};{row.SubmittedAt:dd.MM.yyyy HH:mm}");

            if (append && File.Exists(filePath))
                File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
            else
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string Csv(string? value)
        {
            value ??= string.Empty;
            return value.Contains(';') || value.Contains('"')
                ? "\"" + value.Replace("\"", "\"\"") + "\""
                : value;
        }
    }
}
