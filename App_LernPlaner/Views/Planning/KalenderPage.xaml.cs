using System.Windows.Controls;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Planning
{
    public partial class KalenderPage : Page
    {
        public KalenderPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            DeadlinesList.ItemsSource = await new CalendarService().GetUpcomingDeadlinesAsync(user);
        }
    }
}
