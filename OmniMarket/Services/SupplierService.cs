using Microsoft.EntityFrameworkCore;
using OmniMarket.Data;
using OmniMarket.Models;

namespace OmniMarket.Services;

public class SupplierService
{
    public List<Tedarikci> GetSuppliers(int marketId)
    {
        using var db = new AppDbContext();
        return db.Tedarikciler
            .Where(s => s.MarketId == marketId)
            .OrderBy(s => s.Ad)
            .ToList();
    }

    public Tedarikci AddSupplier(Tedarikci supplier)
    {
        using var db = new AppDbContext();
        db.Tedarikciler.Add(supplier);
        db.SaveChanges();
        return supplier;
    }

    public void UpdateSupplier(Tedarikci supplier)
    {
        using var db = new AppDbContext();
        db.Tedarikciler.Update(supplier);
        db.SaveChanges();
    }

    public void DeleteSupplier(int supplierId)
    {
        using var db = new AppDbContext();
        var supplier = db.Tedarikciler.Find(supplierId);
        if (supplier != null)
        {
            db.Tedarikciler.Remove(supplier);
            db.SaveChanges();
        }
    }
}
