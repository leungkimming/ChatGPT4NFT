# 2. MeetingRoom NFT Contract Walkthrough
* It is written in the Solidity Language compiled to WASM bytecode stored in an ethereum blockchain
* Each deployment of this contract to the blockchain will create a seperate instance with an unique contract address
* It will run in one of the ethereum blockchain's node
# Business logic design
* It is a ERC721 Non-Fungible Token (NFT)
    * Every Token has only ONE copy and is unique
    * Every Token has a unique URI (the UTC booking date)
    * If I own a Token on 13/07/2024, nobody can own a token on the same date
    * If I transfer my NFT to Mr. A, the ownership of 13/07/2024 booking will be changed to Mr. A as well
    * Each contract instance will have its own NFTs created under the contract instance's address  
* After booking a date, I can release it to make it vacant. I can also occupy it again.
    * Other users can search for vacant booking and request you to transfer the ownership
* _tokenIds is a Counter from the Counters library
    * current() returns the current value
    * .increment() will add 1
* Each NFT has a bookingDate and an occupied flag.
    * Booking Date is in Unix time, that is, number of seconds that have elapsed since 1970-01-01T00:00:00Z UTC
    * Booking Time is always 00:00:00.
    * [online converter playground](https://www.epochconverter.com/)
    * In C#, it is a 'long' integer type. 
* _nfts[] = is a list of NFT created with the list index is the Token Id.
    * e.g. _nfts[17] is the NFT with token Id 17 having a bookingDate and an occupied flag.
* _bookingDates[] = is a list of Token Id with the list index of Dates.
    * e.g. _bookingDates[1704326400] is 17
    * _nfts[17] is the NFT with token Id 17 having a bookingDate 1704326400 (2024年1月4日 00:00:00)
    * if _bookingDates[1704326400] is 0, meaning the list does not contain a booking for 4/1/2024.
# Contract Functions
## Function & property types
* 'view' functions will not change anything. It can return a value directly to the caller
* 'update' functions mine new blocks and write data with a transaction. It cannot directly return data to the caller and need to 'emit' events for the caller to receive.
* pure: e.g. calculation, string concat. can return value directly to caller.
## Access Scope
* internal / private: can only be called within the contract 
* public: can be called within the contract or outside
* external: can only be called from outside
## Implementations
* mintMEET will create a new NFT with the desired booking Date
    * Requirements are book Date not exist in _bookingDates[] and the Date must not in the past.
        * There is no DateTime.Now in Solidity.
        * We can only compare it to the Blockchain's block timestamp.
    * _tokenIds.current() gets the new Token Id
    * Create the entries in _nfts[] & _bookingDates[]
    * _safeMint is to create the NFT and transfer to the receipant
    * _setTokenURI is to set the NFT's unique URI
        * The URL that returns the metadata of the NFT, containing
            * NFT Id
            * Booking Date
            * Graphics of the booking date
    * emit is to emit an event for the calling program to receive the result.
        * Contract transactional functions are executed in an ethereum node
        * Return values are emitted to the blockchain's transaction log
        * The calling program needs to get the return values from the log
* _exists(17) check whether there exist a NFT with Token Id 17
* _beforeTokenTransfer is a hook override for additional check before transfer
    * if (to == address(0)), it is a burn operation with booking date in the past
    * otherwise, Requirement: cannot transfer NFT with booking dates in the past
    * This hook takes effect even when you do the transfer in your wallet
* houseKeep() 'burn' NFTs with booking dates in the past
    * _burn(i) will 'burn' the NFT with token Id i
        * Need to delete the corresponding elements in _bookingDates[] & _nfts[]
        * That's why in most function, we need to check _exists(i) before accessing _nfts[i]
* For the rest of the functions, please refer to the coding.
