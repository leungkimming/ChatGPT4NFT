using Newtonsoft.Json.Linq;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Text.Json;

namespace MeetingRoom;
public class Program {
    //ganache -d --database.dbPath D:\Repos\Labs\Blockchain\MeetingRoom\Chain -i 123456
    //truffle compile
    //truffle deploy
    //Copy PrivateKey, ContractAddress & My Friend to appsettings.json
    public static string baseURI { get; set;}
    static async Task Main(string[] args) {
		string param = Path.Combine(Directory.GetCurrentDirectory(), @"../../../appsettings.json");
		string jsonString = await File.ReadAllTextAsync(param);
		JsonDocument doc = JsonDocument.Parse(jsonString);
		string select = doc.RootElement.GetProperty("Nether").GetProperty("Profile").GetString();

		var profile = new Dictionary<string, string> {
	        { "Network", doc.RootElement.GetProperty("Nether").GetProperty(select).GetProperty("Network").GetString()},
	        { "PrivateKey", doc.RootElement.GetProperty("Nether").GetProperty(select).GetProperty("PrivateKey").GetString()},
			{ "ContractAddress", doc.RootElement.GetProperty("Nether").GetProperty(select).GetProperty("ContractAddress").GetString()},
			{ "ImageURI", doc.RootElement.GetProperty("Nether").GetProperty(select).GetProperty("ImageURI").GetString()},
			{ "MyFriend", doc.RootElement.GetProperty("Nether").GetProperty(select).GetProperty("MyFriend").GetString()}
		};

        baseURI = profile["ImageURI"];
        var url = profile["Network"];
		var privateKey = profile["PrivateKey"];
        var account = new Account(privateKey);
        var web3 = new Web3(account, url);
        web3.TransactionManager.UseLegacyAsDefault = true; //Using legacy option instead of 1559
        Console.WriteLine("My account: " + account.Address);

        string contractAddress = profile["ContractAddress"];
        string path = @"../../../../build/contracts/MeetingRoom.json";
        string jsonContent = File.ReadAllText(path);
        JObject jsonObject = JObject.Parse(jsonContent);
        string abi = jsonObject["abi"].ToString();
        var contract = web3.Eth.GetContract(abi, contractAddress);
        var myFriend = profile["MyFriend"];

        //----------------------start unit testings----------------------
        DateTime date0 = DateTime.Now;
        int Tid1 = 0;
        int Tid2 = 0;
        int Tid3 = 0;

		try { await TotalSupply(contract); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"List All NFTs and get the latest date");
		try { date0 = await GetAllNFT(contract); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Latest date: {date0.ToString("dd/MM/yyyy")} UTC");

		Console.WriteLine($"\n\n\nMint 1st NFT on date: {date0.AddDays(1).ToString("dd/MM/yyyy")} UTC");
		try { Tid1 = await Mint(contract, account, date0.AddDays(1)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Mint on same date already exist errror");
		try { await Mint(contract, account, date0.AddDays(1)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Mint on passed date not allowed");
		try { await Mint(contract, account, date0.AddDays(-300)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Mint 2nd NFT on date: {date0.AddDays(2).ToString("dd/MM/yyyy")} UTC");
		try { Tid2 = await Mint(contract, account, date0.AddDays(2)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"List All NFTs");
		try { await GetAllNFT(contract); } catch (Exception ex) { Console.WriteLine(ex.Message); }

		Console.WriteLine($"\n\n\nTransfer NFT not exist errror");
		try { await Transfer(contract, account, myFriend, 999); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Transferred 2nd NFT to My Friend");
		try { await Transfer(contract, account, myFriend, Tid2); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Transferred 2nd NFT again not owner error");
		try { await Transfer(contract, account, myFriend, Tid2); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"List All NFTs");
		try { await GetAllNFT(contract); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"List My NFTs");
		try { await GetMyNFT(contract, account); } catch (Exception ex) { Console.WriteLine(ex.Message); }

		Console.WriteLine($"\n\n\nRelease NFT not exist errror");
		try { await Release(contract, account, 999); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Release 2nd NFT not owner errror");
		try { await Release(contract, account, Tid2); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Release OK");
		try { await Release(contract, account, Tid1); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Release again. not occupied error");
		try { await Release(contract, account, Tid1); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"List All NFTs");
		try { await GetAllNFT(contract); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"List availalbe NFTs");
		try { await GetAvailableNFT(contract); } catch (Exception ex) { Console.WriteLine(ex.Message); }

		Console.WriteLine($"\n\n\nReclaim NFT not exist errror");
		try { await Occupy(contract, account, 999); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Reclaim 2nd NFT not owner errror");
		try { await Occupy(contract, account, Tid2); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Reclaim OK");
		try { await Occupy(contract, account, Tid1); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Reclaim again. Already reclaimed error");
		try { await Occupy(contract, account, Tid1); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"List All NFTs");
		try { await GetAllNFT(contract); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"List availalbe NFTs");
		try { await GetAvailableNFT(contract); } catch (Exception ex) { Console.WriteLine(ex.Message); }

		Console.WriteLine($"\n\n\nGet NFT Id not exist error");
		try { await GetNFT(contract, 999); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Get NFT {Tid1}");
		try { await GetNFT(contract, Tid1); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Get NFT {Tid2}");
		try { await GetNFT(contract, Tid2); } catch (Exception ex) { Console.WriteLine(ex.Message); }

		Console.WriteLine($"\n\n\nLookup date {date0.AddDays(999).ToString("dd/MM/yyyy")} UTC not exist error");
		try { await Lookup(contract, date0.AddDays(999)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Lookup date {date0.AddDays(1).ToString("dd/MM/yyyy")} UTC");
		try { await Lookup(contract, date0.AddDays(1)); } catch (Exception ex) { Console.WriteLine(ex.Message); }
		Console.WriteLine($"Lookup date {date0.AddDays(2).ToString("dd/MM/yyyy")} UTC");
		try { await Lookup(contract, date0.AddDays(2)); } catch (Exception ex) { Console.WriteLine(ex.Message); }

        //For following tests, you have to wait for 2 days to pass before executing.
        DateTime expireDate = DateTime.Now.Date;
        Console.WriteLine($"\n\n\nList My NFTs and get my last Id for expiry testing");
        try { Tid3 = await GetMyNFT(contract, account); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        Console.WriteLine($"Get Expired NFT {Tid3}");
        try { expireDate = await GetNFT(contract, Tid3); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        if (expireDate.Date >= DateTime.Now.Date) {
			Console.WriteLine($"   NFT {Tid3} not expired yet. Wait for tomorrow for the following tests to run.");
			return;
		}
        Console.WriteLine($"\n\n\nTransfer NFT {Tid3} expired errror");
        try { await Transfer(contract, account, myFriend, Tid3); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        Console.WriteLine($"Release NFT {Tid3} expired errror");
        try { await Release(contract, account, Tid3); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        Console.WriteLine($"Reclaim NFT {Tid3} expired errror");
        try { await Occupy(contract, account, Tid3); } catch (Exception ex) { Console.WriteLine(ex.Message); }

        Console.WriteLine($"\n\n\nBurn expired NFTs (need to use permanent block chain with expired NFTs");
        try { await Clean(contract, account); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        Console.WriteLine($"List All NFTs after burn");
        try { await GetAllNFT(contract); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        Console.WriteLine($"\n\n\nLookup date {expireDate.ToString("dd/MM/yyyy")} not exist error");
        try { await Lookup(contract, expireDate); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        Console.WriteLine($"Get Expired NFT {Tid3} not exist error");
        try { expireDate = await GetNFT(contract, Tid3); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        Console.WriteLine($"Transfer NFT {Tid3} not exist errror");
        try { await Transfer(contract, account, myFriend, Tid3); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        Console.WriteLine($"Release NFT {Tid3} not exist errror");
        try { await Release(contract, account, Tid3); } catch (Exception ex) { Console.WriteLine(ex.Message); }
        Console.WriteLine($"Reclaim NFT {Tid3} not exist errror");
        try { await Occupy(contract, account, Tid3); } catch (Exception ex) { Console.WriteLine(ex.Message); }
    }

	static async Task TotalSupply(Contract contract) {
        Console.WriteLine($"Get total Supply");
        var f_totalSupply = contract.GetFunction("totalSupply");
        var total = await f_totalSupply.CallAsync<int>();
        Console.WriteLine($"   totalSupply: {total}");
    }

    static async Task<DateTime> GetNFT(Contract contract, int Id) {
        Console.WriteLine($" List NFT Id = {Id}");
        var f_getNFT = contract.GetFunction("getNFT");
        NFTDTO nft = await f_getNFT.CallDeserializingToObjectAsync<NFTDTO>(Id);
        var udate = DateTimeOffset.FromUnixTimeSeconds((long)nft.BookingDate).DateTime;
        string s_occupy = nft.Occupied ? "occupied" : "available";
        Console.WriteLine($"   NFT: Id={nft.Id}, Owner={nft.Owner}, BookingDate={udate.ToString("dd-MM-yyyy")} UTC, Occupied={s_occupy}");
        return udate;
    }

    static async Task Release(Contract contract, Account account, int Id) {
        Console.WriteLine($" Release MEET NFT Id = {Id}");
        var f_release = contract.GetFunction("release");
        var gas = await f_release.EstimateGasAsync(account.Address, null, null, Id);
        var receipt = await f_release.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, Id);
        var events = receipt.DecodeAllEvents<OccupyEventDTO>();
        var udate = DateTimeOffset.FromUnixTimeSeconds((long)events[0].Event.bookingDate).DateTime;
        string s_occupy = events[0].Event.occupy ? "occupied" : "available";
        Console.WriteLine($"   MEET token ID {events[0].Event.tokenId} minted with bookingDate: {udate.ToString("dd-MM-yyyy")} UTC: {s_occupy}");
    }

    static async Task Occupy(Contract contract, Account account, int Id) {
        Console.WriteLine($" Occupy MEET NFT Id = {Id}");
        var f_occupy = contract.GetFunction("occupy");
        var gas = await f_occupy.EstimateGasAsync(account.Address, null, null, Id);
        var receipt = await f_occupy.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, Id);
        var events = receipt.DecodeAllEvents<OccupyEventDTO>();
        var udate = DateTimeOffset.FromUnixTimeSeconds((long)events[0].Event.bookingDate).DateTime;
        string s_occupy = events[0].Event.occupy ? "occupied" : "available";
        Console.WriteLine($"   MEET token ID {events[0].Event.tokenId} minted with bookingDate: {udate.ToString("dd-MM-yyyy")} UTC: {s_occupy}");
    }

    static async Task<DateTime> GetAllNFT(Contract contract) {
        var f_getNFT = contract.GetFunction("getAllNFTs");
        NFTListDTO nfts = await f_getNFT.CallDeserializingToObjectAsync<NFTListDTO>();
        Console.WriteLine($" List all NFTs (count = {nfts.NFTList.Count})");
        var ldate = DateTime.Now.Date.AddDays(-1);
        foreach (var nft in nfts.NFTList) {
            var udate = DateTimeOffset.FromUnixTimeSeconds((long)nft.BookingDate).DateTime;
            if (udate.Date > ldate) { ldate = udate.Date; }
            string s_occupy = nft.Occupied ? "occupied" : "available";
            Console.WriteLine($"   NFT: Id={nft.Id}, Owner={nft.Owner}, BookingDate={udate.ToString("dd-MM-yyyy")} UTC, Occupied={s_occupy}");
        }
        return ldate;
    }

    static async Task GetAvailableNFT(Contract contract) {
        var f_getNFT = contract.GetFunction("getAvailable");
        NFTListDTO nfts = await f_getNFT.CallDeserializingToObjectAsync<NFTListDTO>();
        Console.WriteLine($" List all available NFTs (count = {nfts.NFTList.Count})");
        foreach (var nft in nfts.NFTList) {
            var udate = DateTimeOffset.FromUnixTimeSeconds((long)nft.BookingDate).DateTime;
            string s_occupy = nft.Occupied ? "occupied" : "available";
            Console.WriteLine($"   NFT: Id={nft.Id}, Owner={nft.Owner}, BookingDate={udate.ToString("dd-MM-yyyy")} UTC, Occupied={s_occupy}");
        }
    }
    static async Task<int> GetMyNFT(Contract contract, Account account) {
        var f_getNFT = contract.GetFunction("getMyToken");
        NFTListDTO nfts = await f_getNFT.CallDeserializingToObjectAsync<NFTListDTO>(account.Address,null,null,null,null);
        Console.WriteLine($" List My NFTs (count = {nfts.NFTList.Count})");
        int lastId = 0;
        DateTime earliest = DateTime.Now.Date.AddYears(1);
        foreach (var nft in nfts.NFTList) {
            var udate = DateTimeOffset.FromUnixTimeSeconds((long)nft.BookingDate).DateTime;
            string s_occupy = nft.Occupied ? "occupied" : "available";
            Console.WriteLine($"   NFT: Id={nft.Id}, BookingDate={udate.ToString("dd-MM-yyyy")} UTC, Occupied={s_occupy}");
            if (udate.Date < earliest) { 
                earliest = udate.Date;
				lastId = (int)nft.Id;
			}
        }
        return lastId;
    }
    static async Task Lookup(Contract contract, DateTime date) {
        Console.WriteLine($" Lookup availability of {date.ToString("dd/MM/yyyy")} UTC");
		TimeSpan ts = TimeZoneInfo.Local.GetUtcOffset(date);
		long unixDate = ((DateTimeOffset)date).ToUnixTimeSeconds();
		long unixDateAdj = unixDate + (ts.Hours * 3600);

		var f_lookup = contract.GetFunction("isOccupied");
        IsOccupiedOutputDTO result = await f_lookup.CallDeserializingToObjectAsync<IsOccupiedOutputDTO>(unixDateAdj);

        if (result.tokenId != 0) {
            await GetNFT(contract, (int)result.tokenId);
        } else {
            Console.WriteLine($"   NFT for Date {date.ToString("dd/MM/yyyy")} UTC does not yet exist.");
        }
    }
    static async Task<int> Mint(Contract contract, Account account, DateTime date) {
        Console.WriteLine($" Mine MEET NFT on {date.ToString("dd/MM/yyyy")} UTC");
		TimeSpan ts = TimeZoneInfo.Local.GetUtcOffset(date);
		long unixDate = ((DateTimeOffset)date).ToUnixTimeSeconds();
		long unixDateAdj = unixDate + (ts.Hours * 3600);
		string tokenURI = $"{baseURI}Metadata?date={date.ToString("yyyy-MM-dd")}(UTC)&ID=";

		var f_mint = contract.GetFunction("mintMEET");
		var gas = await f_mint.EstimateGasAsync(account.Address, null, null, account.Address, unixDateAdj, tokenURI).ConfigureAwait(false);
		var receipt = await f_mint.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, account.Address, unixDateAdj, tokenURI).ConfigureAwait(false);

		var events = receipt.DecodeAllEvents<MintEventDTO>();
		var udate = DateTimeOffset.FromUnixTimeSeconds((long)events[0].Event.BookingDate).DateTime;

        Console.WriteLine($"   MEET token ID {events[0].Event.TokenId} minted with bookingDate: {udate.ToString("dd/MM/yyyy")} UTC to {receipt.From}");
        return (int)events[0].Event.TokenId;
    }

    static async Task Transfer(Contract contract, Account account, string toAddress, int tokenId) {
        Console.WriteLine($" Transfer MEET NFT {tokenId} from {account.Address} to {toAddress}");

        var f_transfer = contract.GetFunction("transferMEET");
        var gas = await f_transfer.EstimateGasAsync(account.Address, null, null, account.Address, toAddress, tokenId);
        var receipt = await f_transfer.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, account.Address, toAddress, tokenId);

        var events = receipt.DecodeAllEvents<TransferEventDTO>();
        Console.WriteLine($"   MEET token ID {events[0].Event.TokenId} tranferred from {events[0].Event.From} to {events[0].Event.To}");
    }

	static async Task Clean(Contract contract, Account account) {
		var f_release = contract.GetFunction("houseKeep");
		var gas = await f_release.EstimateGasAsync(account.Address, null, null, null).ConfigureAwait(false);
		var receipt = await f_release.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, null).ConfigureAwait(false);
		var events = receipt.DecodeAllEvents<BurnEventDTO>();
		Console.WriteLine($"{events[0].Event.Count} NFT burnt.");
	}
}

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

[Event("Mint")]
public class MintEventDTO : IEventDTO {
    [Parameter("address", "to", 1, true)]
    public virtual string To { get; set; }
    [Parameter("uint256", "tokenId", 2, true)]
    public virtual BigInteger TokenId { get; set; }
    [Parameter("uint256", "bookingDate", 3, true)]
    public virtual BigInteger BookingDate { get; set; }
}

[Event("Transfer")]
public class TransferEventDTO : IEventDTO {
    [Parameter("address", "from", 1, true)]
    public virtual string From { get; set; }
    [Parameter("address", "to", 2, true)]
    public virtual string To { get; set; }
    [Parameter("uint256", "tokenId", 3, true)]
    public virtual BigInteger TokenId { get; set; }
}

[Event("Burn")]
public class BurnEventDTO : IEventDTO {
	[Parameter("uint256", "count", 1, false)]
	public virtual BigInteger Count { get; set; }
}

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

[FunctionOutput]
public class NFTListDTO : IFunctionOutputDTO {
    [Parameter("tuple[]", "", 1)]
    public virtual List<NFTDTO> NFTList { get; set; }
}

[FunctionOutput]
public class IsOccupiedOutputDTO : IFunctionOutputDTO {
    [Parameter("bool", "", 1)]
    public virtual bool occupied { get; set; }
    [Parameter("uint256", "", 2)]
    public virtual BigInteger tokenId { get; set; }
}