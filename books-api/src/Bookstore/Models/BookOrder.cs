using System.Text.Json.Serialization;

public class BookOrder
{
    [JsonPropertyName("bookId")]
    public int BookId { get; set; }
    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}