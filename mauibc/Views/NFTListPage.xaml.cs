using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MAUIBC;

public partial class NFTListPage : ContentPage {
    private ObservableCollection<NFTItem> _NFTList;
    public ObservableCollection<NFTItem> NFTList {
        get { return _NFTList; }
        set {
            if (_NFTList != value) {
                _NFTList = value;
                OnPropertyChanged();
            }
        }
    }
    private INFTService nftService;
    public string SelectedOption { get; set; }
    public DateTime MonthSelect { get; set; }
    public ICommand ReleaseCommand => new Command<int>(async (tokenId) =>
        await Shell.Current.GoToAsync($"//ReleaseBooking?tokenId={tokenId}"));
    public ICommand ReclaimCommand => new Command<int>(async (tokenId) =>
        await Shell.Current.GoToAsync($"//ReClaimBooking?tokenId={tokenId}"));
    public NFTListPage(INFTService nft) {
        InitializeComponent();
        nftService = nft;
        SelectedOption = "My";
        MonthSelect = DateTime.UtcNow;
        BindingContext = this;
    }
    private async void ApplyButton_Clicked(object sender, EventArgs e) {
        IsBusy = true;
        NFTItem[] items = await nftService.ListNFT(SelectedOption);
        if (SelectedOption != "All") {
            NFTList = new ObservableCollection<NFTItem>(items);
        } else {
            NFTList = new ObservableCollection<NFTItem>(items.Where(x => x.BookingDate.Month == MonthSelect.Month));
        }
        IsBusy = false;
    }
    protected override void OnAppearing() {
        base.OnAppearing();
        NFTList = new ObservableCollection<NFTItem>();
        if (!nftService.IsConnected)
            ApplyButton.IsEnabled = false;
        else
            ApplyButton.IsEnabled = true;
    }
}