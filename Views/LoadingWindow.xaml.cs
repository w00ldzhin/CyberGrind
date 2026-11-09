using igraUP.Models;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace igraUP.Views
{
    public partial class LoadingWindow : Window
    {
        private User _currentUser;
        private DispatcherTimer _timer;
        private BitmapImage[] _frames;
        private int _currentFrame = 0;
        private int _totalTicks = 0;

        public LoadingWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            _frames = new BitmapImage[59];
            for (int i = 0; i < 59; i++)
            {
                _frames[i] = new BitmapImage(new Uri($"pack://application:,,,/Images/LoadingFlashes/spr_victory_flashes_{i}.png"));
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(15);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            FlashImage.Source = _frames[_currentFrame];
            _currentFrame++;
            _totalTicks++;

            if (_currentFrame >= _frames.Length)
                _currentFrame = 0;

            if (_totalTicks > 60)
            {
                _timer.Stop();

                if (_currentUser == null)
                {
                    var auth = new AuthWindow();
                    auth.Show();
                }
                else
                {
                    var game = new GameWinndow(_currentUser);
                    game.Show();
                }
                this.Close();
            }
        }
    }
}