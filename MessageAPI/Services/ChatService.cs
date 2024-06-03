using MongoDB.Driver;
using MessageAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using System.Net.Http;
using System.Linq.Expressions;

namespace MessageAPI.Services
{
    public class ChatService
    {
        private readonly IMongoCollection<Chat> _chatsCollection;

        public ChatService(
            IOptions<MessageDatabaseSettings> messageDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                messageDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                messageDatabaseSettings.Value.DatabaseName);

            _chatsCollection = mongoDatabase.GetCollection<Chat>(
                messageDatabaseSettings.Value.ChatsCollectionName);
        }
        public async Task<List<Chat>> GetAsync()
        {
            return await _chatsCollection.Find(chat => true).ToListAsync();
        }

        public async Task<Chat> GetAsync(string id)
        {
            return await _chatsCollection.Find<Chat>(chat => chat.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Chat> CreateAsync(Chat chat)
        {
            await _chatsCollection.InsertOneAsync(chat);
            return chat;
        }

        public async Task<Chat> GetChatWithParticipantsAsync(string userId1, string userId2)
        {
            try
            {
                var filter = Builders<Chat>.Filter.And(
                    Builders<Chat>.Filter.Eq("Participants", userId1),
                    Builders<Chat>.Filter.Eq("Participants", userId2)
                );

                return await _chatsCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting chat with participants: {ex.Message}", ex);
            }
        }

        public async Task<List<Chat>> GetChatsForUser(string username)
        {
            try
            {
                var filter = Builders<Chat>.Filter.AnyEq(chat => chat.Participants, username);

                var chats = await _chatsCollection.Find(filter).ToListAsync();

                return chats;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting chats for user: {ex.Message}", ex);
            }
        }

        public async Task<List<Message>> GetChatHistoryAsync(string username1, string username2)
        {
            try
            {
                var filter = Builders<Chat>.Filter.And(
                    Builders<Chat>.Filter.Eq("Participants", username1),
                    Builders<Chat>.Filter.Eq("Participants", username2)
                );

                var chat = await _chatsCollection.Find(filter).FirstOrDefaultAsync();

                if (chat != null)
                {
                    return chat.Messages;
                }
                else
                {
                    return new List<Message>();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting chat history: {ex.Message}", ex);
            }
        }
        public async Task UpdateChatAsync(string id, Chat updatedChat)
        {
            try
            {
                var filter = Builders<Chat>.Filter.Eq("_id", ObjectId.Parse(id));
                var update = Builders<Chat>.Update
                    .Set("Participants", updatedChat.Participants)
                    .Set("Messages", updatedChat.Messages);

                var result = await _chatsCollection.UpdateOneAsync(filter, update);
                if (result.MatchedCount == 0)
                {
                    throw new Exception("No matching document found to update.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error updating chat with id {id}: {ex}");
                throw new Exception($"Error updating chat: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteMessageAsync(string chatId, string messageId)
        {
            try
            {
                var filter = Builders<Chat>.Filter.Eq("Id", ObjectId.Parse(chatId));
                var chat = await _chatsCollection.Find(filter).FirstOrDefaultAsync();

                if (chat != null)
                {
                    var messageToRemove = chat.Messages.FirstOrDefault(m => m.Id.ToString() == messageId);
                    if (messageToRemove != null)
                    {
                        chat.Messages.Remove(messageToRemove);
                        var update = Builders<Chat>.Update.Set("Messages", chat.Messages);
                        var result = await _chatsCollection.UpdateOneAsync(filter, update);
                        return result.ModifiedCount > 0;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting message: {ex.Message}", ex);
            }
        }
    }
}
