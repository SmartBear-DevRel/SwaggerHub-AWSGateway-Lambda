using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class Order
{
    [JsonPropertyName("books")]
    [Required]
    public List<BookOrder> Books { get; set; }
    
    [JsonPropertyName("deliveryAddress")]
    [Required]
    [MinLength(10, ErrorMessage = "Delivery address must be at least 10 characters long")]
    [MaxLength(500, ErrorMessage = "Delivery address must be at most 500 characters long")]
    public string DeliveryAddress { get; set; }
}