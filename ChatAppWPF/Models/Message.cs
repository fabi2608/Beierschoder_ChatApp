using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatAppWPF
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Text { get; set; }
        public string Author { get; set; }
        public DateTime Timestamp {  get; set; }

        public Message()
        {
            Timestamp = DateTime.Now;
        }
    }
}
