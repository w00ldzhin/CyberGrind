using igraUP.Models;
using System.Windows;

namespace igraUP.Views
{
    public partial class GameOverWindow : Window
    {
        private User _currentUser;

        public GameOverWindow(int score, int kills, string time, User user)
        {
            InitializeComponent();
            _currentUser = user;

            TxtScore.Text = score.ToString();
            TxtKills.Text = kills.ToString();
            TxtTime.Text = time;
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            var game = new GameWinndow(_currentUser);
            game.Show();
            this.Close();
        }

        private void BtnMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = new MainWindow(_currentUser);
            menu.Show();
            this.Close();
        }
    }
}