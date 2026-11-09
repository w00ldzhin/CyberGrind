using igraUP.Models;
using igraUP.Service;
using igraUP.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace igraUP
{
    public partial class AuthWindow : Window
    {
        public DispatcherTimer timer;
        public DispatcherTimer timer2;
        public readonly char[] noise = { '#', '@', '$', '%', '&', 'X', ',', '.', '^', ';', ':', '-', '0', '?', '/', '*', '(', ')', '[', ']', '!', '|', '"', '{', '}', };
        public readonly Random ran = new Random();

        public AuthWindow()
        {
            InitializeComponent();

            background1.Tag = background1.Text;
            background2.Tag = background2.Text;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(150);
            timer.Tick += Timer_Tick;
            timer.Start();
            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(200);
            timer2.Tick += Timer2_Tick;
            timer2.Start();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Login(LoginBox.Text, PasswordBox.Password, out string loginerror);
            if (loginerror != string.Empty)
            {
                PassNotes.Text = loginerror;
            }
            else
            {
                User user = SaveService.LoadUsers().FirstOrDefault(u => u.login == LoginBox.Text);

                if (user != null)
                {
                    var game = new MainWindow(user);
                    game.Show();
                    this.Close();
                }
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            AuthService.Register(LoginBox.Text, PasswordBox.Password, out string regerror);
            if (regerror != string.Empty)
            {
                PassNotes.Text = regerror;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (background1.Tag == null) return;

            string original = background1.Tag.ToString();
            StringBuilder sb = new StringBuilder();

            foreach (char c in original)
            {
                if (c == ' ') { sb.Append(' '); continue; }

                int roll = ran.Next(0, 100);

                if (roll < 20)
                {
                    sb.Append(noise[ran.Next(noise.Length)]);
                }
                else if (roll < 40)
                {
                    sb.Append(ran.Next(0, 2) == 0 ? char.ToUpper(c) : char.ToLower(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            background1.Text = sb.ToString();
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            if (background2.Tag == null) return;

            string original = background2.Tag.ToString();
            StringBuilder sb = new StringBuilder();

            foreach (char c in original)
            {
                if (c == ' ') { sb.Append(' '); continue; }

                int roll = ran.Next(0, 100);

                if (roll < 10)
                {
                    sb.Append(noise[ran.Next(noise.Length)]);
                }
                else if (roll < 20)
                {
                    sb.Append(ran.Next(0, 2) == 0 ? char.ToUpper(c) : char.ToLower(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            background2.Text = sb.ToString();
        }
    }
}