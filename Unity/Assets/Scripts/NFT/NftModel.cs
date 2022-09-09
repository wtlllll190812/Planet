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
    public Vector3Int? rot;
    public GameObject nftGameObject;

    public override JObject Serialize()
    {
        JObject res = new JObject();
        res["tokenID"] = tokenId.ToString();
        res["name"] = nftData.name;
        res["px"] = pos?.x;
        res["py"] = pos?.y;
        res["pz"] = pos?.z;
        res["rotx"]=rot?.x;
        res["roty"]=rot?.y;
        res["rotz"]=rot?.z;
        return res;
    }

    public override void DeSerialize(Nft nft)
    {
        nftData = nft;
        tokenId = nft.tokenId;
        nftKind = "model";

        AssetBundle nftBundle = AssetBundle.LoadFromMemory(nft.imgData);
        //nftImage = nftBundle.LoadAllAssets<Texture>()[0];
        //nftBundle = AssetBundle.LoadFromMemory(nft.nftData);
        nftGameObject = GameObject.Instantiate(nftBundle.LoadAllAssets<GameObject>()[0]);
        var model=nftGameObject.AddComponent<NftHandler>();
        nftGameObject.name = nft.name;
        nftGameObject.SetActive(false);
        nftObjList.Add(this);
        nftModelDic.Add(nft.name,this);
        model.nftModel=this;
    }
}
