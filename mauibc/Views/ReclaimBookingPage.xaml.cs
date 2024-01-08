namespace MAUIBC;
[QueryProperty("tokenId", "tokenId")]
public partial class ReClaimBookingPage : ContentPage {
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
    public ReClaimBookingPage(INFTService nft) {
        InitializeComponent();
        nftService = nft;
        BindingContext = this;
    }

    private async void OnReclaimClicked(object sender, EventArgs e) {
        if (tokenId <= 0) {
            Message.Text = "Invalid NFT Token Id";
            return;
        }
        await Task.Yield();
        ReclaimButton.IsEnabled = false;
        try {
            IsBusy = true;
            NFTItem result = await nftService.Reclaim(tokenId);
            IsBusy = false;
            Message.Text = @$"Reclaim Booking NFT Id:{result.Id}, BookingDate (UTC): {result.BookingDate.ToString("dd/MM/yyyy")}";
        } catch (Exception ex) {
            IsBusy = false;
            Message.Text = ex.Message;
        }
        ReclaimButton.IsEnabled = true;
    }
    protected override void OnAppearing() {
        base.OnAppearing();
        Message.Text = "";
        if (!nftService.IsConnected)
            ReclaimButton.IsEnabled = false;
        else
            ReclaimButton.IsEnabled = true;
    }
}