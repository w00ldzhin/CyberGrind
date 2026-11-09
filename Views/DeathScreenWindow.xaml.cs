using igraUP.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace igraUP.Views
{
    public partial class DeathScreenWindow : Window
    {
        private DispatcherTimer _timer;
        private BitmapImage[] _frames;
        private int _currentFrame = 0;

        private int _score;
        private int _kills;
        private string _time;
        private User _user;

        public DeathScreenWindow(int score, int kills, string time, User user)
        {
            InitializeComponent();
            _score = score;
            _kills = kills;
            _time = time;
            _user = user;

            _frames = new BitmapImage[24];
            for (int i = 0; i < 24; i++)
            {
                _frames[i] = new BitmapImage(new Uri($"pack://application:,,,/Images/DeathFlashes/spr_death_flashes_{i}.png"));
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_currentFrame < _frames.Length)
            {
                FlashImage.Source = _frames[_currentFrame];
                _currentFrame++;
            }
            else
            {
                _timer.Stop();
                var gameOver = new GameOverWindow(_score, _kills, _time, _user);
                gameOver.Show();
                this.Close();
            }
        }
    }
}