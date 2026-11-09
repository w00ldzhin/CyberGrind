using igraUP.Models;
using igraUP.Service;
using igraUP.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace igraUP
{
    public partial class MainWindow : Window
    {
        private User _currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            TxtCurrentUser.Text = _currentUser.login;
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            LoadingWindow loading = new LoadingWindow(_currentUser);
            loading.Show();
            this.Close();
        }

        private void BtnLeaderboard_Click(object sender, RoutedEventArgs e)
        {
            var leaderboard = new LeaderBoardWindow(_currentUser);
            leaderboard.Show();
            this.Close();
        }
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void BtnInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Игра разработана студентом группы ПР-321/Б в ЮУрГТК - Черепановым В.В. (R3lo|w00ldzhin) для учебной практики 12-24 октября 2026 года");
        }
    }
}