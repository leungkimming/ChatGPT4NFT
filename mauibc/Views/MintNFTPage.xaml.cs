using ADRaffy.ENSNormalize;
using System;
using System.Collections.ObjectModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MAUIBC;

public partial class MintNFTPage : ContentPage {
    private INFTService nftService;
    public DateTime Tomorrow {
        get { return DateTime.UtcNow.Date.AddDays(0); }
    }
    public MintNFTPage(INFTService nft) {
        InitializeComponent();
        nftService = nft;
        BindingContext = this;
    }
    private async void OnMintClicked(object sender, EventArgs e) {
        await Task.Yield();
        MintButton.IsEnabled = false;
        try {
            IsBusy = true;
            NFTItem query = await nftService.Lookup(BookingDate.Date);
            if (query.Id != 0) {
                if (query.Owner == nftService.MyAddress) {
                    Message.Text = $"You have already booked {BookingDate.Date.ToString("dd/MM/yyyy")} UTC.";
                    if (query.Vacant) {
                        Message.Text += "\nHowever, it is now vacant. Please Reclaim it.";
                    }
                } else {
                    Message.Text = $"Booking on {BookingDate.Date.ToString("dd/MM/yyyy")} (UTC) is currently owned by {query.Owner}.";
                    if (query.Vacant) {
                        Message.Text += "\nHowever, it is now vacant. You can request the owner to tranfer it to you.";
                    } else {
                        Message.Text += "\nPlease select another Date.";
                    }
                }
                IsBusy = false;
                MintButton.IsEnabled = true;
                return;
            }
            MintEvent mintevent = await nftService.Mint(BookingDate.Date);
            IsBusy = false;
            Message.Text = @$"Booking NFT Id:{mintevent.TokenId} 
minted with bookingDate: {mintevent.BookingDate.ToString("dd/MM/yyyy")} (UTC) 
to your Account {mintevent.To} 
Please import this NFT into your wallet:
  Contract:{nftService.contract.Address}
  Token Id:{mintevent.TokenId}";
        } catch (Exception ex) {
            IsBusy = false;
            Message.Text = ex.Message;
        }
        MintButton.IsEnabled = true;
    }
    protected override void OnAppearing() {
        base.OnAppearing();
        Message.Text = "";
        if (!nftService.IsConnected)
            MintButton.IsEnabled = false;
        else
            MintButton.IsEnabled = true;
    }
}