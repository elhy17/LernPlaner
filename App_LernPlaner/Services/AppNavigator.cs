using System;
using System.Windows.Controls;

namespace App_LernPlaner.Services
{
    /// <summary>
    /// Einfache zentrale Navigation für Dashboard-Bereiche: jede Seite kann
    /// AppNavigator.NavigateTo(...) aufrufen, um den Inhalt des Haupt-Frames zu
    /// wechseln, ohne eigene Fenster (Windows) zu öffnen.
    /// </summary>
    public static class AppNavigator
    {
        public static Action<Page>? NavigateAction;

        public static void NavigateTo(Page page) => NavigateAction?.Invoke(page);
    }
}
