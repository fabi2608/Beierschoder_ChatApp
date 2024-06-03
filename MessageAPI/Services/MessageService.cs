using MessageAPI.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MessageAPI.Services
{
    public class MessageService
    {
        private readonly IMongoCollection<Message> _messagesCollection;
        public MessageService(
            IOptions<MessageDatabaseSettings> messageDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                messageDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                messageDatabaseSettings.Value.DatabaseName);

            _messagesCollection = mongoDatabase.GetCollection<Message>(
                messageDatabaseSettings.Value.MessagesCollectionName);
        }

        public async Task<List<Message>> GetAsync() =>
            await _messagesCollection.Find(_ => true).ToListAsync();

        public async Task<Message?> GetAsync(string id) =>
            await _messagesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Message newMessage) =>
            await _messagesCollection.InsertOneAsync(newMessage);

        public async Task UpdateAsync(string id, Message updatedMessage) =>
            await _messagesCollection.ReplaceOneAsync(x => x.Id == id, updatedMessage);

        public async Task RemoveAsync(string id) =>
            await _messagesCollection.DeleteOneAsync(x => x.Id == id);
    }
}
