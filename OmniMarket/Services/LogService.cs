using Microsoft.EntityFrameworkCore;
using OmniMarket.Data;
using OmniMarket.Models;

namespace OmniMarket.Services;

public class LogService
{
    public void AddLog(int marketId, string kullaniciAdi, LogType islemTipi, string detay)
    {
        using var db = new AppDbContext();
        var log = new Log
        {
            MarketId = marketId,
            KullaniciAdi = kullaniciAdi,
            IslemTipi = islemTipi,
            Detay = detay,
            Tarih = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) // Postgres requires UTC
        };

        db.Logs.Add(log);
        db.SaveChanges();
    }

    public async Task<List<Log>> GetLogsAsync(int marketId)
    {
        using var db = new AppDbContext();
        return await db.Logs
            .Where(l => l.MarketId == marketId)
            .OrderByDescending(l => l.Tarih)
            .ToListAsync();
    }
}
