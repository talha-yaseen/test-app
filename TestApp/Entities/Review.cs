using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TestApp.Entities;

public class Review
{
    [BsonId]
    public BsonValue Id { get; set; }

    [BsonElement("stars")]
    public double Stars { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("description")]
    public string Description { get; set; }

    [BsonElement("location")]
    public string Location { get; set; }

    [BsonElement("writtenBy")]
    public string WrittenBy { get; set; }

    [BsonElement("personImage")]
    public string PersonImage { get; set; }
}

