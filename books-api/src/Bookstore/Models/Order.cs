using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Order
{
    [JsonPropertyName("books")]
    public List<BookOrder> Books { get; set; }

    [JsonPropertyName("deliveryAddress")]
    public string DeliveryAddress { get; set; }
}