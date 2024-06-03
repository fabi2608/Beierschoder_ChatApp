using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ThirdParty.Json.LitJson;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
namespace MessageAPI.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("Text")]
        public string Text { get; set; } = null!;
        public string Author { get; set; } = null!;

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]

        public DateTime TimeStamp { get; set; }
    }
}
