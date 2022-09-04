using UnityEngine;
using System.Linq;
using System.Numerics;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class NftModel : NftObject
{
    public static Dictionary<string, NftModel> nftModelDic = new Dictionary<string, NftModel>();
    public Vector3Int pos;
    public GameObject nftGameObject;

    public override JObject Serialize()
    {
        JObject res = new JObject();
        res["tokenID"] = tokenId.ToString();
        res["x"] = pos.x;
        res["y"] = pos.y;
        res["z"] = pos.z;
        return res;
    }

    public override void DeSerialize(Nft nft)
    {
        pos = new Vector3Int();
        tokenId = BigInteger.Parse(jobj["tokenID"].ToString());
        pos.x = int.Parse(jobj["x"].ToString());
        pos.y = int.Parse(jobj["x"].ToString());
        pos.z = int.Parse(jobj["x"].ToString());

        if (!nftObjDic.ContainsKey(tokenId))
        {
            Nft nft = BlockchainManager.instance.nfts.Select(x => x.tokenId = tokenId) as Nft;

            AssetBundle nftBundle = AssetBundle.LoadFromMemory(nft.imgData);
            nftImage = nftBundle.LoadAllAssets<Texture>()[0];
            nftBundle = AssetBundle.LoadFromMemory(nft.nftData);
            nftGameObject = GameObject.Instantiate(nftBundle.LoadAllAssets<GameObject>()[0]);
            nftObjDic.Add(tokenId, this);
        }
        else
        {
            var obj = nftObjDic[tokenId] as NftModel;
            nftImage = obj.nftImage;
            nftGameObject = GameObject.Instantiate(obj.nftGameObject);
            nftObjDic[tokenId].nftImage = null;
        }
    }
}
