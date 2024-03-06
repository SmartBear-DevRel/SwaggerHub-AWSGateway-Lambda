using System.Collections.Generic;
using System.Text.Json.Serialization;

public class OrderDetails 
{
    [JsonPropertyName("id")]
    public int id { get; set; }

    [JsonPropertyName("books")]
    public List<BookOrder> Books { get; set; }

    [JsonPropertyName("status")]
    public Status Status { get; set; }

    [JsonPropertyName("deliveryAddress")]
    public string DeliveryAddress { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; }
}

public enum Status
{
    Placed,
    Paid,
    Delivered
}
