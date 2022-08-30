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
                Params = new { owner = "0xd754a613C08cA611AF1408fBed1FB793aF733b07" }
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
}