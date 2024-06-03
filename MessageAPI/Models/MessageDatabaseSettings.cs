namespace MessageAPI.Models
{
    public class MessageDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string MessagesCollectionName { get; set; } = null!;

        public string ChatsCollectionName { get; set; } = null!;

    }
}
