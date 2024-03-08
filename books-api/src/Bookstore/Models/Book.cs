using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Book
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("authors")]
    public List<string> Authors { get; set; }
    [JsonPropertyName("published")]
    public string Published { get; set; }
}