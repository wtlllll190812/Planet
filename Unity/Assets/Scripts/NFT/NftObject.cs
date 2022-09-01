using UnityEngine;
using System.Linq;
using System.Numerics;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public abstract class NftObject
{
    public static Dictionary<BigInteger, NftObject> nftObjDic=new Dictionary<BigInteger, NftObject>();

    public Texture nftImage;
    public BigInteger tokenId;

    public abstract JObject Serialize();

    public abstract void DeSerialize(JObject jobj);
}