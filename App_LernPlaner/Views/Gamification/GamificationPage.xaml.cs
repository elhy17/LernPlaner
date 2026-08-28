using System.Linq;
using System.Windows.Controls;
using App_LernPlaner.Models;
using App_LernPlaner.Services;

namespace App_LernPlaner.Views.Gamification
{
    public class LeaderboardRowVm
    {
        public int Rank { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Xp { get; set; }
    }

    // FA-30 bis FA-32: Punkte, Badges, Rangliste
    public partial class GamificationPage : Page
    {
        private readonly GamificationService _gamificationService = new();
        private readonly ClassService _classService = new();
        private readonly Repositories.UserRepository _users = new();

        public GamificationPage()
        {
            InitializeComponent();
            Loaded += async (_, _) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync()
        {
            var user = sessionManager.CurrentUser;
            if (user == null) return;

            int xp = AppConfig.UseMockData ? user.XPPoints : (await _users.GetByIdAsync(user.UserId))?.XPPoints ?? user.XPPoints;
            LblXp.Text = $"{xp} XP";
            LblLevel.Text = $"Level {(xp / 200) + 1}";

            BadgesList.ItemsSource = await _gamificationService.GetBadgesAsync(user.UserId);

            var classes = await _classService.GetClassesForUserAsync(user.UserId);
            CmbClass.ItemsSource = classes;
            if (classes.Count > 0) CmbClass.SelectedIndex = 0;
        }

        private async void CmbClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbClass.SelectedItem is not Class cls) return;

            var leaderboard = await _gamificationService.GetLeaderboardAsync(cls.ClassId);
            LeaderboardList.ItemsSource = leaderboard.Select((u, i) => new LeaderboardRowVm
            {
                Rank = i + 1,
                UserName = u.UserName,
                Xp = u.XPPoints
            }).ToList();
        }
    }
}
