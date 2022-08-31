using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using MoralisUnity;
using MoralisUnity.Platform.Objects;
using MoralisUnity.Web3Api.Client;
using MoralisUnity.Web3Api.Core;
using MoralisUnity.Web3Api.Core.Models;
using MoralisUnity.Web3Api.Models;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Encoders;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

[System.Serializable]
public class Commodity {
    public BigInteger itemId;
    public BigInteger tokenId;
    public string owner;
    public BigInteger price;
    public BigInteger amount;
}

[System.Serializable]
public class Token {
    public BigInteger tokenId;
    public string tokenURI;
    public BigInteger amount;
}

public class BlockchainManager : MonoBehaviour {
    public static BlockchainManager instance;

    public string contractAddress;
    public string abi;

    public string addressStr = null;
    public string balanceStr = null;
    
    // event callback
    

    // Start is called before the first frame update
    void Start() { }

    private void Awake() {
        instance = this;
    }

    // Update is called once per frame
    void Update() { }

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

    // GetNFT
    [Button("GetNFT")]
    public async Task<List<Token>> GetNFT() {
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
    public async Task<List<Commodity>> GetUnsoldItems() {
        // Function ABI input parameters
        object[] inputParams = new object[0];
        // Function ABI Output parameters
        object[] outputParams = new object[1];
        outputParams[0] = new {
            internalType = "struct Planet.Commodity[]", name = "", type = "tuple[]",
            components = new[] {
                new { internalType = "uint256", name = "itemId", type = "uint256" },
                new { internalType = "uint256", name = "tokenId", type = "uint256" },
                new { internalType = "address payable", name = "owner", type = "address" },
                new { internalType = "uint256", name = "price", type = "uint256" },
                new { internalType = "uint256", name = "amount", type = "uint256" },
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
        List<Commodity> commodities = new List<Commodity>();
        int i = 0;
        foreach (JToken c in res) {
            Commodity commodity = new Commodity();
            JArray ca = c as JArray;
            commodity.itemId = BigInteger.Parse(ca?[0].ToString());
            commodity.tokenId = BigInteger.Parse(ca?[1].ToString());
            commodity.owner = ca?[2].ToString();
            commodity.price = BigInteger.Parse(ca?[3].ToString());
            commodity.amount = BigInteger.Parse(ca?[4].ToString());
            commodities.Add(commodity);
            i++;
        }

        return commodities;
    }

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

    [Button("TransferNFT")]
    public async void TransferNFT(string from, string to, BigInteger tokenId, BigInteger amount) {
        object[] args = {
            from, to, tokenId, amount
        };

        string res = await ExecuteContractFunction("TransferNFT", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
        }
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
        }
    }

    [Button("changeFee")]
    public async void changeFee(BigInteger _fee) {
        object[] args = {
            _fee
        };

        string res = await ExecuteContractFunction("changeFee", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
        }
    }

    [Button("withdraw")]
    public async void withdraw(BigInteger _amount) {
        object[] args = {
            _amount
        };

        string res = await ExecuteContractFunction("withdraw", args, new HexBigInteger("0x0"));

        if (res == null) {
            Debug.LogError("Transaction Fail!");
        }
        else {
            Debug.Log($"Transaction Success! Transaction: {res}");
        }
    }

    [Button("GetRandomKItems")]
    public async Task<List<Token>> GetRandomKItems(BigInteger value) {
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
        List<Token> tokens = new List<Token>();
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
                Token t = new Token();
                t.tokenId = tokenIDs[i];
                t.tokenURI = tokenURIs[i];
                t.amount = 1;
                tokens.Add(t);
            }
            else {
                tokens[existIndex].amount += 1;
            }
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
}