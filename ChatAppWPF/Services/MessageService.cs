using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChatAppWPF.Services
{
    public class MessageService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7037/api/Chat";
        private ChatService _chatService;
        public MessageService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);

            _chatService = new ChatService();
        }

        public async Task SendMessageAsync(string username, string targetUsername, Message message)
        {
            try
            {
                message.Timestamp = DateTime.UtcNow;
                Chat existingChat = await _chatService.GetOrCreateChatAsync(username, targetUsername);
                existingChat.Messages.Add(message);

                await _chatService.UpdateChatAsync(existingChat);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error sending message: {ex.Message}", ex);
            }
        }
        public async Task DeleteMessageAsync(string chatId, string messageId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{chatId}/Messages/{messageId}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting message: {ex.Message}", ex);
            }
        }
    }
}
