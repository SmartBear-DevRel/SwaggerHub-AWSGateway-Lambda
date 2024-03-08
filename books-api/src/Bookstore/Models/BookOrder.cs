using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class BookOrder
{
    [JsonPropertyName("bookId")]
    [Required]
    public Guid BookId { get; set; }
    
    [JsonPropertyName("quantity")]
    [Required]
    public int Quantity { get; set; }
}