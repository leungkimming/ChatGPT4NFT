using Newtonsoft.Json.Linq;
using Nethereum.Contracts;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Windows.Media.AppBroadcasting;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.Util;
using ADRaffy.ENSNormalize;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MAUIBC; 
internal class NFTService : INFTService {
    public string profile { get; set; }
    private string baseURI;
    private string contractAddress;
    private string url;
    private Account account { get; set; }
    private Web3 web3 { get; set; }
    public Contract contract { get; set; }
    public bool IsConnected { get; set; }
    public decimal Balance { get; set; }
    public string MyAddress {
        get { return account.Address; }
    }

    private readonly IConfiguration config;
    public NFTService(IConfiguration _config) {
        IsConnected = false;
        config = _config;
        profile = config.GetSection("MAUIBC:Profile").Value;
        contractAddress = config.GetSection($"MAUIBC:{profile}:ContractAddress").Value;
        url = config.GetSection($"MAUIBC:{profile}:Network").Value;
        baseURI = config.GetSection($"MAUIBC:{profile}:ImageURI").Value;
    }
    public async Task<bool> login(string privateKey) {
        IsConnected = false;
        try {
            account = new Account(privateKey);
            web3 = new Web3(account, url);
            web3.TransactionManager.UseLegacyAsDefault = true; //Using legacy option instead of 1559
            var balance = await web3.Eth.GetBalance.SendRequestAsync(account.Address).ConfigureAwait(false);
            if (balance.Value == 0) return false;
            Balance = Math.Round(Web3.Convert.FromWei(balance.Value, UnitConversion.EthUnit.Ether), 3);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "MAUIBC.Resources.Contracts.MeetingRoom.json";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream)) {
                string jsonContent = reader.ReadToEnd();
                JObject jsonObject = JObject.Parse(jsonContent);
                string abi = jsonObject["abi"].ToString();
                contract = web3.Eth.GetContract(abi, contractAddress);
                IsConnected = true;
                return true;
            }
        } catch (Exception ex) {
            return false;
        }
    }
    public void logout() {
        account = null;
        web3 = null;
        contract = null;
        IsConnected = false;
        Balance = 0;
    }

    public async Task<MintEvent> Mint(DateTime date) {
        TimeSpan ts = TimeZoneInfo.Local.GetUtcOffset(date);
        long unixDate = ((DateTimeOffset)date).ToUnixTimeSeconds();
        long unixDateAdj = unixDate + (ts.Hours * 3600);
        string tokenURI = $"{baseURI}Metadata?date={date.ToString("yyyy-MM-dd")}(UTC)&ID=";

        var f_mint = contract.GetFunction("mintMEET");
        var gas = await f_mint.EstimateGasAsync(account.Address, null, null, account.Address, unixDateAdj, tokenURI).ConfigureAwait(false);
        var receipt = await f_mint.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, account.Address, unixDateAdj, tokenURI).ConfigureAwait(false);

        var events = receipt.DecodeAllEvents<MintEventDTO>();
        var udate = DateTimeOffset.FromUnixTimeSeconds((long)events[0].Event.BookingDate).DateTime;
        //var ldate = TimeZoneInfo.ConvertTimeFromUtc(udate, TimeZoneInfo.Local);
        MintEvent mintevent = new MintEvent();
        mintevent.TokenId = (int)events[0].Event.TokenId;
        mintevent.BookingDate = udate;
        mintevent.To = receipt.From;
        return mintevent;
    }

    public async Task<NFTItem[]> ListNFT(string filter) {
        string function = "";
        switch (filter) {
            case "All": function = "getAllNFTs"; break;
            case "My": function = "getMyToken"; break;
            case "Vacant": function = "getAvailable"; break;
            default: function = "getAllNFTs"; break;
        }
        var f_getNFT = contract.GetFunction(function);
        NFTListDTO nfts = await f_getNFT.CallDeserializingToObjectAsync<NFTListDTO>(account.Address, null, null, null, null).ConfigureAwait(false);
        NFTItem[] result = new NFTItem[nfts.NFTList.Count];
        int i = 0;
        foreach (var nft in nfts.NFTList) {
            var udate = DateTimeOffset.FromUnixTimeSeconds((long)nft.BookingDate).DateTime;
            //var ldate = TimeZoneInfo.ConvertTimeFromUtc(udate, TimeZoneInfo.Local);
            result[i++] = new NFTItem() {
                Id = (int)nft.Id,
                Owner = nft.Owner,
                BookingDate = udate,
                Vacant = !nft.Occupied,
                enableRelease = (nft.Occupied && account.Address == nft.Owner),
                enableReclaim = (!nft.Occupied && account.Address == nft.Owner),
            };
        }
        return result;
    }
    public async Task<NFTItem> Release(int Id) {
        var f_release = contract.GetFunction("release");
        var gas = await f_release.EstimateGasAsync(account.Address, null, null, Id).ConfigureAwait(false);
        var receipt = await f_release.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, Id).ConfigureAwait(false);
        var events = receipt.DecodeAllEvents<OccupyEventDTO>();
        var udate = DateTimeOffset.FromUnixTimeSeconds((long)events[0].Event.bookingDate).DateTime;
        //var ldate = TimeZoneInfo.ConvertTimeFromUtc(udate, TimeZoneInfo.Local);
        NFTItem result = new NFTItem() {
            BookingDate = udate,
            Id = Id,
            Owner = account.Address,
            Vacant = events[0].Event.occupy
        };
        return result;
    }
    public async Task<NFTItem> Reclaim(int Id) {
        var f_release = contract.GetFunction("occupy");
        var gas = await f_release.EstimateGasAsync(account.Address, null, null, Id).ConfigureAwait(false);
        var receipt = await f_release.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, Id).ConfigureAwait(false);
        var events = receipt.DecodeAllEvents<OccupyEventDTO>();
        var udate = DateTimeOffset.FromUnixTimeSeconds((long)events[0].Event.bookingDate).DateTime;
        //var ldate = TimeZoneInfo.ConvertTimeFromUtc(udate, TimeZoneInfo.Local);
        NFTItem result = new NFTItem() {
            BookingDate = udate,
            Id = Id,
            Owner = account.Address,
            Vacant = events[0].Event.occupy
        };
        return result;
    }
    public async Task<int> Clean() {
        var f_release = contract.GetFunction("houseKeep");
        var gas = await f_release.EstimateGasAsync(account.Address, null, null, null).ConfigureAwait(false);
        var receipt = await f_release.SendTransactionAndWaitForReceiptAsync(account.Address, gas, null, null, null, null).ConfigureAwait(false);
        var events = receipt.DecodeAllEvents<BurnEventDTO>();
        return (int)events[0].Event.Count;
    }
    public async Task<NFTItem> Lookup(DateTime date) {
        TimeSpan ts = TimeZoneInfo.Local.GetUtcOffset(date);
        long unixDate = ((DateTimeOffset)date).ToUnixTimeSeconds();
        long unixDateAdj = unixDate + (ts.Hours * 3600);
        //DateTime utcDate = date.ToUniversalTime();
        //long unixDate = ((DateTimeOffset)utcDate).ToUnixTimeSeconds();
        //long unixDate1 = ((DateTimeOffset)date).ToUnixTimeSeconds();
        var f_lookup = contract.GetFunction("isOccupied");
        IsOccupiedOutputDTO result = await f_lookup.CallDeserializingToObjectAsync<IsOccupiedOutputDTO>(unixDateAdj).ConfigureAwait(false);
        if (result.tokenId != 0) {
            var f_getNFT = contract.GetFunction("getNFT");
            NFTDTO nft = await f_getNFT.CallDeserializingToObjectAsync<NFTDTO>(result.tokenId).ConfigureAwait(false);
            var udate = DateTimeOffset.FromUnixTimeSeconds((long)nft.BookingDate).DateTime;
            //var ldate = TimeZoneInfo.ConvertTimeFromUtc(udate, TimeZoneInfo.Local);
            return new NFTItem() {
                BookingDate = udate,
                Id = (int) nft.Id,
                Owner = nft.Owner,
                Vacant = !nft.Occupied
            };
        } else {
            return new NFTItem() {
                Id = 0,
                BookingDate = DateTime.MinValue,
                Owner = null,
                Vacant = true
            };
        }
    }
}
