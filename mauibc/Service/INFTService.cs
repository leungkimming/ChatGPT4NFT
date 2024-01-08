using Nethereum.Contracts;
using Nethereum.Web3.Accounts;
using System;
using System.Threading.Tasks;

namespace MAUIBC;
public interface INFTService {
    bool IsConnected { get; set; }
    Contract contract { get; set; }
    string profile { get; set; }
    decimal Balance { get; set; }
    string MyAddress { get; }
    Task<bool> login(string privateKey);
    void logout();
    Task<MintEvent> Mint(DateTime date);
    Task<NFTItem[]> ListNFT(string filter);
    Task<NFTItem> Release(int Id);
    Task<NFTItem> Reclaim(int Id);
    Task<NFTItem> Lookup(DateTime date);
    Task<int> Clean();
}
