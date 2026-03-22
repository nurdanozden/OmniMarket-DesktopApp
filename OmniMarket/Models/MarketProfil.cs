using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OmniMarket.Models;

public class MarketProfil
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string MarketAdi { get; set; } = "NUR MARKET";

    [MaxLength(500)]
    public string Adres { get; set; } = string.Empty;

    [MaxLength(500)]
    public string LogoPath { get; set; } = string.Empty;

    [MaxLength(20)]
    public string TemaRengi { get; set; } = "#3B82F6";

    public int MarketId { get; set; }

    [ForeignKey("MarketId")]
    public Market? Market { get; set; }
}
