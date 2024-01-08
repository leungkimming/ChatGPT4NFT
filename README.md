# Project Goals
* Using NFT as booking reservation and show the booking date as the NFT's graphics in wallets
* A MAUI form based App to execute NFT contract functions
* A ChatGPT based bot to execute the same NFT contract functions
* Complete and working programs in GitHub
# Ethereum development environment
## Truffle
### Installation
* [Windows 11 Installation](https://github.com/orgs/trufflesuite/discussions/5089)
* Ensure the DOS path includes the followings:
```
C:\Program Files\nodejs\;
C:\ProgramData\chocolatey\bin;
C:\Users\<user>\AppData\Local\Microsoft\WindowsApps;
C:\Users\<user>\AppData\Roaming\npm;
```
* Besides 'Truffle', 'Ganache' will be installed. Ganache runs on top of truffle to provide a testing blockchain in localhost
* [Quick Start](https://trufflesuite.com/docs/truffle/quickstart/)
### Create the Contract Project
* In DOS prompt, create a new project folder 'MyProject' and CD to the folder
* Execute 'truffle help' will give you all the Truffle commands
* Execute 'Ganache help' will give you all the Ganache commands
* Execute 'truffle init'. The following folders/file will be created
```
 Directory of C:\Users\<user>\Desktop\MyProject

04/01/2024  13:45    <DIR>          .
04/01/2024  14:18    <DIR>          ..
04/01/2024  13:45    <DIR>          contracts
04/01/2024  13:45    <DIR>          migrations
04/01/2024  13:45    <DIR>          test
19/11/2023  17:13             5,927 truffle-config.js
```
* In truffle-config.js, uncomment below
```
    development: {
     host: "127.0.0.1",     // Localhost (default: none)
     port: 8545,            // Standard Ethereum port (default: none)
     network_id: "*",       // Any network (default: none)
    },
```
* and add below under the development item above.
```
    dashboard: {
      port: 24012,
      host: "localhost"
    },
```
* 'development' network is for testing with a localhost chain
* 'dashboard' network is for conneting the Metamask wallet's testing chain on the Cloud.
### Contract development
* Copy the MeetingRoom.sol to the contracts folder. This is our NFT Contact code.
* Copy the 1_MeetingRoom.js to the migrations folder. This is for deploying the contract to the testing chains.
* [MeetingRoom NFT Contract Explained](README1.md)
* To start the ethereum development blockchain
    * In DOS prompt, CD to MyProject folder adn create a new folder call 'Chain'
    * Execute 'ganache -d --database.dbPath D:\Repos\Labs\Blockchain\MeetingRoom\Chain -i 123456'
        * -d is to stop random generation of accounts and keys
        * ----database.dbPath is to persist all the blockchain's data for subsequent test runs
    * Note the 10 accounts and 10 private keys generated.
        * They will be used in the next secton
    * This DOS prompt will be used to show the blockchain's transaction log 
* To compile and deploy the MeetingBooking contract,
    * Open a new DOS prompt and CD to the project folder
    * In DOS prompt, execute 'truffle compile' and 'truffle deploy'
    * Notice the transaction log and the contract's address in both DOS prompts
    * The contract address will be used in the next section

## Unit Test the Contract
* Next, we need to write a unit test for all the functions inside the contract.
    * We will use C# and the Nethereum nuget library
* Copy the 'nether' folder to the MyProject folder

### appsettings_sample.json
* rename to appsettings.json
* Under "Ganache"
    * "ContractAddress": <-- copy from the result of the 'truffle deploy' command above
    * "Network": "http://localhost:8545" <-- same as that defined in the 'truffle-config.js' - development network
    * "ImageURI": "https://xxxx.yyyy.com/"
        * For initial development, just hardcode to a dummy URL
        * Finally, it is an URL that returns the metadata and image of the NFT
    * "PrivateKey": <-- copy from Granache's first private key (It is the first account's private key for creating NFT)
    * "MyFriend": <-- copy from Granache's second account (NOT PRIVATE KEY! We will transfer NFT to this account) 
* "Profile": "Ganache" <-- Unit test will use Ganache. Ignore Infura for now
### program.cs
* The effort in developing unit testing will not be wasted
* The data classes and contract calling function will be reused in the final App in section 3
#### Setup
* Setup the Nethereum.Web3.Accounts.Account
    * The account.Address will be the ethereum unique address associated with the private key 
```
var privateKey = profile["PrivateKey"];
var account = new Account(privateKey);
Console.WriteLine("My account: " + account.Address);
```
* Setup the Nethereum.Web3.Web3
```
var url = profile["Network"];
var web3 = new Web3(account, url);
```
* Setup the Nethereum.Contracts.Contract
    * The Application Binary Interface (abi) in MeetingRoom.json is the compiled version of MeetingRoom.sol
    * 'contract' is build from the Contract's abi and its deployed instance address
```
string contractAddress = profile["ContractAddress"];
string path = @"../../../../build/contracts/MeetingRoom.json";
string jsonContent = File.ReadAllText(path);
JObject jsonObject = JObject.Parse(jsonContent);
string abi = jsonObject["abi"].ToString();
var contract = web3.Eth.GetContract(abi, contractAddress);
```
#### unix date handling
* Convert to UTC Unix Date
    * e.g. Our goal is to make '04/01/2024 00:00:00' in the date parameter into a Coordinated Universal Time (UTC) of Unix Timestamp, that is, number of seconds that have elapsed since 1970-01-01T00:00:00Z UTC
    * The DateTimeOffset.ToUnixTimeSeconds function will assume variable date is local time and auto convert it to UTC (-8hr from HK). It becomes '03/01/2024 16:00:00' which is not what we want
    * To correct it, we need to find out the adjustment: ts = TimeZoneInfo.Local.GetUtcOffset(date)
    * Then, we add back the adjustment (ts.Hours * 3600) because 1 Hr has 3600 seconds.
```
static async Task Lookup(Contract contract, DateTime date) {
    TimeSpan ts = TimeZoneInfo.Local.GetUtcOffset(date);
    long unixDate = ((DateTimeOffset)date).ToUnixTimeSeconds();
    long unixDateAdj = unixDate + (ts.Hours * 3600);
```
* Convert UTC Unix back to DateTime
    * The DateTimeOffset.FromUnixTimeSeconds keeps the date in UTC without adjusting it back to local time
    * Hence, we do not need to reverse the adjustment
```
var udate = DateTimeOffset.FromUnixTimeSeconds((long)nft.BookingDate).DateTime;
```
#### View function returning a scalar variable
```
var f_totalSupply = contract.GetFunction("totalSupply");
var total = await f_totalSupply.CallAsync<int>();
```
* The corresponding function in MeetingRoom.sol
```
function totalSupply() public view returns (uint256) {
    return _tokenIds.current();
}
```
#### View function returning an object
* Note the annotations match with that in MeetingRoom.sol below
```
var f_getNFT = contract.GetFunction("getNFT");
NFTDTO nft = await f_getNFT.CallDeserializingToObjectAsync<NFTDTO>(Id);

[FunctionOutput]
public class NFTDTO : IFunctionOutputDTO {
    [Parameter("uint256", "Id", 1)]
    public BigInteger Id { get; set; }

    [Parameter("address", "owner", 2)]
    public string Owner { get; set; }

    [Parameter("uint256", "bookingDate", 3)]
    public BigInteger BookingDate { get; set; }

    [Parameter("bool", "occupied", 4)]
    public bool Occupied { get; set; }
}
```
* The corresponding function in MeetingRoom.sol
```
struct NFTList {
    uint256 Id;
    address owner;
    uint256 bookingDate;
    bool occupied;
}
function getNFT(uint256 Tid) external view returns (NFTList memory) {
    require(_exists(Tid), "Token does not exist");
    return NFTList(Tid, ownerOf(Tid), _nfts[Tid].bookingDate, _nfts[Tid].occupied);
}
```
#### Update function receiving events as return
* Update functions need to mine new block and require 'gas'. Need to estimate gas usage.
* SendTransactionAndWaitForReceiptAsync parameters:
    * 1: the wallet account paying the transaction
    * 2: the gas
    * 3: amont of ether to send (not used)
    * 4: max fee per gas (not used)
    * 5: max priority fee per gas (not used)
    * 6: arguments[] for our function
* Note the annotations match with that in MeetingRoom.sol below
```
var f_occupy = contract.GetFunction("occupy");
var gas = await f_occupy.EstimateGasAsync(account.Address, null, null, Id);
var receipt = await f_occupy.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, Id);
var events = receipt.DecodeAllEvents<OccupyEventDTO>();

[Event("Occupy")]
public class OccupyEventDTO : IEventDTO {
    [Parameter("bool", "occupy", 1, false)]
    public bool occupy { get; set; }

    [Parameter("address", "sender", 2, true)]
    public string sender { get; set; }

    [Parameter("uint256", "tokenId", 3, true)]
    public BigInteger tokenId { get; set; }

    [Parameter("uint256", "bookingDate", 4, true)]
    public BigInteger bookingDate { get; set; }
}
```
* The corresponding function in MeetingRoom.sol
    * define the Occupy event
```
event Occupy(bool occupy, address indexed sender, uint256 indexed tokenId, uint256 indexed bookingDate);

function occupy(uint256 tokenId) external {
    require(_exists(tokenId), "Token does not exist");
    require(ownerOf(tokenId) == msg.sender, "Only the token owner can occupy");
    require(_nfts[tokenId].bookingDate >= (block.timestamp - (block.timestamp % 1 days)), "Token Expired");
    require(!_nfts[tokenId].occupied, "Token already occupied");
    _nfts[tokenId].occupied = true;
    emit Occupy(true, msg.sender, tokenId, _nfts[tokenId].bookingDate);
}
```
#### NFT Metadata & Image generation
* Each NFT should have an unique URI that returns the metadata of the NFT, containing
    * Booking Date
    * Graphics of the booking date
    * NFT Id
    * NFT short Name
* E.g. The NFT URI "https://xxxx.yyyy.com/Metadata?date=2024-02-17(UTC)&ID=13" will generate below metadata:
```
{
 "description": "2024-02-17",
 "image": "https://xxxx.yyyy.com/Image?ID=13&date=2024-02-17(UTC)",
 "name": "MEET"
}
```
* The "image" is not a png file but an URL that dynamically draw the graphics of ID and booking date string
* The 'baseURI' is obtained from appsettings.json, "imageURI", which is "https://xxxx.yyyy.com/"
* Hence, in the 'Mint' function:
    * Note that the ID is unknown now
```
string tokenURI = $"{baseURI}Metadata?date={date.ToString("yyyy-MM-dd")}(UTC)&ID=";
var f_mint = contract.GetFunction("mintMEET");
var gas = await f_mint.EstimateGasAsync(account.Address, null, null, account.Address, unixDateAdj, tokenURI).ConfigureAwait(false);
var receipt = await f_mint.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, account.Address, unixDateAdj, tokenURI).ConfigureAwait(false);
```
* The corresponding function in MeetingRoom.sol
    * abi.encodePacked(a,b) is to concat strings a & b
    * This is to append the NFT ID just created to the URI
    * _setTokenURI is to set the unchangeable URI to this NFT
```
function mintMEET(address recipient, uint256 bookingDate, string memory tokenURI_) external returns (uint256) {
    require((_bookingDates[bookingDate] == 0), "Booking date already exists");
    require(bookingDate >= (block.timestamp - (block.timestamp % 1 days)), "Booking date must not be in the past");
    _tokenIds.increment();
    uint256 newTokenId = _tokenIds.current();
    _nfts[newTokenId] = NFT(bookingDate, true);
    _bookingDates[bookingDate] = newTokenId;
    _safeMint(recipient, newTokenId);
    string memory tokenURI = string(abi.encodePacked(tokenURI_, Strings.toString(newTokenId)));
    _setTokenURI(newTokenId, tokenURI);
    emit Mint(recipient, newTokenId, bookingDate);
    return newTokenId;
}
```
* For unit testing without a wallet, all these can be ignored for the time being
#### Unit Test Design
* date0: the latest booking date. The unit test will use dates after date0 and hence can be rerun many times
* Tid1, Tid2: date0 + 1 & date0 + 2. Mint two NFTs for all unit testings.
* Tid3: Get the earliest booking date. If already expired, continue running. Otherwise, stop
* First time execution: Completed with a message that the 'burn' function cannot be tested until tomorrow.
```
Get Expired NFT 1
 List NFT Id = 1
   NFT: Id=1, Owner=0x90F8bf6A479f320ead074411a4B0e7944Ea8c9C1, BookingDate=05-01-2024 UTC, Occupied=occupied
   NFT 1 not expired yet. Wait for tomorrow for the following tests to run.
```
* When executed the next day, all tests are successful
## [Next, Test blockchain in Infura and Metamask wallets](README2.md)
# Credits
* Nodejs
* truffle suite
* Infura
* Microsoft Visual Studio
* Azure Function
* Nethereum
* OpenAI
* SemanticKernel