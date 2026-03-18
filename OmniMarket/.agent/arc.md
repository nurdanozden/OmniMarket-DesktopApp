# OmniMarket – Akıllı Market Depo Yönetim Sistemi

## 📌 Proje Tanımı

OmniMarket, birden fazla marketin kendi depo/stok süreçlerini yönetebildiği, son kullanma tarihi (SKT) takibi yapan, kritik durumlarda görsel uyarılar üreten ve modern kullanıcı arayüzüne sahip bir masaüstü uygulamasıdır.

Proje, WPF ve MVVM mimarisi kullanılarak geliştirilmiştir ve veri yönetimi için PostgreSQL tercih edilmiştir.

---

## 🎯 Amaç

* Market depolarındaki ürünleri merkezi bir sistemde yönetmek
* Son kullanma tarihlerini takip etmek
* Kritik durumları (bozulma, az stok) kullanıcıya görsel olarak bildirmek
* Çoklu market desteği sunmak
* Modern UI/UX ile kullanıcı deneyimini artırmak

---

## 🧱 Kullanılan Teknolojiler

| Teknoloji             | Açıklama                   |
| --------------------- | -------------------------- |
| WPF                   | Masaüstü arayüz geliştirme |
| MVVM                  | Katmanlı mimari            |
| Entity Framework Core | ORM                        |
| PostgreSQL            | Veritabanı                 |
| Npgsql                | PostgreSQL .NET provider   |
| XAML                  | UI tasarımı                |
| C#                    | Uygulama dili              |

---

## 🏗️ Mimari Yapı (MVVM)

### 📁 Proje Klasör Yapısı

```
OmniMarket/
│
├── Models/
├── ViewModels/
├── Views/
├── Services/
├── Data/
├── Helpers/
└── Resources/
```

---

### 🧩 Katman Açıklamaları

#### Models

Veritabanı tablolarını temsil eder.

#### ViewModels

UI ile iş mantığı arasındaki bağlantıyı sağlar.

#### Views

XAML tabanlı kullanıcı arayüzleri.

#### Services

İş mantığı ve veri işlemleri.

#### Data

DbContext ve veritabanı bağlantısı.

---

## 🗄️ Veritabanı Tasarımı (PostgreSQL)

### Market Tablosu

| Alan     | Tip      |
| -------- | -------- |
| Id       | int (PK) |
| Name     | varchar  |
| Username | varchar  |
| Password | varchar  |

---

### Product Tablosu

| Alan          | Tip       |
| ------------- | --------- |
| Id            | int (PK)  |
| Name          | varchar   |
| Category      | varchar   |
| Barcode       | varchar   |
| Stock         | int       |
| PurchasePrice | decimal   |
| SalePrice     | decimal   |
| ExpiryDate    | timestamp |
| MarketId      | int (FK)  |

---

### İlişki

* 1 Market → N Product

---

## 🔐 Kimlik Doğrulama

* Kullanıcı giriş sistemi
* Her kullanıcı = 1 market
* Veriler market bazlı izole edilir

---

## 🖥️ Ekranlar

### 1. Login / Register

* Kullanıcı adı / şifre
* Market oluşturma
* Authentication işlemleri

---

### 2. Dashboard

#### Özellikler:

* Toplam ürün sayısı
* SKT yaklaşan ürün sayısı
* SKT geçmiş ürün sayısı
* Toplam stok değeri

---

### 3. Ürün Listesi (DataGrid)

Kolonlar:

* Ürün Adı
* Kategori
* Stok
* Fiyatlar
* SKT
* Durum

---

## 🎨 Görsel Durum Sistemi (Critical Feature)

### Renk Kodları

| Durum      | Açıklama    |
| ---------- | ----------- |
| 🔴 Kırmızı | SKT geçmiş  |
| 🟡 Sarı    | SKT ≤ 7 gün |
| 🟢 Yeşil   | Normal      |

---

### Teknik Uygulama

* WPF DataGrid
* DataTrigger kullanımı
* Binding ile dinamik renk değişimi

---

## ⚙️ İş Kuralları

### SKT Kontrolü

```
if (Today > ExpiryDate) → Kırmızı
else if (ExpiryDate - Today <= 7) → Sarı
else → Yeşil
```

---

### Stok Uyarısı

```
if (Stock < 5) → Kritik uyarı
```

---

### Kar Hesaplama

```
Profit = (SalePrice - PurchasePrice) * Stock
```

---

## ➕ Ürün Yönetimi

### Ürün Ekleme

* Form üzerinden veri girişi
* Validation kontrolleri

### Ürün Güncelleme

* Seçili ürün düzenlenir

### Ürün Silme

* Onay dialog ile silme

---

## 🔍 Arama ve Filtreleme

* Ürün adına göre arama
* Barkod ile hızlı erişim
* Kategori filtreleme

---

## 🚨 Akıllı Uyarı Sistemi

Dinamik panel:

* “Ürün bitmek üzere”
* “SKT yaklaşan ürün”
* “SKT geçmiş ürün”

---

## 🎨 UI / UX Tasarım

### Tema

* Modern market arayüzü
* Minimalist ve sade tasarım

### Özellikler

* Rounded components
* Shadow efektleri
* Hover animasyonları
* Responsive grid yapı

---

## 🌙 Ek Özellikler (Opsiyonel)

* Dark Mode
* Grafiksel raporlar
* Excel export
* Toast notification sistemi

---

## 🔌 PostgreSQL Bağlantı

### NuGet Paketleri

```
Npgsql
Npgsql.EntityFrameworkCore.PostgreSQL
Microsoft.EntityFrameworkCore.Tools
```

---

### Connection String

```
Host=localhost;Port=5432;Database=OmniMarketDb;Username=postgres;Password=1234
```

---

## 🧪 Test Senaryoları

* Ürün ekleme/silme
* SKT değişimi → renk değişimi
* Farklı market login
* Stok uyarıları

---

## 🚀 Proje Değeri

Bu proje:

* Basit CRUD uygulaması değildir
* Çok kullanıcılı yapı içerir
* MVVM mimarisi kullanır
* Gerçek dünya senaryosuna uygundur
* Görsel olarak zengin ve dinamiktir

---

## 🎓 Sonuç

OmniMarket, modern yazılım geliştirme tekniklerini içeren, kullanıcı deneyimini ön planda tutan ve akademik olarak güçlü bir masaüstü uygulamasıdır.

---
