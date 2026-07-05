# 🛒 OmniMarket Desktop App

OmniMarket, modern donanımlar ve verimli süreç yönetimi düşünülerek tasarlanmış kapsamlı bir **Market ve Stok Yönetim** masaüstü uygulamasıdır. İçerisindeki modüller yardımıyla raf ve depo yönetimini kolaylaştırır, zayiat oranlarını düşürür.

## 🚀 Öne Çıkan Özellikler

- **Gelişmiş Dashboard Paneli**: Marketin mevcut toplam stok değerini, Son Kullanma Tarihi (SKT) yaklaşan/geçen ürün durumunu, anlık ve net finansal özetleri tek ekrandan takip edin.
- **Ürün ve Stok Yönetimi (Gruplu Takip)**: Birden fazla farklı partide (farklı SKT ve fiyatlarda) gelen aynı marka ürünleri gruplayarak veya detayına inerek yönetin. Toplu silme ve düzenleme imkanı.
- **SKT Öncelikli Uyarı Modülü**: SKT'ye 7 günden az kalan ürünleri sarı uyarılarla, SKT'si geçenleri kırmızı uyarılarla liste şeklinde görüntüleyin.
- **Hızlı Fiyat & Kampanya Aksiyonları**: SKT'si yaklaşan ürünler için tek tıkla **İndirim Kampanyası** başlatabilir ya da tedarikçiye **İade Talebi** oluşturabilirsiniz.
- **Tedarikçi (Supplier) Yönetimi**: Hangi ürünün hangi tedarikçiden geldiğini kaydedebilir, tedarikçi performansını sistemde tutabilirsiniz.
- **Güvenli Loglama Sistemi (Audit Trails)**: Hangi ürünün ne zaman eklendiği, hangi fiyattan satıldığı veya ne zaman sistemden tam olarak silindiği gibi işlemler kaydedilir (Loglanır).

## 🛠️ Kullanılan Teknolojiler

- **Framework**: `.NET 9.0`
- **Arayüz (UI)**: WPF (Windows Presentation Foundation)
- **Mimari Tasarım**: MVVM (Model-View-ViewModel) (Topluluk standartlarına uygun, modüler ViewModel katmanları)
- **Veritabanı**: PostgreSQL (Entity Framework Core, Code-First yaklaşımı ve LINQ ile)

## 📂 Proje Yapısı

- `Models`: Veritabanı tablolarına karşılık gelen veri modelleri (User, Product, Supplier, Log vb.)
- `ViewModels`: Arayüz (WPF) bileşenlerinin ve iş mantıklarının birbirinden ayrıldığı katman.
- `Views`: Kullanıcının etkileşime girdiği ekranlar (.xaml dosyaları).
- `Services`: Veritabanına veri yazıp okuma işlemlerini gerçekleştiren merkezi iş servisleri (ProductService, vb.)
- `Data`: EF Core katmanı, AppDbContext ayarları.

## 🚀 Kurulum & Çalıştırma

1. Projeyi sisteminize indirin (clone).
2. Veritabanı gereksinimleri için PostgreSQL'in yüklü olduğundan emin olun.
3. `OmniMarket\Data\AppDbContext.cs` içindeki veritabanı bağlantı cümlenizi (Connection String) kendi local PostgreSQL sunucu bilgilerinize göre güncelleyin.
4. Entity Framework migration'larını veritabanına uygulamak için terminalde proje klasörü içindeyken (Örn: `OmniMarket\` klasöründe):
   ```bash
   dotnet ef database update
   ```
5. Projeyi .NET 9 ortamında derleyip başlatmak için Visual Studio'yu kullanın veya terminalde uygulamanın kök dizinine giderek:
   ```bash
   dotnet run --project OmniMarket\OmniMarket.csproj
   ```

## 🤝 Geliştirme Katkısı
Uygulama arka planında **Birim bazlı** listelemeler yerine, stok gruplamaları üzerinden mantıksal hesaplamalar devreye alınmıştır. Yeni geliştirmelerde `ProductService` içerisindeki veri çekme/gruplama kurallarının dikkate alınması tavsiye edilir.
