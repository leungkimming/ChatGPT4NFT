# 2. Deploy to Testing Environment with Wallets
* Setup Infura private chain
* Setup web wallet
* Setup mobile wallet
* Deploy contract to Infura
* Give NFT a meaningful graphics
* Run unit tests in testing environment
* Import resulting NFT into mobile wallet

## Setup Infura private chain
* Navigate to [Infura Web](https://app.infura.io/)
* Register with a free plan
* Click "CREATE NEW API KEY"
* Name it "MeetingRoom"
* Click "MeetingRoom"
* Click "Active Endpoints"
* Copy the Ethereum Sepolia endpoint as below:
```
https://sepolia.infura.io/v3/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
```
* xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx is your private blockchain API Id
* edit appsettings.json in your project folder
* set the endpoint to Infura Network and save the file
```
"Infura": {

    "Network": "https://sepolia.infura.io/v3/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
```
## Setup web wallet
* We are going to use your browser's Metamask wallet for testing. Never use it to manage your real crypto currencies or NFT!
* Install
    * In Edge/Chrom browser, click 'Extensions' icon
    * Click Open Microsoft Edge Add-ons
    * Seach 'metamask'
    * Install 'MetaMask' from MetaMask (must from MetaMask, not others)
    * Setup your password and backup the recovery keywords (take a picture or write it down)
* Copy your private key to appsettings.json
    * Click the dropdown next to your account at the top
    * Click the 3 dots on the top right side
    * Click Account Details
    * Click Show private key
    * Enter your wallet password
    * Hold to reveal Private Key
    * Copy the private key
    * Paste to appsettings.json, Infura's PrivateKey as follows
```
"Infura": {

    "PrivateKey": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
```
* Connect to your Infura private blockchain
    * Click the dropdown next to your account at the top
    * Click Settings
    * Click Networks on the left menu
    * Click Add a network
        * Network Name: infuraTest
        * New RPC URL: [your Infura endpoint above]
        * Chain ID: 11155111
        * Currency symbol: ETH
        * Block explorer URL: blank
    * Click Save
    * Exit Settings
    * On the top right side, dropdown and select infuraTest
* Obtain some Ethers gas
    * Note your address and remaining Ether (now 0)
    * Click the icon next to your address to copy it
    * Click on Infura web's 'Faucet'
    * Paste your address and click 'RECEIVE ETH'
    * Wait a minute and your wallet have 0.5 Ether to use as gas for testing
## Setup mobile wallet as MyFriend in the unit testing
* You are using your MetaMask mobile wallet for testing. Never use it to manage real crypto currencies or NFT
* Install
    * In App Store / Google Play Store, search for metaMask
    * Ensure it is "MetaMask - Blockchain Wallet" and not others
    * Install and launch the App
    * Setup your password and backup the recovery keywords (take a picture or write it down)
* Copy your wallet's address to appsettings.json
    * Click the icon next to your address to copy it
    * Paste to appsettings.json, Infura, MyFriend as follows:
```
    "Infura": {

      "MyFriend": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
```
* Connect to your Infura private blockchain
    * Click Settings on bottom right most
    * Click Netowrks
    * Click Add Network
    * Click CUSTOM NETWORKS tab
        * Network Name: infuraTest
        * New RPC URL: [your Infura endpoint above]
        * Chain ID: 11155111
        * Currency symbol: ETH
        * Block explorer URL: blank
    * Click Add
    * Exit Networks and click the wallet icon (bottom left most)
    * On the top middle, dropdown and select infuraTest
* Same as the MetaMask web wallet, you can obtain some Ethers gas tomorrow (get Ethers only once per day)
## Connect Truffle to Infura
* If still running Ganache on a DOS prompt, press ctrl-C to stop
* Execute 'Truffle dashboard'
* It will launch a browser and navigate to http://localhost:24012/rpcs
* This is the TRUFFLE Dashboard web
* Click the 'Click to connect' on left bottom
* Logon MetaMask wallet as instructed
## Deploy contract to Infura
* In another DOS prompt, CD to the project folder
    * Execute 'truffle compile'
    * Execute 'truffle deploy --network dashboard'
        * Truffle will deploy contract via your MetaMask wallet thru the dashboard network
            * Since your wallet is connected to Infura, truffle will deploy the contract to Infura 
        * In the TRUFFLE Dashboard web, click 'Confirm'
        * In MetaMask wallet, Click 'Confirm'
    * Copy the Contract's address
    * Paste to appsettings.json, Infura, ContractAddress as below
    * All Infura parameters should have the correct value set
    * Change the Profile to Infura
```
"Infura": {
    "ContractAddress": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",

},
"Profile": "Infura"
```
## Give NFT a meaningful graphics
* Build NFT metadata & image generator Azure Functions
### Deisgn
* Recall that when you mint a NFT, you will assign an unique, unchangeable URI, which will return a metadata, containing
    * Booking Date
    * Graphics of the booking date
    * NFT Id
    * NFT short Name
* We will develop 2 Azure Functions for this purpose. They are 'Metadata' and 'Image'.
* Calling 'Metadata?date=2024-02-17(UTC)&ID=13' will generate something like this
```
{
 "description": "2024-02-17",
 "image": "https://xxxx.yyyy.com/Image?ID=13&date=2024-02-17(UTC)",
 "name": "MEET"
}
```
* Calling 'Image?ID=13&date=2024-02-17(UTC)' will generate an image like this:
```
+-----------------+
| Booking NFT #13 |
|   2024-02-17    |
+-----------------+
```
### Development Setup
* Follow below guide to subscribe to Azure, create a simple Azure Function, deploy to Azure and test
* [Azure Function Development Guide](https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-your-first-function-visual-studio)
* When renaming the function, call it 'Metadata'
* When you successfully Verify your function in Azure (step 3), copy your Azure Function's URL
    * Paste into appsettings.json
```
"Infura": {

    "ImageURI": "https://xxxxxxxxxxxxxxxxxxxxxx.azurewebsites.net/api/",
```
* Copy the program code in the Meet.cs file under the Metadata folder to your Azure Function program
    * Copy the 'using' statements, the 'Metadata' and 'Image' functions
* add <PackageReference Include="System.Drawing.Common" Version="8.0.0" />
* You can test it locally and if ok, deploy to Azure

## Run unit test in testing enviornment
* Run the unit test again today and tomorrow (for testing expired NFT clean up)
* Record the NFT IDs created
    * Id 1 belongs to your MetaMask web wallet's account
    * Id 2 belongs to your MetaMask mobile wallet's account (it was transferred to MyFriend in one of the unit test)
## Import NFT to mobile wallets
* After successful running of unit testing on the Infura network, a few NFTs are created with their Ids recorded
* Login web or mobile MetaMask wallet
* Click the 'NFTs' tab in the middle of the wallet
* Click Import NFT
    * Input Address = the Contract address in appsettings.json, Infura, ContractAddress
    * Token ID = the NFT IDs by which your wallet own
    * The NFT image is the picture of the UTC booking date of the NFT


