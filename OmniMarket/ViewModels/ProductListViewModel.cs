using System.Collections.ObjectModel;
using System.Windows;
using OmniMarket.Helpers;
using OmniMarket.Models;
using OmniMarket.Services;

namespace OmniMarket.ViewModels;

public class ProductListViewModel : BaseViewModel
{
    private readonly ProductService _productService = new();
    private static readonly Random _rng = new();
    private int _marketId;
    private string _marketName = string.Empty;
    private string? _initialFilter;

    private ObservableCollection<Product> _products = new();
    private Product? _selectedProduct;
    private string _searchText = string.Empty;
    private string _selectedCategory = string.Empty;
    private List<string> _categories = new();

    // Aktif filtre bilgisi (UI'da göstermek için)
    private string _activeFilterLabel = string.Empty;
    public string ActiveFilterLabel
    {
        get => _activeFilterLabel;
        set => SetProperty(ref _activeFilterLabel, value);
    }

    // Sabit kategori listesi (form için)
    public List<string> FormCategories { get; } = new()
    {
        "Un & Bakliyat",
        "Süt & Süt Ürünleri",
        "İçecekler",
        "Atıştırmalıklar",
        "Temizlik Ürünleri",
        "Dondurulmuş Gıdalar"
    };

    // Form fields
    private string _formName = string.Empty;
    private string _formCategory = string.Empty;
    private string _formBarcode = string.Empty;
    private int _formStock;
    private decimal _formPurchasePrice;
    private decimal _formSalePrice;
    private DateTime _formExpiryDate = DateTime.Today.AddMonths(6);
    private bool _isFormVisible;
    private bool _isEditing;
    private int _editingProductId;

    #region Properties

    public ObservableCollection<Product> Products
    {
        get => _products;
        set => SetProperty(ref _products, value);
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set => SetProperty(ref _selectedProduct, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ExecuteSearch();
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
                ExecuteFilter();
        }
    }

    public List<string> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    // Form properties
    public string FormName     { get => _formName;     set => SetProperty(ref _formName, value); }
    public string FormCategory { get => _formCategory; set => SetProperty(ref _formCategory, value); }
    public string FormBarcode  { get => _formBarcode;  set => SetProperty(ref _formBarcode, value); }
    public int FormStock       { get => _formStock;    set => SetProperty(ref _formStock, value); }
    public decimal FormPurchasePrice { get => _formPurchasePrice; set => SetProperty(ref _formPurchasePrice, value); }
    public decimal FormSalePrice     { get => _formSalePrice;     set => SetProperty(ref _formSalePrice, value); }
    public DateTime FormExpiryDate   { get => _formExpiryDate;    set => SetProperty(ref _formExpiryDate, value); }

    public bool IsFormVisible
    {
        get => _isFormVisible;
        set => SetProperty(ref _isFormVisible, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }

    public string FormTitle => IsEditing ? "Ürün Düzenle" : "Yeni Ürün Ekle";

    #endregion

    #region Commands

    public RelayCommand AddProductCommand { get; }
    public RelayCommand EditProductCommand { get; }
    public RelayCommand DeleteProductCommand { get; }
    public RelayCommand SaveProductCommand { get; }
    public RelayCommand CancelFormCommand { get; }
    public RelayCommand RefreshCommand { get; }
    public RelayCommand ClearFilterCommand { get; }
    public RelayCommand GenerateBarcodeCommand { get; }
    public RelayCommand CallSupplierCommand { get; }

    #endregion

    public ProductListViewModel()
    {
        AddProductCommand = new RelayCommand(ShowAddForm);
        EditProductCommand = new RelayCommand(ShowEditForm);
        DeleteProductCommand = new RelayCommand(ExecuteDelete);
        SaveProductCommand = new RelayCommand(ExecuteSave);
        CancelFormCommand = new RelayCommand(HideForm);
        RefreshCommand = new RelayCommand(LoadProducts);
        ClearFilterCommand = new RelayCommand(ClearFilter);
        GenerateBarcodeCommand = new RelayCommand(GenerateBarcode);
        CallSupplierCommand = new RelayCommand(ExecuteCallSupplier);
    }

    public void Initialize(int marketId, string marketName, string? filter = null)
    {
        _marketId = marketId;
        _marketName = marketName;
        _initialFilter = filter;
        LoadProducts();
        ApplyInitialFilter();
    }

    private void LoadProducts()
    {
        var products = _productService.GetProducts(_marketId);
        Products = new ObservableCollection<Product>(products);

        var categories = _productService.GetCategories(_marketId);
        categories.Insert(0, "Tüm Kategoriler");
        Categories = categories;
    }

    private void ApplyInitialFilter()
    {
        if (string.IsNullOrEmpty(_initialFilter))
        {
            ActiveFilterLabel = string.Empty;
            return;
        }

        var allProducts = _productService.GetProducts(_marketId);
        List<Product> filtered;

        switch (_initialFilter)
        {
            case "expired":
                filtered = allProducts.Where(p => DateTime.Today > p.ExpiryDate).ToList();
                ActiveFilterLabel = "🔴 Filtre: SKT Geçmiş Ürünler";
                break;
            case "expiring":
                filtered = allProducts.Where(p => DateTime.Today <= p.ExpiryDate && (p.ExpiryDate - DateTime.Today).TotalDays <= 7).ToList();
                ActiveFilterLabel = "🟡 Filtre: SKT Yaklaşan Ürünler";
                break;
            case "all":
                filtered = allProducts;
                ActiveFilterLabel = "📦 Filtre: Tüm Ürünler";
                break;
            case "stockvalue":
                filtered = allProducts.OrderByDescending(p => p.SalePrice * p.Stock).ToList();
                ActiveFilterLabel = "💰 Filtre: Stok Değerine Göre";
                break;
            default:
                filtered = allProducts;
                ActiveFilterLabel = string.Empty;
                break;
        }

        Products = new ObservableCollection<Product>(filtered);
        _initialFilter = null; // Tek seferlik filtre
    }

    private void ExecuteSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            LoadProducts();
            return;
        }

        var results = _productService.SearchProducts(_marketId, SearchText);
        Products = new ObservableCollection<Product>(results);
    }

    private void ExecuteFilter()
    {
        if (string.IsNullOrWhiteSpace(SelectedCategory) || SelectedCategory == "Tüm Kategoriler")
        {
            LoadProducts();
            return;
        }

        var results = _productService.FilterByCategory(_marketId, SelectedCategory);
        Products = new ObservableCollection<Product>(results);
    }

    private void ClearFilter()
    {
        SearchText = string.Empty;
        SelectedCategory = "Tüm Kategoriler";
        LoadProducts();
    }

    private void GenerateBarcode()
    {
        string barcode;
        do
        {
            barcode = $"869{_rng.Next(10000000, 99999999)}";
        }
        while (_productService.BarcodeExists(_marketId, barcode));

        FormBarcode = barcode;
    }

    private void ShowAddForm()
    {
        IsEditing = false;
        ClearForm();
        GenerateBarcode(); // Otomatik barkod oluştur
        IsFormVisible = true;
        OnPropertyChanged(nameof(FormTitle));
    }

    private void ShowEditForm()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Lütfen düzenlemek istediğiniz ürünü seçin.", "Uyarı",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsEditing = true;
        _editingProductId = SelectedProduct.Id;
        FormName = SelectedProduct.Name;
        FormCategory = SelectedProduct.Category;
        FormBarcode = SelectedProduct.Barcode;
        FormStock = SelectedProduct.Stock;
        FormPurchasePrice = SelectedProduct.PurchasePrice;
        FormSalePrice = SelectedProduct.SalePrice;
        FormExpiryDate = SelectedProduct.ExpiryDate;
        IsFormVisible = true;
        OnPropertyChanged(nameof(FormTitle));
    }

    private void ExecuteDelete()
    {
        if (SelectedProduct == null)
        {
            MessageBox.Show("Lütfen silmek istediğiniz ürünü seçin.", "Uyarı",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"\"{SelectedProduct.Name}\" ürününü silmek istediğinize emin misiniz?",
            "Silme Onayı",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            _productService.DeleteProduct(SelectedProduct.Id, _marketName);
            LoadProducts();
        }
    }

    private void ExecuteSave()
    {
        // ─── Validation ───
        if (string.IsNullOrWhiteSpace(FormName))
        {
            MessageBox.Show("Ürün adı boş bırakılamaz.", "Doğrulama Hatası",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(FormCategory))
        {
            MessageBox.Show("Lütfen bir kategori seçin.", "Doğrulama Hatası",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (FormPurchasePrice < 0)
        {
            MessageBox.Show("Alış fiyatı negatif olamaz.", "Doğrulama Hatası",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (FormSalePrice < 0)
        {
            MessageBox.Show("Satış fiyatı negatif olamaz.", "Doğrulama Hatası",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (FormStock < 0)
        {
            MessageBox.Show("Stok negatif olamaz.", "Doğrulama Hatası",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (FormExpiryDate == default)
        {
            MessageBox.Show("Son kullanma tarihi boş olamaz.", "Doğrulama Hatası",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (IsEditing)
        {
            var product = new Product
            {
                Id = _editingProductId,
                Name = FormName,
                Category = FormCategory,
                Barcode = FormBarcode,
                Stock = FormStock,
                PurchasePrice = FormPurchasePrice,
                SalePrice = FormSalePrice,
                ExpiryDate = DateTime.SpecifyKind(FormExpiryDate, DateTimeKind.Utc),
                MarketId = _marketId
            };
            _productService.UpdateProduct(product, _marketName);
        }
        else
        {
            var product = new Product
            {
                Name = FormName,
                Category = FormCategory,
                Barcode = FormBarcode,
                Stock = FormStock,
                PurchasePrice = FormPurchasePrice,
                SalePrice = FormSalePrice,
                ExpiryDate = DateTime.SpecifyKind(FormExpiryDate, DateTimeKind.Utc),
                MarketId = _marketId
            };
            _productService.AddProduct(product, _marketName);
        }

        HideForm();
        LoadProducts();
    }

    private void HideForm()
    {
        IsFormVisible = false;
        ClearForm();
    }

    private void ClearForm()
    {
        FormName = string.Empty;
        FormCategory = string.Empty;
        FormBarcode = string.Empty;
        FormStock = 0;
        FormPurchasePrice = 0;
        FormSalePrice = 0;
        FormExpiryDate = DateTime.Today.AddMonths(6);
    }

    private void ExecuteCallSupplier(object? parameter)
    {
        if (parameter is Product product && product.Tedarikci != null)
        {
            MessageBox.Show($"Tedarikçiyi Ara: {product.Tedarikci.IletisimNo}",
                $"{product.Tedarikci.Ad}", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
