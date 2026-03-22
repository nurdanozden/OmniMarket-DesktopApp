using System.Collections.ObjectModel;
using System.Windows;
using OmniMarket.Helpers;
using OmniMarket.Models;
using OmniMarket.Services;

namespace OmniMarket.ViewModels;

public class SupplierListViewModel : BaseViewModel
{
    private readonly SupplierService _supplierService = new();
    private int _marketId;

    private ObservableCollection<Tedarikci> _suppliers = new();
    private Tedarikci? _selectedSupplier;

    private string _formAd = string.Empty;
    private string _formIletisimNo = string.Empty;
    private string _formTeslimatGunleri = string.Empty;
    private string _formKategori = string.Empty;
    private bool _isFormVisible;
    private bool _isEditing;
    private int _editingSupplierId;

    public ObservableCollection<Tedarikci> Suppliers
    {
        get => _suppliers;
        set => SetProperty(ref _suppliers, value);
    }

    public Tedarikci? SelectedSupplier
    {
        get => _selectedSupplier;
        set => SetProperty(ref _selectedSupplier, value);
    }

    public string FormAd { get => _formAd; set => SetProperty(ref _formAd, value); }
    public string FormIletisimNo { get => _formIletisimNo; set => SetProperty(ref _formIletisimNo, value); }
    public string FormTeslimatGunleri { get => _formTeslimatGunleri; set => SetProperty(ref _formTeslimatGunleri, value); }
    public string FormKategori { get => _formKategori; set => SetProperty(ref _formKategori, value); }

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

    public string FormTitle => IsEditing ? "Tedarik�i D�zenle" : "Yeni Tedarik�i";

    public RelayCommand AddSupplierCommand { get; }
    public RelayCommand EditSupplierCommand { get; }
    public RelayCommand DeleteSupplierCommand { get; }
    public RelayCommand SaveSupplierCommand { get; }
    public RelayCommand CancelFormCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public SupplierListViewModel()
    {
        AddSupplierCommand = new RelayCommand(ShowAddForm);
        EditSupplierCommand = new RelayCommand(ShowEditForm);
        DeleteSupplierCommand = new RelayCommand(ExecuteDelete);
        SaveSupplierCommand = new RelayCommand(ExecuteSave);
        CancelFormCommand = new RelayCommand(HideForm);
        RefreshCommand = new RelayCommand(LoadSuppliers);
    }

    public void Initialize(int marketId)
    {
        _marketId = marketId;
        LoadSuppliers();
    }

    private void LoadSuppliers()
    {
        Suppliers = new ObservableCollection<Tedarikci>(_supplierService.GetSuppliers(_marketId));
    }

    private void ShowAddForm()
    {
        IsEditing = false;
        ClearForm();
        IsFormVisible = true;
        OnPropertyChanged(nameof(FormTitle));
    }

    private void ShowEditForm()
    {
        if (SelectedSupplier == null)
        {
            MessageBox.Show("L�tfen d�zenlemek istedi�iniz tedarik�iyi se�in.", "Uyar�",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsEditing = true;
        _editingSupplierId = SelectedSupplier.Id;
        FormAd = SelectedSupplier.Ad;
        FormIletisimNo = SelectedSupplier.IletisimNo;
        FormTeslimatGunleri = SelectedSupplier.TeslimatGunleri;
        FormKategori = SelectedSupplier.Kategori;
        IsFormVisible = true;
        OnPropertyChanged(nameof(FormTitle));
    }

    private void ExecuteSave()
    {
        if (string.IsNullOrWhiteSpace(FormAd))
        {
            MessageBox.Show("L�tfen tedarik�i ad� girin.", "Eksik Bilgi",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (IsEditing)
        {
            var supplier = new Tedarikci
            {
                Id = _editingSupplierId,
                MarketId = _marketId,
                Ad = FormAd,
                IletisimNo = FormIletisimNo,
                TeslimatGunleri = FormTeslimatGunleri,
                Kategori = FormKategori
            };
            _supplierService.UpdateSupplier(supplier);
        }
        else
        {
            var supplier = new Tedarikci
            {
                MarketId = _marketId,
                Ad = FormAd,
                IletisimNo = FormIletisimNo,
                TeslimatGunleri = FormTeslimatGunleri,
                Kategori = FormKategori
            };
            _supplierService.AddSupplier(supplier);
        }

        HideForm();
        LoadSuppliers();
    }

    private void ExecuteDelete()
    {
        if (SelectedSupplier == null)
        {
            MessageBox.Show("Silmek i�in bir tedarik�i se�in.", "Uyar�",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"'{SelectedSupplier.Ad}' silinsin mi?", "Onay",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            _supplierService.DeleteSupplier(SelectedSupplier.Id);
            LoadSuppliers();
        }
    }

    private void HideForm()
    {
        IsFormVisible = false;
        ClearForm();
    }

    private void ClearForm()
    {
        FormAd = string.Empty;
        FormIletisimNo = string.Empty;
        FormTeslimatGunleri = string.Empty;
        FormKategori = string.Empty;
        _editingSupplierId = 0;
    }
}
