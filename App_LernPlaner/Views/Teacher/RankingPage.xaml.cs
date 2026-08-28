using System.Linq;
using System.Windows.Controls;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Teacher
{
    // FA-32 (Rangliste), aus Lehrersicht: Orientierung für die Notengebung
    public partial class RankingPage : Page
    {
        private readonly ClassService _classService = new();
        private readonly GamificationService _gamificationService = new();

        public RankingPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadClassesAsync();
        }

        private async System.Threading.Tasks.Task LoadClassesAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            var classes = (await _classService.GetAllClassesAsync()).Where(c => user.IsAdmin || c.TeacherId == user.UserId).ToList();
            CmbClass.ItemsSource = classes;
            if (classes.Count > 0) CmbClass.SelectedIndex = 0;
        }

        private async void CmbClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbClass.SelectedItem is not Class cls) return;

            var leaderboard = await _gamificationService.GetLeaderboardAsync(cls.ClassId);
            LeaderboardList.ItemsSource = leaderboard.Select((u, i) => new Gamification.LeaderboardRowVm
            {
                Rank = i + 1,
                UserName = u.UserName,
                Xp = u.XPPoints
            }).ToList();
        }
    }
}
