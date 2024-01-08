# Create an MAUI App for the Contract
* Install Microsoft Visual Studio latest community edition, including latest .NET and cross platform development tools
* Copy the 'mauibc' folder to your project folder
* [MAUI Get Started](https://learn.microsoft.com/en-us/dotnet/maui/get-started/first-app?view=net-maui-8.0&tabs=vswin&pivots=devices-windows)
## Program Structure
* Ignore the ChatGPT AI part for now. It will be covered in the next section
* Most of the coding is copy and adopted from unit testing coding
* Resources\Raw\appsettings_sample.json:
    * rename to appsettings.json
    * similar to that in the unit test
* Models\NFT.cs
    * The Events and FunctionOuput data structures are adapted from unit tests
    * NFTItem
        * Id, Owner, Booking copied from NFTDTO FunctionOuput
        * Vacant is the negation of Occupied in NFTDTO
        * enableRelease: login wallet account is the NFT owner and not Vacant
        * enableReclaim: login wallet account is the NFT owner and Vacant
* Service\INFTService.cs and NFTService.cs
    * Mint, ListNFT, Release, Reclaim, Lookup & Clean are adapted from unit tests
    * IsConnect: whether has login or logout wallet
    * profile: the current profile, Ganache or Infura
    * Balance: the balance of Ether of the login wallet
    * MyAddress: the Ethereum address of your login wallet
    * contract: the deployed MeetingRoom contract instance's address
    * login
        * Get the parameters in current profile
        * User login by entering the wallet's private key
        * Creates Nethereum objects
            * Nethereum.Web3.Accounts.Account
            * Nethereum.Web3.Web3
            * Nethereum.Contracts.Contract
        * Update contract, Balance, MyAddress & IsConnect property
    * Logout
        * Clear the Nethereum objects
        * Clear the contract, Balance, MyAddress & IsConnect property
* ActivityIndicator control
    * various xaml files
```
<ActivityIndicator IsRunning="{Binding IsBusy}" Scale="4" Color="black"
    HorizontalOptions="Center"
    VerticalOptions="Center"
    IsVisible="{Binding IsBusy}" />
```
    * wwwroot\css\app.css
```
.spinner {
    border: 16px solid silver;
    border-top: 16px solid #337AB7;
    border-radius: 50%;
    width: 80px;
    height: 80px;
    animation: spin 700ms linear infinite;
    top: 40%;
    left: 55%;
    position: absolute;
}

@keyframes spin {
    0% {
        transform: rotate(0deg)
    }

    100% {
        transform: rotate(360deg)
    }
}

```
* AppShell.xaml:
    * MainPage as first page
    * Menu to go to different Routes
* MauiPrograms.cs: register dependency injection for NFT service and view pages
* Views
    * OnAppearing override
        * diable buttons if IsConnected is false
    * OnClicked events
        * set IsBusy flag to turn on/off ActivityIndicator
        * to make ActivityIndicator spin and button enable/disable, we need to call 'await Task.Yield()' first 
    * MainPage: login and logout
    * NFTListPage: Enquiry with filtering by My Token/Released bookings/Bookings by month
        * According to the enableRelease &  enableReclaim flag, show command to invoke release or reclaim page passing the NFT Id.
    * MintNFTPage: Make new bookings
    * ReleaseBookingPage: Release bookings
    * ReclaimBookingPage: Reclaim released bookings
    * HouseKeepPage: clean up expired NFTs
## Test the MAUI App
* You need your wallet's private key to conntect to your wallet first
* [In case you forgot how to obtain your wallet's private key, read here](README2.md)
* After wallet connected, the remaining Ether will be shown. You need Ether to execute transactions
* [In case you forgot how to obtain Ether, read here](README2.md)
* The drop down menu is on the top left hand side
* You can setup more MetaMask wallets on different mobiles, obtain the private key and Mint NFTs
* Remember to import those NFTs as instructed by the program
* You can transfer NFTs among these mobiles provided that they are not yet expired
* Read the next section before trying the ChatGPT function
## [Next, we will create a ChatGPT bot to do the same things as MAUI App](README4.md)