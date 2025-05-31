using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TestApp.Entities;

[BsonIgnoreExtraElements]
public class Movie
{
    [BsonElement("plot")]
    public string? Plot { get; set; }

    [BsonElement("poster")]
    public string? Poster { get; set; }

    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("fullplot")]
    public string? FullPlot { get; set; }
}

