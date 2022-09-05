using UnityEngine;
using System.Linq;
using System.Numerics;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[System.Serializable]
public abstract class NftObject:MonoBehaviour
{
    public static Dictionary<BigInteger, NftObject> nftObjDic=new Dictionary<BigInteger, NftObject>();

    public Nft nftData;
    public Texture nftImage;
    public BigInteger tokenId;
    public string nftKind;

    public abstract JObject Serialize();

    public abstract void DeSerialize(Nft nft);
}