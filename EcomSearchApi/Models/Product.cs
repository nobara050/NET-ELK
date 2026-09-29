using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcomSearchApi.Models;

[Table("Products")]
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Column(TypeName = "numeric(12,2)")]
    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string[] Tags { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
