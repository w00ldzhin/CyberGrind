using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Timers;
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
    public partial class WarningWindow : Window
    {
        private const int TotalSeconds = 10;
        private int secondsLeft = TotalSeconds;
        private readonly DispatcherTimer timer;
        public WarningWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += Timer_Tick;

           
            timer.Start();
        }

       
             

        private void Timer_Tick(object sender, EventArgs e)
        {
            secondsLeft--;
       

            if (secondsLeft <= 0)
            {
                timer.Stop();
                LoadingWindow loading = new LoadingWindow(null);
                loading.Show();
                this.Close();
            }
        }

   
    }
}