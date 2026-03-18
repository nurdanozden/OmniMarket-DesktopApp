using Microsoft.EntityFrameworkCore;
using OmniMarket.Data;
using OmniMarket.Models;

namespace OmniMarket.Services;

public class AuthService
{
    /// <summary>
    /// Kullanıcı girişi. Başarılıysa Market döndürür, değilse null.
    /// </summary>
    public Market? Login(string username, string password)
    {
        using var db = new AppDbContext();
        return db.Markets.FirstOrDefault(m =>
            m.Username == username && m.Password == password);
    }

    /// <summary>
    /// Yeni market kaydı oluşturur. Başarılıysa Market döndürür.
    /// Username zaten varsa null döndürür.
    /// </summary>
    public Market? Register(string marketName, string username, string password)
    {
        using var db = new AppDbContext();

        // Kullanıcı adı kontrolü
        if (db.Markets.Any(m => m.Username == username))
            return null;

        var market = new Market
        {
            Name = marketName,
            Username = username,
            Password = password
        };

        db.Markets.Add(market);
        db.SaveChanges();
        return market;
    }
}
