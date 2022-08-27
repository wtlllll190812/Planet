// SPDX-License-Identifier: GPL-3.0

pragma solidity ^0.8.9;

import "@openzeppelin/contracts/token/ERC1155/extensions/ERC1155URIStorage.sol";
import "@openzeppelin/contracts/utils/Counters.sol";
import "@openzeppelin/contracts/utils/structs/EnumerableMap.sol";
import "@openzeppelin/contracts/utils/structs/EnumerableSet.sol";

contract Planet is ERC1155URIStorage {
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

    constructor(string memory uri_) ERC1155(uri_) {
        Owner = payable(msg.sender);
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
            _tokenIds.increment();
            uint256 newTokenId = _tokenIds.current();
            _mint(Owner, newTokenId, amounts[i], bytes(tokenURIs[i]));
            _setURI(newTokenId, tokenURIs[i]);
            userOwnedTokens[Owner].set(newTokenId, amounts[i]);
        }
    }

    function GetNFT(address owner)
        public
        view
        returns (uint256[] memory, uint256[] memory)
    {
        uint256 length = userOwnedTokens[owner].length();
        uint256[] memory tokenIds = new uint[](length);
        uint256[] memory amounts = new uint[](length);

        for (uint256 i = 0; i < length; i++) {
            (tokenIds[i], amounts[i]) = userOwnedTokens[owner].at(i);
        }

        return (tokenIds, amounts);
    }

    function TransferNFT(
        address from,
        address to,
        uint256 tokenId,
        uint256 amount
    ) public {
        require(amount >= 1, "Amount must be at least 1");
        safeTransferFrom(from, to, tokenId, amount, bytes(uri(tokenId)));
        userOwnedTokens[from].set(
            tokenId,
            userOwnedTokens[from].get(tokenId) - amount
        );
        userOwnedTokens[to].set(
            tokenId,
            userOwnedTokens[to].get(tokenId) + amount
        );
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
        // contract receive the token
        if (userOwnedTokens[address(this)].contains(tokenId)) {
            userOwnedTokens[address(this)].set(
                tokenId,
                userOwnedTokens[address(this)].get(tokenId) + amount
            );
        } else {
            userOwnedTokens[address(this)].set(tokenId, amount);
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

    function SellItemAndTransferOwnership(uint256 itemId, uint256 amount)
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
        if (userOwnedTokens[address(this)].get(tokenId) > amount) {
            userOwnedTokens[address(this)].set(
                tokenId,
                userOwnedTokens[address(this)].get(tokenId) - amount
            );
        } else {
            userOwnedTokens[address(this)].remove(tokenId);
        }
        safeTransferFrom(
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
        if (userOwnedTokens[address(this)].get(tokenId) > amount) {
            userOwnedTokens[address(this)].set(
                tokenId,
                userOwnedTokens[address(this)].get(tokenId) - amount
            );
        } else {
            userOwnedTokens[address(this)].remove(tokenId);
        }
        safeTransferFrom(
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

    function getBalance() public view onlyOwner returns (uint256) {
        return address(this).balance;
    }

    function withdraw(uint256 _amount) public onlyOwner {
        Owner.transfer(_amount);
    }
}
