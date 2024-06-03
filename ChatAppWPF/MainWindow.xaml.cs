using System;
using System.Windows;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;

namespace ChatAppWPF
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_OpenLoginWindow_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            this.Close();
        }
    }
}

