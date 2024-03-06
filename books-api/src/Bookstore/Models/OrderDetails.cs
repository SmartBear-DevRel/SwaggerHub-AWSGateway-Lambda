using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;

public class OrderDetails 
{
    [JsonPropertyName("id")]
    public int id { get; set; }

    [JsonPropertyName("books")]
    public List<BookOrder> Books { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("deliveryAddress")]
    public string DeliveryAddress { get; set; }

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; set; }
}


public enum Status
{
    [EnumMember(Value = "placed")]
    Placed,
    [EnumMember(Value = "paid")]
    Paid,
    [EnumMember(Value = "delivered")]
    Delivered
}
