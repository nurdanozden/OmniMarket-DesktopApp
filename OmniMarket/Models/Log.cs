using System.ComponentModel.DataAnnotations;

namespace OmniMarket.Models;

public enum LogType
{
    Ekleme,
    Silme,
    Guncelleme,
    Login,
    StokCikisi,
    FiyatGuncelleme,
    UrunIade,
    Kampanya
}

public class Log
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public int MarketId { get; set; }

    [Required]
    [MaxLength(100)]
    public string KullaniciAdi { get; set; } = string.Empty;

    [Required]
    public LogType IslemTipi { get; set; }

    [Required]
    [MaxLength(500)]
    public string Detay { get; set; } = string.Empty;

    // Audit: değişiklik öncesi ve sonrası değerler
    [MaxLength(500)]
    public string? EskiDeger { get; set; }

    [MaxLength(500)]
    public string? YeniDeger { get; set; }

    [Required]
    public DateTime Tarih { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public Market? Market { get; set; }
}
