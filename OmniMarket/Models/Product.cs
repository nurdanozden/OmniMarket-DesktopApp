using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniMarket.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Barcode { get; set; } = string.Empty;

    public int Stock { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PurchasePrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SalePrice { get; set; }

    public DateTime ExpiryDate { get; set; }

    // Foreign Key
    public int MarketId { get; set; }

    // Navigation property
    [ForeignKey("MarketId")]
    public Market? Market { get; set; }

    // Computed: SKT durumu
    [NotMapped]
    public string ExpiryStatus
    {
        get
        {
            if (DateTime.Today > ExpiryDate)
                return "Expired";    // Kırmızı
            else if ((ExpiryDate - DateTime.Today).TotalDays <= 7)
                return "Warning";    // Sarı
            else
                return "Normal";     // Yeşil
        }
    }

    // Computed: Kar hesaplama
    [NotMapped]
    public decimal Profit => (SalePrice - PurchasePrice) * Stock;

    // Computed: Stok durumu
    [NotMapped]
    public bool IsLowStock => Stock < 5;
}
