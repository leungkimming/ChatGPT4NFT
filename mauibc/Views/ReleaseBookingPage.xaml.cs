namespace MAUIBC;
[QueryProperty("tokenId", "tokenId")]
public partial class ReleaseBookingPage : ContentPage {
    private INFTService nftService;

    private int _tokenId;
    public int tokenId {
        get { return _tokenId; }
        set {
            if (_tokenId != value) {
                _tokenId = value;
                OnPropertyChanged();
            }
        }
    }
    public ReleaseBookingPage(INFTService nft) {
        InitializeComponent();
        nftService = nft;
        BindingContext = this;
    }

    private async void OnReleaseClicked(object sender, EventArgs e) {
        if (tokenId <= 0) {
            Message.Text = "Invalid NFT Token Id";
            return;
        }
        await Task.Yield();
        ReleaseButton.IsEnabled = false;
        try {
            IsBusy = true;
            NFTItem result = await nftService.Release(tokenId);
            IsBusy = false;
            Message.Text = @$"Released Booking NFT Id:{result.Id}, BookingDate (UTC): {result.BookingDate.ToString("dd/MM/yyyy")}";
        } catch (Exception ex) {
            IsBusy = false;
            Message.Text = ex.Message;
        }
        ReleaseButton.IsEnabled = true;
    }
    protected override void OnAppearing() {
        base.OnAppearing();
        Message.Text = "";
        if (!nftService.IsConnected)
            ReleaseButton.IsEnabled = false;
        else
            ReleaseButton.IsEnabled = true;
    }
}