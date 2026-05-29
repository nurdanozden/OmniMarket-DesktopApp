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

    public int MarketId { get; set; }

    public int? TedarikciId { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DiscountRate { get; set; }

    public bool ReturnRequested { get; set; }

    [ForeignKey("MarketId")]
    public Market? Market { get; set; }

    [ForeignKey("TedarikciId")]
    public virtual Tedarikci? Tedarikci { get; set; }

    [NotMapped]
    public string ExpiryStatus
    {
        get
        {
            if (DateTime.Today > ExpiryDate)
                return "Expired";
            else if ((ExpiryDate - DateTime.Today).TotalDays <= 7)
                return "Warning";
            else
                return "Normal";
        }
    }

    [NotMapped]
    public decimal Profit => SalePrice - PurchasePrice;

    [NotMapped]
    public bool IsLowStock => Stock < 5;

    [NotMapped]
    public bool IsSelected { get; set; }
}

