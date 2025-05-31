using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Thrift.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
          [BsonElement("Email")]
        public string Email { get; set; }
        
        [BsonElement("UserName")]
        public string UserName { get; set; }
        
        [BsonElement("PasswordHash")]
        public string PasswordHash { get; set; }
        
        [BsonElement("FirstName")]
        public string FirstName { get; set; }
        
        [BsonElement("LastName")]
        public string LastName { get; set; }
        
        [BsonElement("PhoneNumber")]
        public string PhoneNumber { get; set; }
        
        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [BsonElement("LastLogin")]
        public DateTime? LastLogin { get; set; }
        
        [BsonElement("EmailConfirmed")]
        public bool EmailConfirmed { get; set; }
        
        [BsonElement("ProfilePictureUrl")]
        public string ProfilePictureUrl { get; set; }
        
        [BsonElement("PreferredCurrency")]
        public string PreferredCurrency { get; set; } = "USD";
        
        [BsonElement("DefaultTheme")]
        public string DefaultTheme { get; set; } = "light";
        
        [BsonElement("Roles")]
        public List<string> Roles { get; set; } = new List<string>();
        
        [BsonIgnore]
        public string FullName => $"{FirstName} {LastName}";
    }
}