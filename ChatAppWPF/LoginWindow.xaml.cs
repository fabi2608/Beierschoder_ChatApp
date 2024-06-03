using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatAppWPF
{
    public partial class LoginWindow : Window
    {
        private readonly HttpClient _httpClient;

        public LoginWindow()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7037/api/");
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var username = txtbox_username.Text;
            var password = txtbox_password.Password;

            try
            {
                var newUser = new User { Username = username, Password = password };

                var response = await _httpClient.PostAsJsonAsync("User", newUser);
                MessageBox.Show(response.StatusCode.ToString());
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Registrierung erfolgreich");
                    OpenHomeWindow(username);
                }
                else
                {
                    MessageBox.Show("Fehler bei der Registrierung: " + response.StatusCode.ToString());
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Fehler bei der Registrierung - Exception: {ex.Message}");
            }
        }
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = txtbox_username.Text;
            var password = txtbox_password.Password;

            try
            {
                var response = await _httpClient.PostAsJsonAsync("User/Login", new User { Username = username, Password = password });

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Anmeldung erfolgreich");
                    OpenHomeWindow(username);
                }
                else
                {
                    MessageBox.Show("Fehler bei der Anmeldung: " + response.StatusCode.ToString());
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Fehler bei der Anmeldung - Exception: {ex.Message}");
            }
        }

        private void OpenHomeWindow(string username)
        {
            HomeWindow homeWindow = new HomeWindow(username);
            homeWindow.Show();

            this.Close();
        }
    }
}
