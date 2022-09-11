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

    // Struct: Commodity
    struct Commodity {
        uint256 itemId;
        uint256 tokenId;
        string tokenURI;
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

    // sum of weight of all products
    uint256 private sumWeight = 0;
    
    // price to pay for a single speed-up: 1*(10**fee) Wei
    // initial: 1 ETH for ropsten
    uint256 public fee = 18;

    // amount of random material to get
    uint256 public K = 5;

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
        external
        onlyOwner
    {
        require(
            tokenURIs.length == amounts.length,
            "Length Mismatch"
        );
        for (uint256 i = 0; i < tokenURIs.length; i++) {
            require(amounts[i] >= 1, "Zero Amount");
            _tokenIds.increment();
            uint256 newTokenId = _tokenIds.current();
            _mint(address(this), newTokenId, amounts[i], bytes(tokenURIs[i]));
            _setURI(newTokenId, tokenURIs[i]);
            userOwnedTokens[address(this)].set(newTokenId, amounts[i]);
            sumWeight += amounts[i];
        }
    }

    function GetNFT(address owner)
        external
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
    ) external {
        require(amount >= 1, "Zero Amount");
        safeTransferFrom(from, to, tokenId, amount, bytes(uri(tokenId)));
        // Check whether `from' has tokenId left
        LoseToken(from, tokenId, amount);
        // Check whether `to' initially has tokenId
        GetToken(to, tokenId, amount);
        // return to this address
        if (to == address(this)) {
            sumWeight += amount;
        }
    }

    function AddItemToMarket(
        uint256 tokenId,
        uint256 amount,
        uint256 price
    ) external {
        require(amount >= 1, "Zero Amount");
        require(
            balanceOf(msg.sender, tokenId) >= amount,
            "Insufficient Balance"
        );
        // add token to the market
        _itemIds.increment();
        uint256 itemId = _itemIds.current();
        idToCommodity[itemId] = Commodity(
            itemId,
            tokenId,
            uri(tokenId),
            payable(msg.sender),
            price,
            amount
        );
        commodityIds.add(itemId);
        // msg.sender lose the token
        LoseToken(msg.sender, tokenId, amount);
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
        external
        payable
    {
        require(
            commodityIds.contains(itemId),
            "No Exist"
        );
        require(
            msg.sender != idToCommodity[itemId].owner,
            "Buy Oneself"
        );
        require(amount >= 1, "Zero Amount");
        uint256 tokenId = idToCommodity[itemId].tokenId; // commodity tokenId
        uint256 price = idToCommodity[itemId].price; // commodity price
        uint256 tokenAmount = idToCommodity[itemId].amount; // commodity amount
        require(tokenAmount >= amount, "Exceed Supply");
        require(msg.value == price * amount, "Incorrect money");

        // market decrease supply
        DecreaseSupply(tokenAmount, amount, itemId);
        // transfer token
        GetToken(msg.sender, tokenId, amount);
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

    function RedeemItems(uint256 itemId, uint256 amount) external {
        require(amount >= 1, "Zero Amount");
        require(
            commodityIds.contains(itemId),
            "No Exist"
        );
        require(
            msg.sender == idToCommodity[itemId].owner,
            "Redeem Yourself"
        );
        uint256 tokenId = idToCommodity[itemId].tokenId; // commodity tokenId
        uint256 tokenAmount = idToCommodity[itemId].amount; // commodity amount
        require(tokenAmount >= amount, "Exceed Supply");

        // market decrease supply
        DecreaseSupply(tokenAmount, amount, itemId);
        // transfer token
        GetToken(msg.sender, tokenId, amount);
        _safeTransferFrom(
            address(this),
            msg.sender,
            tokenId,
            amount,
            bytes(uri(tokenId))
        );
    }

    function GetUnsoldItems() external view returns (Commodity[] memory) {
        uint256 length = commodityIds.length();
        Commodity[] memory commodities = new Commodity[](length);
        for (uint256 i = 0; i < length; i++) {
            commodities[i] = idToCommodity[commodityIds.at(i)];
        }
        return commodities;
    }

    function GetCommoditiesByAddress(address owner)
        external
        view
        returns (Commodity[] memory)
    {
        uint256 num = 0;
        uint256 length = commodityIds.length();
        uint256[] memory indexes = new uint256[](length);
        for (uint256 i = 0; i < length; i++) {
            if (idToCommodity[commodityIds.at(i)].owner == owner) {
                indexes[num++] = i;
            }
        }
        Commodity[] memory commodities = new Commodity[](num);
        for (uint256 i = 0; i < num; i++) {
            commodities[i] = idToCommodity[commodityIds.at(indexes[i])];
        }
        return commodities;
    }

    function GetRandomKItems() external payable {
        require(
            msg.sender == Owner ||
                msg.value >= 1 * (10**fee) ||
                addressToLastTime[msg.sender] == 0 ||
                block.timestamp - addressToLastTime[msg.sender] > 1 days,
            "No Owner No Money No Time"
        );
        uint256 num;
        if (msg.value != 0) {
            num = msg.value / (1 * (10 ** fee)) * K;
        } else {
            num = K;
        }
        require(sumWeight >= num, "Insufficient Pool");
        uint256[] memory tokenIds = new uint[](num);
        string[] memory tokenURIs = new string[](num);

        for (uint256 i = 0; i < num; i++) {
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
                    GetToken(msg.sender, tokenId, 1);
                    LoseToken(address(this), tokenId, 1);
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

    function changeFee(uint256 f) external onlyOwner {
        fee = f;
    }

    function changeK(uint256 k) external onlyOwner {
        K = k;
    }

    function getBalance() external view returns (uint256) {
        return address(this).balance;
    }

    function withdraw(uint256 _amount) external onlyOwner {
        Owner.transfer(_amount);
    }

    function LoseToken(address owner, uint256 tokenId, uint256 amount) internal {
        uint256 newAmount = userOwnedTokens[owner].get(tokenId) - amount;
        if (newAmount != 0) {
            userOwnedTokens[owner].set(
                tokenId,
                userOwnedTokens[owner].get(tokenId) - amount
            );
        } else {
            userOwnedTokens[owner].remove(tokenId);
        }
    }

    function GetToken(address owner, uint256 tokenId, uint256 amount) internal {
        if (userOwnedTokens[owner].contains(tokenId)) {
            userOwnedTokens[owner].set(
                tokenId,
                userOwnedTokens[owner].get(tokenId) + amount
            );
        } else {
            userOwnedTokens[owner].set(tokenId, amount);
        }
    }

    function DecreaseSupply(uint256 tokenAmount, uint256 amount, uint256 itemId) internal {
        if (tokenAmount > amount) {
            idToCommodity[itemId].amount = tokenAmount - amount;
        } else {
            commodityIds.remove(itemId);
        }
    }

    function LastGetTime() external view returns (uint256) {
        return addressToLastTime[msg.sender];
    }
}
