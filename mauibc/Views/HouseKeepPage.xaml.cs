using Nethereum.Contracts.Standards.ERC20.TokenList;

namespace MAUIBC;

public partial class HouseKeepPage : ContentPage {
    private INFTService nftService;
    public HouseKeepPage(INFTService nft) {
        InitializeComponent();
        nftService = nft;
        BindingContext = this;
    }
    private async void OnCleanClicked(object sender, EventArgs e) {
        await Task.Yield();
        CleanButton.IsEnabled = false;
        try {
            IsBusy = true;
            int count = await nftService.Clean();
            IsBusy = false;
            Message.Text = @$"Deleted {count} expired Booking NFTs";
        } catch (Exception ex) {
            IsBusy = false;
            Message.Text = ex.Message;
        }
        CleanButton.IsEnabled = true;
    }
    protected override void OnAppearing() {
        base.OnAppearing();
        Message.Text = "";
        if (!nftService.IsConnected)
            CleanButton.IsEnabled = false;
        else
            CleanButton.IsEnabled = true;
    }
}