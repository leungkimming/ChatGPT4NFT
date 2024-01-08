// SPDX-License-Identifier: MIT
pragma solidity ^0.8.4;

import "@openzeppelin/contracts/token/ERC721/extensions/ERC721URIStorage.sol";
import "@openzeppelin/contracts/utils/Counters.sol";
import "@openzeppelin/contracts/access/Ownable.sol";
import "@openzeppelin/contracts/utils/Address.sol";

contract MeetingRoom is ERC721URIStorage, Ownable {
    using Counters for Counters.Counter;
    Counters.Counter private _tokenIds;

    struct NFT {
        uint256 bookingDate;
        bool occupied;
    }
    struct NFTList {
        uint256 Id;
        address owner;
        uint256 bookingDate;
        bool occupied;
    }
    event Mint(address indexed to, uint256 indexed tokenId, uint256 indexed bookingDate);
    event Burn(uint256 count);
    event Occupy(bool occupy, address indexed sender, uint256 indexed tokenId, uint256 indexed bookingDate);

    mapping(uint256 => NFT) private _nfts;
    mapping(uint256 => uint256) private _bookingDates;

    constructor() ERC721("MeetingRoom", "MEET") {}

    function totalSupply() public view returns (uint256) {
        return _tokenIds.current();
    }

    function getAllNFTs() external view returns (NFTList[] memory) {
        NFTList[] memory allNFTs;
        uint256 count = 0;
        for (uint256 i = 1; i <= _tokenIds.current(); i++) {
            if (_exists(i)) {
                count++;
            }
        }

        allNFTs = new NFTList[](count);
        count = 0;
        for (uint256 i = 1; i <= _tokenIds.current(); i++) {
            if (_exists(i)) {
                allNFTs[count] = NFTList(i, ownerOf(i), _nfts[i].bookingDate, _nfts[i].occupied);
                count++;
            }
        }
        return allNFTs;
    }
    
    function getNFT(uint256 Tid) external view returns (NFTList memory) {
        require(_exists(Tid), "Token does not exist");
        return NFTList(Tid, ownerOf(Tid), _nfts[Tid].bookingDate, _nfts[Tid].occupied);
    }

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

    function getBookingDate(uint256 tokenId) external view returns (uint256) {
        require(_exists(tokenId), "Token does not exist");
        return _nfts[tokenId].bookingDate;
    }

    function isOccupied(uint256 date) external view returns (bool, uint256) {
        uint256 _tokenId = _bookingDates[date];
        if (_exists(_tokenId)) {
            return (_nfts[_tokenId].occupied, _tokenId);
        } else {
            return (false, 0);
        }
    }

    function release(uint256 tokenId) external {
        require(_exists(tokenId), "Token does not exist");
        require(ownerOf(tokenId) == msg.sender, "Only the token owner can release");
        require(_nfts[tokenId].bookingDate >= (block.timestamp - (block.timestamp % 1 days)), "Token Expired");
        require(_nfts[tokenId].occupied, "Token not yet occupied");
        _nfts[tokenId].occupied = false;
        emit Occupy(false, msg.sender, tokenId, _nfts[tokenId].bookingDate);
    }
    function concatenate(string memory string1, string memory string2) pure internal returns (string memory) {
        return string(abi.encodePacked(string1, string2));
    }
    function occupy(uint256 tokenId) external {
        require(_exists(tokenId), "Token does not exist");
        require(ownerOf(tokenId) == msg.sender, "Only the token owner can occupy");
        require(_nfts[tokenId].bookingDate >= (block.timestamp - (block.timestamp % 1 days)), "Token Expired");
        require(!_nfts[tokenId].occupied, "Token already occupied");
        _nfts[tokenId].occupied = true;
        emit Occupy(true, msg.sender, tokenId, _nfts[tokenId].bookingDate);
    }

    function getAvailable() external view returns (NFTList[] memory) {
        NFTList[] memory availableTokens;
        uint256 count = 0;
        for (uint256 i = 1; i <= _tokenIds.current(); i++) {
            if (_exists(i) && !_nfts[i].occupied && (_nfts[i].bookingDate >= (block.timestamp - (block.timestamp % 1 days)))) {
                count++;
            }
        }
        availableTokens = new NFTList[](count);
        count = 0;
        for (uint256 i = 1; i <= _tokenIds.current(); i++) {
            if (_exists(i) && !_nfts[i].occupied && (_nfts[i].bookingDate >= (block.timestamp - (block.timestamp % 1 days)))) {
                availableTokens[count] = NFTList(i, ownerOf(i), _nfts[i].bookingDate, _nfts[i].occupied);
                count++;
            }
        }
        return availableTokens;
    }

    function getMyToken() external view returns (NFTList[] memory) {
        NFTList[] memory availableTokens;
        uint256 count = 0;
        for (uint256 i = 1; i <= _tokenIds.current(); i++) {
            if (_exists(i) && ownerOf(i) == msg.sender) {
                count++;
            }
        }
        availableTokens = new NFTList[](count);
        count = 0;
        for (uint256 i = 1; i <= _tokenIds.current(); i++) {
            if (_exists(i) && ownerOf(i) == msg.sender) {
                availableTokens[count] = NFTList(i, ownerOf(i), _nfts[i].bookingDate, _nfts[i].occupied);
                count++;
            }
        }
        return availableTokens;
    }

    function _beforeTokenTransfer(address from, address to, uint256 tokenId, uint256 batchSize) internal virtual override {
        super._beforeTokenTransfer(from, to, tokenId, batchSize);
        if (to != address(0)) {
            require(_nfts[tokenId].bookingDate >= (block.timestamp - (block.timestamp % 1 days)), "Token Expired");
        }
    }

    function transferMEET(address from, address to, uint256 tokenId) external {
        require(_exists(tokenId), "Token does not exist");
        require(ownerOf(tokenId) == from, "Only the token owner can transfer");
        require(_nfts[tokenId].bookingDate >= (block.timestamp - (block.timestamp % 1 days)), "Token Expired");
        _transfer(from, to, tokenId);
    }

    function houseKeep() public returns (uint256) {
        uint256 count = 0;
        for (uint256 i = 1; i <= _tokenIds.current(); i++) {
            if (_exists(i) && (_nfts[i].bookingDate < (block.timestamp - (block.timestamp % 1 days)))) {
                _burn(i);
                delete _bookingDates[_nfts[i].bookingDate];
                delete _nfts[i];
                count++;
            }
        }
        emit Burn(count);
        return count;
    }
}
