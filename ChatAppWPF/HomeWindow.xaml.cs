using ChatAppWPF.Services;
using MongoDB.Bson;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ChatAppWPF
{
    public partial class HomeWindow : Window
    {
        private readonly HttpClient _httpClient;
        private ChatService _chatService;
        private MessageService _messageService;

        private string _username = "";
        private string _targetUsername = "";

        private List<Chat> _chatsList = new List<Chat>();
        public HomeWindow(string username)
        {
            InitializeComponent();

            _httpClient = new HttpClient();
            _chatService = new ChatService();
            _messageService = new MessageService();
            _httpClient.BaseAddress = new Uri("https://localhost:7037/api/");

            _username = username;
            lst_searchresults.Visibility = Visibility.Collapsed;
            lbl_searchresults.Visibility = Visibility.Collapsed;
            lbl_usernamedisplay.Visibility = Visibility.Collapsed;
            lbl_usernamedisplay.Content = _username;
            btn_logout.Visibility = Visibility.Collapsed;
            btn_deleteaccount.Visibility = Visibility.Collapsed;
            LoadChatsForUser(_username);

        }
        
        private async void LoadChatsForUser(string username)
        {
            try
            {
                List<Chat> chatParticipants = await _chatService.GetChatsForUser(username);

                foreach (var chat in chatParticipants)
                {
                    _chatsList.Add(chat);
                    string chatPartner = chat.Participants.FirstOrDefault(participant => participant != username);
                    if (chatPartner != null)
                    {
                        lst_chats.Items.Add(chatPartner);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading chats: {ex.Message}");
            }
        }
        private async void lst_chats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedParticipant = lst_chats.SelectedItem as string;
            _targetUsername = selectedParticipant;
            lbl_chatpartnerdisplay.Content = selectedParticipant;

            if (selectedParticipant != null)
            {
                try
                {
                    List<Message> chatMessages = await _chatService.GetChatHistoryAsync(_username, selectedParticipant);

                    lst_chat.Items.Clear();

                    DateTime? currentDate = null;

                    foreach (var message in chatMessages)
                    {
                        if (currentDate == null || message.Timestamp.Date != currentDate)
                        {
                            TextBlock dateTextBlock = new TextBlock
                            {
                                Text = message.Timestamp.ToShortDateString(),
                                HorizontalAlignment = HorizontalAlignment.Center,
                                TextAlignment = TextAlignment.Center,
                                Foreground = Brushes.Gray
                            };
                            lst_chat.Items.Add(dateTextBlock);
                            currentDate = message.Timestamp.Date;
                        }

                        string timeString = message.Timestamp.ToShortTimeString();
                        string messageString = $"{message.Author}: {message.Text}";
                        lst_chat.Items.Add(new TextBlock
                        {
                            Inlines =
                    {
                        new Run(timeString)
                        {
                            FontSize = 10,
                            Foreground = Brushes.Gray
                        },
                        new Run(" "),
                        new Run(messageString)
                    }
                        });

                        lst_chat.ScrollIntoView(lst_chat.Items[lst_chat.Items.Count - 1]);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading chat: {ex.Message}");
                }
            }
        }





        private async void Txtbox_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = txtbox_search.Text;

            if (string.IsNullOrEmpty(query))
            {
                lst_searchresults.Visibility = Visibility.Collapsed;
                lbl_searchresults.Visibility = Visibility.Collapsed;
                return;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"User/Search?query={query}");

            if (response.IsSuccessStatusCode)
            {
                List<User> searchResults = await response.Content.ReadAsAsync<List<User>>();

                searchResults = searchResults.Where(user => user.Username != _username).ToList();

                if (searchResults.Any())
                {
                    lst_searchresults.ItemsSource = searchResults;
                    lst_searchresults.Visibility = Visibility.Visible;
                    lbl_searchresults.Visibility = Visibility.Visible;
                }
                else
                {
                    lst_searchresults.Visibility = Visibility.Collapsed;
                    lbl_searchresults.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                MessageBox.Show("Failed to retrieve search results.");
            }
        }

        private async void lst_searchresults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            User selectedUser = (User)lst_searchresults.SelectedItem;

            if (selectedUser != null)
            {
                _targetUsername = selectedUser.Username;
                lbl_chatpartnerdisplay.Content = selectedUser.Username;
                lst_searchresults.Visibility = Visibility.Collapsed;
                lbl_searchresults.Visibility = Visibility.Collapsed;

                if (!lst_chats.Items.Contains(selectedUser.Username))
                {
                    lst_chats.Items.Add(selectedUser.Username);
                }

                lst_chats.SelectedItem = selectedUser.Username;
                lst_chats_SelectionChanged(lst_chats, null);
            }
        }
        private async void btn_sendmessage_Click(object sender, RoutedEventArgs e)
        {
            string messageText = txtbox_message.Text;

            if (string.IsNullOrEmpty(messageText))
            {
                MessageBox.Show("Please enter a message.");
                return;
            }

            try
            {
                Message newMessage = new Message
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Text = messageText,
                    Author = _username,
                    Timestamp = DateTime.Now
                };

                txtbox_message.Text = string.Empty;

                lst_chat.Items.Add(new TextBlock
                {
                    Inlines =
            {
                new Run(newMessage.Timestamp.ToShortTimeString())
                {
                    FontSize = 10,
                    Foreground = Brushes.Gray
                },
                new Run(" "),
                new Run($"{_username}: {messageText}")
            }
                });

                lst_chat.ScrollIntoView(lst_chat.Items[lst_chat.Items.Count - 1]);

                await _messageService.SendMessageAsync(_username, _targetUsername, newMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}");
            }
        }

        private void img_userlogo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (lbl_usernamedisplay.Visibility == Visibility.Visible)
            {
                lbl_usernamedisplay.Visibility = Visibility.Collapsed;
                btn_logout.Visibility = Visibility.Collapsed;
                btn_deleteaccount.Visibility = Visibility.Collapsed;
            }
            else
            {
                lbl_usernamedisplay.Visibility = Visibility.Visible;
                btn_logout.Visibility = Visibility.Visible;
                btn_deleteaccount.Visibility = Visibility.Visible;
            }
        }

        private void btn_logout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to logout, {_username}", "Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Logout confirmed!");

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private async void btn_deleteaccount_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete your account?", "Delete Account", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    HttpResponseMessage response = await _httpClient.DeleteAsync($"User/{_username}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Account deleted successfully!");
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete account.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting account: {ex.Message}");
                }
            }
        }

        private async void lst_chats_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (lst_chats.SelectedItem != null)
                {
                    try
                    {

                        string selectedUsername = lst_chats.SelectedItem as string;

                        Chat selectedChat = GetChatByUsername(selectedUsername);

                        if (selectedChat != null)
                        {
                            lst_chats.Items.Remove(selectedUsername);
                            lst_chat.Items.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Invalid selected chat.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting chat: {ex.Message}");
                    }
                }
            }
        }
        private Chat GetChatByUsername(string username)
        {
            try
            {
                foreach (Chat chat in _chatsList)
                {
                    if (chat.Participants.Contains(username))
                    {
                        return chat;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving chat: {ex.Message}");
                return null;
            }
        }


        private async void lst_chat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (lst_chat.SelectedItem != null)
                {
                    try
                    {
                        var selectedTextBlock = lst_chat.SelectedItem as TextBlock;
                        if (selectedTextBlock != null)
                        {
                            string selectedMessage = selectedTextBlock.Inlines.OfType<Run>().LastOrDefault()?.Text;

                            if (!string.IsNullOrEmpty(selectedMessage))
                            {
                                string[] parts = selectedMessage.Split(':');
                                string selectedUsername = parts[0].Trim();
                                string messageText = string.Join(":", parts.Skip(1)).Trim();

                                Console.WriteLine($"Selected Username: {selectedUsername}");
                                Console.WriteLine($"Message Text: {messageText}");

                                Chat selectedChat = GetChatByUsername(selectedUsername);

                                if (selectedChat != null)
                                {
                                    Console.WriteLine($"Chat Messages Count: {selectedChat.Messages.Count}");


                                    Message selectedChatMessage = selectedChat.Messages.FirstOrDefault(msg => msg.Text == messageText && msg.Author == selectedUsername);

                                    if (selectedChatMessage != null)
                                    {
                                        string messageId = selectedChatMessage.Id;

                                        await _messageService.DeleteMessageAsync(selectedChat.Id, messageId);

                                        lst_chat.Items.Remove(lst_chat.SelectedItem);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Selected message not found in chat.");
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Invalid selected chat.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting message: {ex.Message}");
                    }
                }
            }
        }


        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }


    }

}