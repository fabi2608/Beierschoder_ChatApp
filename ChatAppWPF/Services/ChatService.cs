using Amazon.Runtime.Internal;
using MongoDB.Bson.IO;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChatAppWPF.Services
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7037/api/Chat";

        public ChatService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<List<Message>> GetChatHistoryAsync(string username1, string username2)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/History/{username1}/{username2}");

                response.EnsureSuccessStatusCode();
                var messages = await response.Content.ReadFromJsonAsync<List<Message>>();

                if (messages != null)
                {
                    foreach (var message in messages)
                    {
                        message.Timestamp = message.Timestamp.ToLocalTime();
                    }
                }
                return messages;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting chat history: {ex.Message}", ex);
            }
        }
        public async Task UpdateChatAsync(Chat chat)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{chat.Id}", chat);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating chat: {ex.Message}", ex);
            }
        }
        public async Task<Chat> GetOrCreateChatAsync(string username, string targetUsername)
        {
            Chat existingChat = null;
            try
            {
                existingChat = await _httpClient.GetFromJsonAsync<Chat>($"{BaseUrl}/Participants/{username}/{targetUsername}");
                return existingChat;
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    existingChat = new Chat
                    {
                        Participants = new List<string> { username, targetUsername },
                        Messages = new List<Message>()
                    };

                    await SendChatAsync(existingChat);

                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting or creating chat: {ex.Message}", ex);
            }

            return existingChat;
        }
        public async Task<bool> SendChatAsync(Chat chat)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(BaseUrl, chat);
                response.EnsureSuccessStatusCode();
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending chat: {ex.Message}");
                return false;
            }
        }
        public async Task<List<Chat>> GetChatsForUser(string username)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{BaseUrl}/User/{username}");

                if (response.IsSuccessStatusCode)
                {
                    var chats = await response.Content.ReadFromJsonAsync<List<Chat>>();
                    return chats;
                }
                else
                {
                    throw new Exception("Failed to retrieve chats for user.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting chats for user: {ex.Message}", ex);
            }
        }
    }
}
