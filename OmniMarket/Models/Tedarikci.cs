using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniMarket.Models;

public class Tedarikci
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Ad { get; set; } = string.Empty;

    [MaxLength(30)]
    public string IletisimNo { get; set; } = string.Empty;

    [MaxLength(100)]
    public string TeslimatGunleri { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Kategori { get; set; } = string.Empty;

    public int MarketId { get; set; }

    [ForeignKey("MarketId")]
    public Market? Market { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
