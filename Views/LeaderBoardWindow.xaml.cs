using igraUP.Models;
using igraUP.Service;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace igraUP.Views
{
    public partial class LeaderBoardWindow : Window
    {
        private User _currentUser;

        public LeaderBoardWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadData();
        }

        private void LoadData()
        {
            var scores = SaveService.LoadScores();

            var displayScores = scores
                .GroupBy(s => s.Username)
                .Select(g => g.OrderByDescending(s => s.Points).First())
                .OrderByDescending(s => s.Points)
                .Select((s, index) => new
                {
                    Rank = index + 1,
                    Username = s.Username,
                    Points = s.Points,
                    Time = s.Time
                })
                .ToList();

            LstScores.ItemsSource = displayScores;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            var menu = new MainWindow(_currentUser);
            menu.Show();
            this.Close();
        }
    }
}