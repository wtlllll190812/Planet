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
        res["px"] = pos?.x;
        res["py"] = pos?.y;
        res["pz"] = pos?.z;
        return res;
    }

    public override void DeSerialize(Nft nft)
    {
        nftData = nft;
        tokenId = nft.tokenId;
        nftKind = "model";

        if (!nftModelDic.ContainsKey(nft.name))
        {
            AssetBundle nftBundle = AssetBundle.LoadFromMemory(nft.imgData);
            nftImage = nftBundle.LoadAllAssets<Sprite>()[0];

            AssetBundle nftBundle2 = AssetBundle.LoadFromMemory(nft.nftData);
            nftGameObject = GameObject.Instantiate(nftBundle2.LoadAllAssets<GameObject>()[0]);

            var model = nftGameObject.AddComponent<NftHandler>();
            nftGameObject.AddComponent<BoxCollider>();
            nftGameObject.name = nft.name;
            nftGameObject.SetActive(false);
            nftObjList.Add(this);
            nftModelDic.Add(nft.name, this);
            model.nftModel = this;
        }
    }
}
