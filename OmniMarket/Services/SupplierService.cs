using Microsoft.EntityFrameworkCore;
using System;
using System.Windows;
using OmniMarket.Data;
using OmniMarket.Models;

namespace OmniMarket.Services;

public class SupplierService
{
    public List<Tedarikci> GetSuppliers(int marketId)
    {
        try
        {
            using var db = new AppDbContext();
            return db.Tedarikciler
                .Where(s => s.MarketId == marketId)
                .OrderBy(s => s.Ad)
                .ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tedarikçiler yüklenirken hata oluştu: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            return new List<Tedarikci>();
        }
    }

    public Tedarikci AddSupplier(Tedarikci supplier)
    {
        try
        {
            using var db = new AppDbContext();
            db.Tedarikciler.Add(supplier);
            db.SaveChanges();
            return supplier;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tedarikçi eklenirken hata oluştu: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            return supplier;
        }
    }

    public void UpdateSupplier(Tedarikci supplier)
    {
        try
        {
            using var db = new AppDbContext();
            db.Tedarikciler.Update(supplier);
            db.SaveChanges();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tedarikçi güncellenirken hata oluştu: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void DeleteSupplier(int supplierId)
    {
        try
        {
            using var db = new AppDbContext();
            var supplier = db.Tedarikciler.Find(supplierId);
            if (supplier != null)
            {
                db.Tedarikciler.Remove(supplier);
                db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Tedarikçi silinirken hata oluştu: {ex.Message}", "Veritabanı Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
