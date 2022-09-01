using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using UnityEngine;
using MoralisUnity;
using System.Numerics;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using Nethereum.Hex.HexTypes;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using MoralisUnity.Web3Api.Core;
using System.Collections.Generic;
using MoralisUnity.Web3Api.Client;
using MoralisUnity.Web3Api.Models;
using UnityEngine.SceneManagement;
using MoralisUnity.Platform.Objects;
using System.Text.RegularExpressions;
using MoralisUnity.Web3Api.Core.Models;
using Org.BouncyCastle.Utilities.Encoders;



// Token(Legacy)
[System.Serializable]
public class Token {
    public BigInteger tokenId;
    public string tokenURI;
    public BigInteger amount;
}

[System.Serializable]
public class Commodity {
    public BigInteger itemId;
    public BigInteger tokenId;
    public string tokenURI;
    public string owner;
    public BigInteger price;
    public BigInteger amount;
}

[System.Serializable]
public class NftMetaData {
    public string name;
    public string description;
    public string nftUrl;
    public string imgUrl;
}

[System.Serializable]
public class Nft {
    public string name;
    public string description;
    public BigInteger tokenId;
    public BigInteger amount;
    public byte[] nftData;
    public byte[] imgData;
}

public class BlockchainManager : MonoBehaviour {
    public static BlockchainManager instance;

    public string contractAddress;
    public string abi;

    public string addressStr = null;
    public string balanceStr = null;

    public List<Nft> nfts;
    public List<Commodity> commodities;
    public List<Commodity> myCommodities;

    // Start is called before the first frame update
    void Start() { }

    private async void Awake() {
        instance = this;
    }

    // Update is called once per frame
    void Update() { }

    # region Contract

    public async void UpdateAddressOnConnect() {
        if (MoralisState.Initialized.Equals(Moralis.State)) {
            MoralisUser user = await Moralis.GetUserAsync();

            if (user == null) {
                addressStr = null;
                // User is null so go back to the authentication scene.
                SceneManager.LoadScene(0);
            }

            // Set User's wallet address.
            addressStr = FormatUserAddressForDisplay(user?.ethAddress);
        }
    }

    public void UpdateAddressBalanceOnDisconnect() {
        addressStr = null;
        balanceStr = null;
    }

    public async void UpdateBalance() {
        if (MoralisState.Initialized.Equals(Moralis.State)) {
            MoralisUser user = await Moralis.GetUserAsync();

            // Retrienve the user's native balance;
            NativeBalance balanceResponse =
                await Moralis.Web3Api.Account.GetNativeBalance(user?.ethAddress,
                    Moralis.CurrentChain.EnumValue);

            double balance = 0.0;
            float decimals = Moralis.CurrentChain.Decimals * 1.0f;
            string sym = Moralis.CurrentChain.Symbol;

            // Make sure a response to the balanace request weas received. The 
            // IsNullOrWhitespace check may not be necessary ...
            if (balanceResponse != null && !string.IsNullOrWhiteSpace(balanceResponse.Balance)) {
                double.TryParse(balanceResponse.Balance, out balance);
            }

            // Display native token amount token in fractions of token.
            // NOTE: May be better to link this to chain since some tokens may have
            // more than 18 sigjnificant figures.
            balanceStr = string.Format("{0:0.####} {1}",
                (balance / (double)Mathf.Pow(10.0f, decimals)), sym);
        }
    }

    private string FormatUserAddressForDisplay(string addr) {
        string resp = addr;

        if (resp.Length > 13) {
            resp = string.Format("{0}...{1}", resp.Substring(0, 6),
                resp.Substring(resp.Length - 4, 4));
        }

        return resp;
    }

    // uri
    public async Task<string> uri(BigInteger tokenID) {
        // Function ABI input parameters
        object[] inputParams = new object[1];
        inputParams[0] = new { internalType = "uint256", name = "tokenId", type = "uint256" };
        // Function ABI Output parameters
        object[] outputParams = new object[1];
        outputParams[0] = new { internalType = "string", name = "", type = "string" };
        // Function ABI
        object[] abi = new object[1];
        abi[0] = new {
            inputs = inputParams, name = "uri", outputs = outputParams, stateMutability = "view",
            type = "function"
        };
        // Define request object
        RunContractDto rcd = new RunContractDto() {
            Abi = abi,
            Params = new {
                tokenId = tokenID
            }
        };
        // resp: tx hash
        JToken resp =
            await Moralis.Web3Api.Native.RunContractFunctionOrigin(contractAddress, "uri", rcd,
                ChainList.ropsten);

        return resp.ToString();
    }

    // GetNFT (legacy)
    [Button("GetNFT")]
    public async Task<List<Token>> GetNFTFromContract() {
        try {
            MoralisUser user = await Moralis.GetUserAsync();
            // Function ABI input parameters
            object[] inputParams = new object[1];
            inputParams[0] = new { internalType = "address", name = "owner", type = "address" };
            // Function ABI Output parameters
            object[] outputParams = new object[3];
            outputParams[0] = new { internalType = "uint256[]", name = "", type = "uint256[]" };
            outputParams[1] = new { internalType = "string[]", name = "", type = "string[]" };
            outputParams[2] = new { internalType = "uint256[]", name = "", type = "uint256[]" };
            // Function ABI
            object[] abi = new object[1];
            abi[0] = new {
                inputs = inputParams, name = "GetNFT", outputs = outputParams, stateMutability = "view",
                type = "function"
            };
            // Define request object
            RunContractDto rcd = new RunContractDto() {
                Abi = abi,
                Params = new { owner = user.ethAddress }
            };
            // resp: unparsed json {"result":{"0":["4","6","2"],"1":["T4","T6","T2"],"2":["2","1","2"]}}
            JToken resp =
                await Moralis.Web3Api.Native.RunContractFunctionOrigin(contractAddress, "GetNFT", rcd,
                    ChainList.ropsten);
            JArray tokenIDsJ = (resp as JObject)?["0"] as JArray;
            JArray tokenURIsJ = (resp as JObject)?["1"] as JArray;
            JArray amountsJ = (resp as JObject)?["2"] as JArray;

            List<BigInteger> tokenIDs =
                JsonConvert.DeserializeObject<List<BigInteger>>(tokenIDsJ.ToString());
            List<string> tokenURIs = JsonConvert.DeserializeObject<List<string>>(tokenURIsJ.ToString());
            List<BigInteger> amounts = JsonConvert.DeserializeObject<List<BigInteger>>(amountsJ.ToString());

            int length = tokenIDs.Count;
            List<Token> tokens = new List<Token>();
            for (int i = 0; i < length; i++) {
                Token token = new Token();
                token.tokenId = tokenIDs[i];
                token.tokenURI = tokenURIs[i];
                token.amount = amounts[i];
                tokens.Add(token);
            }

            return tokens;
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }

        return null;
    }

    // Get Balance of the Contract
    [Button("GetContractBalance")]
    public async Task<BigInteger> GetContractBalance() {
        // Function ABI input parameters
        object[] inputParams = new object[0];
        // Function ABI Output parameters
        object[] outputParams = new object[1];
        outputParams[0] = new { internalType = "uint256", name = "", type = "uint256" };
        // Function ABI
        object[] abi = new object[1];
        abi[0] = new {
            inputs = inputParams, name = "getBalance", outputs = outputParams, stateMutability = "view",
            type = "function"
        };
        // Define request object
        RunContractDto rcd = new RunContractDto() {
            Abi = abi,
            Params = new { }
        };
        // resp: tx hash
        JToken resp =
            await Moralis.Web3Api.Native.RunContractFunctionOrigin(contractAddress, "getBalance", rcd,
                ChainList.ropsten);

        BigInteger balance = BigInteger.Zero;
        BigInteger.TryParse(resp.ToString(), out balance);
        return balance;
    }

    // Get Unsold Items
    [Button("GetUnsoldItems")]
    public async void GetUnsoldItems() {
        // Function ABI input parameters
        object[] inputParams = new object[0];
        // Function ABI Output parameters
        object[] outputParams = new object[1];
        outputParams[0] = new {
            internalType = "struct Planet.Commodity[]", name = "", type = "tuple[]",
            components = new[] {
                new { internalType = "uint256", name = "itemId", type = "uint256" },
                new { internalType = "uint256", name = "tokenId", type = "uint256" },
                new { internalType = "string", name = "tokenURI", type = "string" },
                new { internalType = "address payable", name = "owner", type = "address" },
                new { internalType = "uint256", name = "price", type = "uint256" },
                new { internalType = "uint256", name = "amount", type = "uint256" }
            }
        };
        // Function ABI
        object[] abi = new object[1];
        abi[0] = new {
            inputs = inputParams, name = "GetUnsoldItems", outputs = outputParams, stateMutability = "view",
            type = "function"
        };
        // Define request object
        RunContractDto rcd = new RunContractDto() {
            Abi = abi,
            Params = new { }
        };
        // resp: tx hash
        JToken res =
            await Moralis.Web3Api.Native.RunContractFunctionOrigin(contractAddress, "GetUnsoldItems", rcd,
                ChainList.ropsten);
        commodities = new List<Commodity>();
        int i = 0;
        foreach (JToken c in res) {
            Commodity commodity = new Commodity();
            JArray ca = c as JArray;
            commodity.itemId = BigInteger.Parse(ca?[0].ToString());
            commodity.tokenId = BigInteger.Parse(ca?[1].ToString());
            commodity.tokenURI = ca?[2].ToString();
            commodity.owner = ca?[3].ToString();
            commodity.price = BigInteger.Parse(ca?[4].ToString());
            commodity.amount = BigInteger.Parse(ca?[5].ToString());
            commodities.Add(commodity);
            i++;
        }
    }

    // Get One's Commodities
    [Button("GetCommoditiesByAddress")]
    public async Task<List<Commodity>> GetCommoditiesByAddress(string address) {
        // Function ABI input parameters
        object[] inputParams = new object[1];
        inputParams[0] = new {
            internalType = "address", name = "owner", type = "address"
        };
        // Function ABI Output parameters
        object[] outputParams = new object[1];
        outputParams[0] = new {
            internalType = "struct Planet.Commodity[]", name = "", type = "tuple[]",
            components = new[] {
                new { internalType = "uint256", name = "itemId", type = "uint256" },
                new { internalType = "uint256", name = "tokenId", type = "uint256" },
                new { internalType = "string", name = "tokenURI", type = "string" },
                new { internalType = "address payable", name = "owner", type = "address" },
                new { internalType = "uint256", name = "price", type = "uint256" },
                new { internalType = "uint256", name = "amount", type = "uint256" }
            }
        };
        // Function ABI
        object[] abi = new object[1];
        abi[0] = new {
            inputs = inputParams, name = "GetCommoditiesByAddress", outputs = outputParams,
            stateMutability = "view",
            type = "function"
        };
        // Define request object
        RunContractDto rcd = new RunContractDto() {
            Abi = abi,
            Params = new {
                owner = address
            }
        };
        // resp: tx hash
        JToken res =
            await Moralis.Web3Api.Native.RunContractFunctionOrigin(contractAddress, "GetCommoditiesByAddress",
                rcd,
                ChainList.ropsten);
        List<Commodity> commodities = new List<Commodity>();
        int i = 0;
        foreach (JToken c in res) {
            Commodity commodity = new Commodity();
            JArray ca = c as JArray;
            commodity.itemId = BigInteger.Parse(ca?[0].ToString());
            commodity.tokenId = BigInteger.Parse(ca?[1].ToString());
            commodity.tokenURI = ca?[2].ToString();
            commodity.owner = ca?[3].ToString();
            commodity.price = BigInteger.Parse(ca?[4].ToString());
            commodity.amount = BigInteger.Parse(ca?[5].ToString());
            commodities.Add(commodity);
            i++;
        }

        return commodities;
    }

    [Button("GetMyCommodities")]
    public async void GetMyCommodities() {
        myCommodities = await GetCommoditiesByAddress(addressStr);
    }

    // MintNFT
    [Button("MintNFT")]
    public async void MintNFT(List<string> tokenURIs, List<BigInteger> amounts) {
        object[] args = {
            tokenURIs, amounts
        };

        string res = await ExecuteContractFunction("MintNFT", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
        }
    }

    // TransferNFT (only current user -> others)
    [Button("TransferNFT")]
    public async void TransferNFT(string to, BigInteger tokenId, BigInteger amount) {
        object[] args = {
            addressStr, to, tokenId, amount
        };

        string res = await ExecuteContractFunction("TransferNFT", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
            return;
        }

        Debug.Log($"Transaction Success! Transaction: {res}");

        LoseToken(tokenId, amount);
    }

    [Button("AddItemToMarket")]
    public async void AddItemToMarket(BigInteger tokenId, BigInteger amount, BigInteger price) {
        object[] args = {
            tokenId, amount, price
        };

        string res = await ExecuteContractFunction("AddItemToMarket", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
            LoseToken(tokenId, amount);
            GetUnsoldItems();
        }
    }

    [Button("BuyItemAndTransferOwnership")]
    public async void BuyItemAndTransferOwnership(BigInteger itemId, BigInteger amount, BigInteger value) {
        object[] args = {
            itemId, amount
        };

        string res =
            await ExecuteContractFunction("BuyItemAndTransferOwnership", args, new HexBigInteger(value));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
            int index = ItemId2CommodityId(itemId);
            if (index != -1)
                ReceiveToken(commodities[index].tokenId, commodities[index].tokenURI, amount);
            GetUnsoldItems();
        }
    }

    [Button("RedeemItems")]
    public async void RedeemItems(BigInteger tokenId, BigInteger amount) {
        object[] args = {
            tokenId, amount
        };

        string res = await ExecuteContractFunction("RedeemItems", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
            ReceiveToken(tokenId, await uri(tokenId), amount);
            GetUnsoldItems();
        }
    }

    [Button("changeFee")]
    public async void changeFee(BigInteger fee) {
        object[] args = {
            fee
        };

        string res = await ExecuteContractFunction("changeFee", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
        }
    }
    
    [Button("changeK")]
    public async void changeK(BigInteger K) {
        object[] args = {
            K
        };

        string res = await ExecuteContractFunction("changeK", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
        }
    }

    [Button("withdraw")]
    public async void withdraw(BigInteger amount) {
        object[] args = {
            amount
        };

        string res = await ExecuteContractFunction("withdraw", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
        }
    }

    // GetRandomKItems
    [Button("GetRandomKItems")]
    public async Task<List<Nft>> GetRandomKItems(BigInteger value) {
        object[] args = { };

        string res =
            await ExecuteContractFunction("GetRandomKItems", args, new HexBigInteger(value));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }

        Debug.Log($"Transaction Success! Transaction: {res}");
        await Task.Delay(10000);
        int time = 10;
        // Get Transaction
        BlockTransaction txn = await Moralis.Web3Api.Native.GetTransaction(res, ChainList.ropsten);
        while (txn == null) {
            await Task.Delay(10000);
            time += 10;
            txn = await Moralis.Web3Api.Native.GetTransaction(res, ChainList.ropsten);
            Debug.Log(time);
            if (time >= 90) {
                Debug.LogError("Update to blockchain fail!");
                return null;
            }
        }

        int blockNumber = Int32.Parse(txn.BlockNumber);
        string txnHash = txn.Logs.Last().TransactionHash;

        // Get Contract Event
        object abi = new {
            anonymous = false, name = "GetRandomKItemEvent", type = "event",
            inputs = new[] {
                new {
                    indexed = false, internalType = "uint256[]", name = "tokenIds", type = "uint256[]"
                },
                new { indexed = false, internalType = "string[]", name = "tokenURIs", type = "string[]" }
            }
        };
        List<LogEvent> eventList = await Moralis.Web3Api.Native.GetContractEvents(contractAddress,
            "0x7c416904aa25bb5bcade5e79dc30bae7916ce773ef5e89c2e2145542933d3a96",
            abi, ChainList.ropsten, null, null, blockNumber, blockNumber);

        // Get Tokens
        JObject data = new JObject();
        foreach (var e in eventList) {
            if (e.TransactionHash == txnHash) {
                data = (JObject)e.Data;
                break;
            }
        }

        // Parse Token
        List<Nft> tokens = new List<Nft>();
        JArray tokenIDsJ = data?["tokenIds"] as JArray;
        JArray tokenURIsJ = data?["tokenURIs"] as JArray;
        List<BigInteger> tokenIDs = JsonConvert.DeserializeObject<List<BigInteger>>(tokenIDsJ.ToString());
        List<string> tokenURIs = JsonConvert.DeserializeObject<List<string>>(tokenURIsJ.ToString());
        int count = tokenIDs.Count;
        for (int i = 0; i < count; i++) {
            int existIndex = -1;
            for (int j = 0; j < tokens.Count; j++) {
                if (tokens[j].tokenId == tokenIDs[i]) {
                    existIndex = j;
                    break;
                }
            }

            if (existIndex == -1) {
                Nft nft = new Nft();
                nft.tokenId = tokenIDs[i];
                nft.amount = 1;
                string tokenURI = tokenURIs[i];
                WebClient MyWebClient = new WebClient();
                byte[] metaData = MyWebClient.DownloadData(tokenURI);
                string metaDataString = Encoding.UTF8.GetString(metaData);
                NftMetaData metaDataObject = JsonConvert.DeserializeObject<NftMetaData>(metaDataString);
                nft.name = metaDataObject.name;
                nft.description = metaDataObject.description;
                // The format of data may have problems...
                nft.nftData = MyWebClient.DownloadData(metaDataObject.nftUrl);
                nft.imgData = MyWebClient.DownloadData(metaDataObject.imgUrl);
                tokens.Add(nft);
            }
            else {
                tokens[existIndex].amount += 1;
            }

            ReceiveToken(tokenIDs[i], tokenURIs[i], 1);
        }

        return tokens;
    }

    private async Task<string> ExecuteContractFunction(string functionName, object[] args,
        HexBigInteger value) {
        // Set gas estimate
        HexBigInteger gas = new HexBigInteger(0);
        HexBigInteger gasPrice = new HexBigInteger("0x0");

        string res =
            await Moralis.ExecuteContractFunction(contractAddress, abi, functionName, args, value, gas,
                gasPrice);

        return res;
    }

    #endregion

    #region IPFS

    [Button("UploadToIpfs")]
    public async Task<string> UploadToIpfs(string nftName, string nftDesc, string nftPath, string imgPath) {
        // Get data of NFT material
        FileStream fs = new FileStream(nftPath, FileMode.Open, FileAccess.Read);
        byte[] nftData = new byte[fs.Length];
        fs.Read(nftData, 0, nftData.Length);
        fs.Close();

        fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
        byte[] imgData = new byte[fs.Length];
        fs.Read(imgData, 0, imgData.Length);
        fs.Close();

        // We are replacing any space for an empty
        string filteredName = Regex.Replace(nftName, @"\s", "");
        string ipfsNFTDataPath = await SaveNftDataToIpfs(filteredName, nftData);
        string ipfsImgPath = await SaveNftDataToIpfs(filteredName + "_img", imgData);

        if (string.IsNullOrEmpty(ipfsNFTDataPath) || string.IsNullOrEmpty(ipfsImgPath)) {
            Debug.LogError("Failed to save NFT data or img data to IPFS");
            return null;
        }

        Debug.Log("NFT data file saved successfully to IPFS:");
        Debug.Log(ipfsNFTDataPath);
        Debug.Log("NFT img file saved successfully to IPFS:");
        Debug.Log(ipfsImgPath);

        // Build Metadata
        object metadata = BuildMetadata(nftName, nftDesc, ipfsNFTDataPath, ipfsImgPath);
        string dateTime = DateTime.Now.Ticks.ToString();

        string metadataName = $"{filteredName}" + $"_{dateTime}" + ".json";

        // Store metadata to IPFS
        string json = JsonConvert.SerializeObject(metadata);
        string base64Data = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        string ipfsMetadataPath = await SaveToIpfs(metadataName, base64Data);

        if (ipfsMetadataPath == null) {
            Debug.LogError("Failed to save metadata to IPFS");
            return null;
        }

        Debug.Log("Metadata saved successfully to IPFS:");
        Debug.Log(ipfsMetadataPath);

        return ipfsMetadataPath;
    }

    private async UniTask<string> SaveToIpfs(string name, string data) {
        string pinPath = null;

        try {
            IpfsFileRequest request = new IpfsFileRequest() {
                Path = name,
                Content = data
            };

            List<IpfsFileRequest> requests = new List<IpfsFileRequest> { request };
            List<IpfsFile> resp = await Moralis.GetClient().Web3Api.Storage.UploadFolder(requests);

            IpfsFile ipfs = resp.FirstOrDefault<IpfsFile>();

            if (ipfs != null) {
                pinPath = ipfs.Path;
            }
        }
        catch (Exception exp) {
            Debug.LogError($"IPFS Save failed: {exp.Message}");
        }

        return pinPath;
    }

    private async UniTask<string> SaveNftDataToIpfs(string name, byte[] imageData) {
        return await SaveToIpfs(name, Convert.ToBase64String(imageData));
    }

    private object BuildMetadata(string name, string desc, string nftUrl, string imgUrl) {
        object metadataObj = new {
            name = name,
            description = desc,
            imgUrl = imgUrl,
            nftUrl = nftUrl
        };

        return metadataObj;
    }

    public async void GetNFT(string address) {
        try {

            NftOwnerCollection noc =
                await Moralis.GetClient().Web3Api.Account.GetNFTsForContract(address.ToLower(),
                    contractAddress,
                    ChainList.ropsten);

            List<NftOwner> nftOwners = noc.Result;

            // We only proceed if we find some
            if (!nftOwners.Any()) {
                Debug.Log("You don't own any NFT");
                return;
            }

            nfts = new List<Nft>();
            WebClient MyWebClient = new WebClient();
            MyWebClient.Credentials = CredentialCache.DefaultCredentials; //获取或设置用于向Internet资源的请求进行身份验证的网络凭据
            foreach (var nftOwner in nftOwners) {
                Nft nft = new Nft();
                nft.tokenId = BigInteger.Parse(nftOwner.TokenId);
                nft.amount = BigInteger.Parse(nftOwner.Amount);
                byte[] metaData;
                try {
                    metaData = MyWebClient.DownloadData(nftOwner.TokenUri);
                }
                catch (Exception) {
                    continue;
                }

                string metaDataString = Encoding.UTF8.GetString(metaData);
                NftMetaData metaDataObject = JsonConvert.DeserializeObject<NftMetaData>(metaDataString);
                if (metaDataObject == null) continue;
                nft.name = metaDataObject.name;
                nft.description = metaDataObject.description;
                // The format of data may have problems...
                nft.nftData = MyWebClient.DownloadData(metaDataObject.nftUrl);
                nft.imgData = MyWebClient.DownloadData(metaDataObject.imgUrl);
                nfts.Add(nft);
            }
        }
        catch (Exception exp) {
            Debug.LogError(exp.Message);
        }
    }

    public async void GetMyNFT() {
        MoralisUser user=await Moralis.GetUserAsync();
        GetNFT(user.ethAddress);
    }

    #endregion

    #region Utils

    void LoseToken(BigInteger tokenId, BigInteger amount) {
        for (int i = 0; i < nfts.Count; i++) {
            if (nfts[i].tokenId == tokenId) {
                nfts[i].amount -= amount;
                if (nfts[i].amount == 0) {
                    nfts.RemoveAt(i);
                }

                return;
            }
        }
    }

    void ReceiveToken(BigInteger tokenId, string tokenURI, BigInteger amount) {
        for (int i = 0; i < nfts.Count; i++) {
            if (nfts[i].tokenId == tokenId) {
                nfts[i].amount += amount;
                return;
            }
        }

        // Add New NFT
        WebClient MyWebClient = new WebClient();
        Nft nft = new Nft();
        nft.tokenId = tokenId;
        nft.amount = amount;
        byte[] metaData = MyWebClient.DownloadData(tokenURI);
        string metaDataString = Encoding.UTF8.GetString(metaData);
        NftMetaData metaDataObject = JsonConvert.DeserializeObject<NftMetaData>(metaDataString);
        nft.name = metaDataObject.name;
        nft.description = metaDataObject.description;
        // The format of data may have problems...
        nft.nftData = MyWebClient.DownloadData(metaDataObject.nftUrl);
        nft.imgData = MyWebClient.DownloadData(metaDataObject.imgUrl);
        nfts.Add(nft);
    }

    int ItemId2CommodityId(BigInteger itemId) {
        for (int i = 0; i < commodities.Count; i++) {
            if (commodities[i].itemId == itemId)
                return i;
        }

        return -1;
    }

    #endregion
}