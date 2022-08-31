// SPDX-License-Identifier: GPL-3.0

pragma solidity ^0.8.9;

import "@openzeppelin/contracts/token/ERC1155/extensions/ERC1155URIStorage.sol";
import "@openzeppelin/contracts/token/ERC1155/utils/ERC1155Holder.sol";
import "@openzeppelin/contracts/utils/Counters.sol";
import "@openzeppelin/contracts/utils/structs/EnumerableMap.sol";
import "@openzeppelin/contracts/utils/structs/EnumerableSet.sol";

contract Planet is ERC1155URIStorage, ERC1155Holder {
    // Intend to satisfy multiple derivation and ERC165
    function supportsInterface(bytes4 interfaceId)
        public
        view
        override(ERC1155, ERC1155Receiver)
        returns (bool)
    {
        return super.supportsInterface(interfaceId);
    }

    using Counters for Counters.Counter;
    using EnumerableMap for EnumerableMap.UintToUintMap;
    using EnumerableSet for EnumerableSet.UintSet;
    Counters.Counter private _tokenIds;
    Counters.Counter private _itemIds;
    address payable public Owner;

    // Mapping: address => tokenId => amount
    mapping(address => EnumerableMap.UintToUintMap) private userOwnedTokens;

    struct Commodity {
        uint256 itemId;
        uint256 tokenId;
        address payable owner;
        uint256 price;
        uint256 amount;
    }

    // Mapping: itemId => commodity
    mapping(uint256 => Commodity) private idToCommodity;

    // Set: commodityIds
    EnumerableSet.UintSet private commodityIds;

    // Mapping: address => lastTime
    mapping(address => uint256) private addressToLastTime;

    // sum of all products
    /// attention: hasn't change the variable
    uint256 private sumWeight = 0;
    uint256 public fee = 1;

    // events
    event GetRandomKItemsEvent(uint256[] tokenIds, string[] tokenURIs);

    constructor(string memory uri_) ERC1155(uri_) {
        Owner = payable(msg.sender);
        // set BaseURI
        _setBaseURI(uri_);
    }

    modifier onlyOwner() {
        require(msg.sender == Owner, "Not owner");
        _;
    }

    function MintNFT(string[] memory tokenURIs, uint256[] memory amounts)
        public
        onlyOwner
    {
        require(
            tokenURIs.length == amounts.length,
            "ERC1155: tokenURIs and amounts length mismatch"
        );
        for (uint256 i = 0; i < tokenURIs.length; i++) {
            require(amounts[i] >= 1, "Amount must be at least 1");
            _tokenIds.increment();
            uint256 newTokenId = _tokenIds.current();
            _mint(address(this), newTokenId, amounts[i], bytes(tokenURIs[i]));
            _setURI(newTokenId, tokenURIs[i]);
            userOwnedTokens[address(this)].set(newTokenId, amounts[i]);
            sumWeight += amounts[i];
        }
    }

    function GetNFT(address owner)
        public
        view
        returns (
            uint256[] memory,
            string[] memory,
            uint256[] memory
        )
    {
        uint256 length = userOwnedTokens[owner].length();
        uint256[] memory tokenIds = new uint[](length);
        string[] memory tokenURIs = new string[](length);
        uint256[] memory amounts = new uint[](length);

        for (uint256 i = 0; i < length; i++) {
            (tokenIds[i], amounts[i]) = userOwnedTokens[owner].at(i);
            tokenURIs[i] = uri(tokenIds[i]);
        }

        return (tokenIds, tokenURIs, amounts);
    }

    function TransferNFT(
        address from,
        address to,
        uint256 tokenId,
        uint256 amount
    ) public {
        require(amount >= 1, "Amount must be at least 1");
        safeTransferFrom(from, to, tokenId, amount, bytes(uri(tokenId)));
        // Check whether `from' has tokenId left
        uint256 fromAmount = userOwnedTokens[from].get(tokenId);
        if (fromAmount > amount) {
            userOwnedTokens[from].set(tokenId, fromAmount - amount);
        } else {
            userOwnedTokens[from].remove(tokenId);
        }
        // Check whether `to' initially has tokenId
        if (userOwnedTokens[to].contains(tokenId)) {
            userOwnedTokens[to].set(
                tokenId,
                userOwnedTokens[to].get(tokenId) + amount
            );
        } else {
            userOwnedTokens[to].set(tokenId, amount);
        }
        // return to this address
        if (to == address(this)) {
            sumWeight += amount;
        }
    }

    function AddItemToMarket(
        uint256 tokenId,
        uint256 amount,
        uint256 price
    ) public {
        require(amount >= 1, "Amount must be at least 1");
        require(
            balanceOf(msg.sender, tokenId) >= amount,
            "Insufficient balance"
        );
        // add token to the market
        _itemIds.increment();
        uint256 itemId = _itemIds.current();
        idToCommodity[itemId] = Commodity(
            itemId,
            tokenId,
            payable(msg.sender),
            price,
            amount
        );
        commodityIds.add(itemId);
        // msg.sender lose the token
        uint256 newAmount = userOwnedTokens[msg.sender].get(tokenId) - amount;
        if (newAmount != 0) {
            userOwnedTokens[msg.sender].set(
                tokenId,
                userOwnedTokens[msg.sender].get(tokenId) - amount
            );
        } else {
            userOwnedTokens[msg.sender].remove(tokenId);
        }
        // transfer
        safeTransferFrom(
            msg.sender,
            address(this),
            tokenId,
            amount,
            bytes(uri(tokenId))
        );
    }

    function BuyItemAndTransferOwnership(uint256 itemId, uint256 amount)
        public
        payable
    {
        require(
            commodityIds.contains(itemId),
            "The item doesn't exist in market"
        );
        require(
            msg.sender != idToCommodity[itemId].owner,
            "Unable to buy one's own items"
        );
        require(amount >= 1, "Amount must be at least 1");
        uint256 tokenId = idToCommodity[itemId].tokenId; // commodity tokenId
        uint256 price = idToCommodity[itemId].price; // commodity price
        uint256 tokenAmount = idToCommodity[itemId].amount; // commodity amount
        require(tokenAmount >= amount, "Requested amount exceeds the supply");
        require(msg.value == price * amount, "Incorrect money");

        // market decrease supply
        if (tokenAmount > amount) {
            idToCommodity[itemId].amount = tokenAmount - amount;
        } else {
            commodityIds.remove(itemId);
        }
        // transfer token
        if (userOwnedTokens[msg.sender].contains(tokenId)) {
            userOwnedTokens[msg.sender].set(
                tokenId,
                userOwnedTokens[msg.sender].get(tokenId) + amount
            );
        } else {
            userOwnedTokens[msg.sender].set(tokenId, amount);
        }
        _safeTransferFrom(
            address(this),
            msg.sender,
            tokenId,
            amount,
            bytes(uri(tokenId))
        );
        // pay
        idToCommodity[itemId].owner.transfer(msg.value);
    }

    function RedeemItems(uint256 itemId, uint256 amount) public {
        require(amount >= 1, "Amount must be at least 1");
        require(
            commodityIds.contains(itemId),
            "The item doesn't exist in market"
        );
        require(
            msg.sender == idToCommodity[itemId].owner,
            "Must redeem one's own items"
        );
        uint256 tokenId = idToCommodity[itemId].tokenId; // commodity tokenId
        uint256 tokenAmount = idToCommodity[itemId].amount; // commodity amount
        require(tokenAmount >= amount, "Requested amount exceeds the supply");

        // market decrease supply
        if (tokenAmount > amount) {
            idToCommodity[itemId].amount = tokenAmount - amount;
        } else {
            commodityIds.remove(itemId);
        }
        // transfer token
        if (userOwnedTokens[msg.sender].contains(tokenId)) {
            userOwnedTokens[msg.sender].set(
                tokenId,
                userOwnedTokens[msg.sender].get(tokenId) + amount
            );
        } else {
            userOwnedTokens[msg.sender].set(tokenId, amount);
        }
        _safeTransferFrom(
            address(this),
            msg.sender,
            tokenId,
            amount,
            bytes(uri(tokenId))
        );
    }

    function GetUnsoldItems() public view returns (Commodity[] memory) {
        uint256 length = commodityIds.length();
        Commodity[] memory commodities = new Commodity[](length);
        for (uint256 i = 0; i < length; i++) {
            commodities[i] = idToCommodity[commodityIds.at(i)];
        }
        return commodities;
    }

    function GetRandomKItems() public payable {
        uint256 K = 5; // amount of random material
        require(sumWeight >= K, "Insufficient NFT Pool");
        require(
            msg.sender == Owner ||
                msg.value == 1 * (10**fee) ||
                addressToLastTime[msg.sender] == 0 ||
                block.timestamp - addressToLastTime[msg.sender] > 1 days,
            "Not owner or incorrect money or not enough time"
        );
        uint256[] memory tokenIds = new uint[](K);
        string[] memory tokenURIs = new string[](K);

        for (uint256 i = 0; i < K; i++) {
            uint256 randNumber = (uint256(
                keccak256(abi.encodePacked(block.timestamp, i))
            ) % sumWeight) + 1;
            for (
                uint256 j = 0;
                j < userOwnedTokens[address(this)].length();
                j++
            ) {
                uint256 amount;
                uint256 tokenId;
                (tokenId, amount) = userOwnedTokens[address(this)].at(j);
                if (randNumber <= amount) {
                    // transfer token
                    if (userOwnedTokens[msg.sender].contains(tokenId)) {
                        userOwnedTokens[msg.sender].set(
                            tokenId,
                            userOwnedTokens[msg.sender].get(tokenId) + 1
                        );
                    } else {
                        userOwnedTokens[msg.sender].set(tokenId, 1);
                    }
                    if (userOwnedTokens[address(this)].get(tokenId) > 1) {
                        userOwnedTokens[address(this)].set(
                            tokenId,
                            userOwnedTokens[address(this)].get(tokenId) - 1
                        );
                    } else {
                        userOwnedTokens[address(this)].remove(tokenId);
                    }
                    _safeTransferFrom(
                        address(this),
                        msg.sender,
                        tokenId,
                        1,
                        bytes(uri(tokenId))
                    );
                    // record return values
                    tokenIds[i] = tokenId;
                    tokenURIs[i] = uri(tokenId);
                    // decrease sumWeight
                    sumWeight -= 1;
                    break;
                } else {
                    randNumber -= amount;
                }
            }
        }
        addressToLastTime[msg.sender] = block.timestamp;
        emit GetRandomKItemsEvent(tokenIds, tokenURIs);
    }

    function changeFee(uint256 _fee) public onlyOwner {
        fee = _fee;
    }

    function getBalance() public view returns (uint256) {
        return address(this).balance;
    }

    function withdraw(uint256 _amount) public onlyOwner {
        Owner.transfer(_amount);
    }
}
