using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using System.Numerics;

namespace MAUIBC;

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
public struct MintEvent {
    public string To;
    public int TokenId;
    public DateTime BookingDate;
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
public class NFTItem {
    public int Id { get; set; }
    public string Owner { get; set; }
    public DateTime BookingDate { get; set; }
    public bool Vacant { get; set; }
    public bool enableRelease { get; set; }
    public bool enableReclaim { get; set; }
}