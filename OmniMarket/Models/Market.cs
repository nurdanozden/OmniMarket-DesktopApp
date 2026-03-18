using System.ComponentModel.DataAnnotations;

namespace OmniMarket.Models;

public class Market
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;

    // Navigation property: 1 Market → N Product
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
