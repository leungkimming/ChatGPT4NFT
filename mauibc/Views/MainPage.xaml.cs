using ChatMAUI;
using Microsoft.Maui.Storage;
using Microsoft.SemanticKernel.ChatCompletion;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Reflection;

namespace MAUIBC;

public partial class MainPage : ContentPage
{
	private INFTService nftService;
	public bool StatusLogin { get; set; }
    private IOpenAI OpenAI { get; set; }
    public MainPage(INFTService nft, IOpenAI openAI) {
		InitializeComponent();
		nftService = nft;
		StatusLogin = false;
        BindingContext = this;
        ProfileLabel.Text = nftService.profile;
        OpenAI = openAI;
    }
	private async void OnLoginClicked(object sender, EventArgs e) {
        await Task.Yield();
        LoginBtn.IsEnabled = false;
        if (StatusLogin) {
            PrivateKey.Text = "";
            nftService.logout();
            StatusLogin = false;
            LoginBtn.Text = $"Connect to your Wallet";
            WalletStatus.Text = "Wallet Status: Disconnected";
			await OpenAI.startChat();
            OpenAI.OnChatHistoryCleared();
		} else {
            IsBusy = true;
            StatusLogin = await nftService.login(PrivateKey.Text);
            IsBusy = false;
            if (StatusLogin) {
                LoginBtn.Text = $"Disconnect your Wallet";
                WalletStatus.Text = $"Wallet Status: Connected Successfully; Balance = {nftService.Balance} Ether";
				//await Shell.Current.GoToAsync($"//MyTokens");
			} else {
                LoginBtn.Text = $"Connect to your Wallet";
                WalletStatus.Text = "Wallet Status: Private Key Error";
            }
        }
        SemanticScreenReader.Announce(LoginBtn.Text);
        LoginBtn.IsEnabled = true;
    }
    protected override void OnAppearing() {
        base.OnAppearing();
	}
}