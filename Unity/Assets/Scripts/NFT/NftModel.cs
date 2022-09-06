using UnityEngine;
using System.Linq;
using System.Numerics;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

[System.Serializable]
public class NftModel : NftObject
{
    public static Dictionary<string, NftModel> nftModelDic = new Dictionary<string, NftModel>();
    public Vector3Int? pos;
    public GameObject nftGameObject;

    public override JObject Serialize()
    {
        JObject res = new JObject();
        res["tokenID"] = tokenId.ToString();
        res["name"] = nftData.name;
        res["x"] = pos?.x;
        res["y"] = pos?.y;
        res["z"] = pos?.z;
        return res;
    }

    public override void DeSerialize(Nft nft)
    {
        nftData = nft;
        tokenId = nft.tokenId;
        nftKind = "model";

        if (!nftObjDic.ContainsKey(tokenId))
        {
            AssetBundle nftBundle = AssetBundle.LoadFromMemory(nft.imgData);
            //nftImage = nftBundle.LoadAllAssets<Texture>()[0];
            //nftBundle = AssetBundle.LoadFromMemory(nft.nftData);
            nftGameObject = GameObject.Instantiate(nftBundle.LoadAllAssets<GameObject>()[0]);
            nftGameObject.SetActive(false);
            nftObjDic.Add(tokenId, this);
        }
        else
        {
            var obj = nftObjDic[tokenId] as NftModel;
            nftImage = obj.nftImage;
            nftGameObject = GameObject.Instantiate(obj.nftGameObject);
            nftObjDic[tokenId].nftImage = null;
        }
        nftModelDic.Add(nft.name,this);
    }
}
