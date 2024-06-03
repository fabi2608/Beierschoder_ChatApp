using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MessageAPI.Models
{
    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Participants")]

        public List<string> Participants { get; set; }

        [BsonElement("Messages")]

        public List<Message> Messages { get; set; }

    }
}
