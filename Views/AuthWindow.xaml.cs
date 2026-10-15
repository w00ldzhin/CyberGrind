using igraUP.Data;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using igraUP.Service;

namespace igraUP
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();

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
                 MainWindow mainWindow = new MainWindow();
                  mainWindow.Show();
                    this.Close();
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
    }
}
