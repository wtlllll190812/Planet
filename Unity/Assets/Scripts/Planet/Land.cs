using UnityEngine;
using System.Linq;
using System.Numerics;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Vector3 = UnityEngine.Vector3;

public class Land :MonoBehaviour, IClickable
//地块类
{
    public Vector3Int pos;
    public Planet planet;
    public bool OnClick(Vector3 startPos,Vector3 activeData)
    {
        //var kind = UIManager.Instance.editorPanel.selectedObj?.nftKind;
        //if (kind == "land")
            GameManager.instance.EditLand(this, activeData);
        //else if (kind == "model")
            GameManager.instance.AddModel(this, activeData);
        return false;
    }
    //public void OnMouseDown()
    //{
    //    var kind = UIManager.Instance.editorPanel.selectedObj?.nftKind;
    //    if (kind == "land")
    //        GameManager.instance.EditLand(this, transform.up);
    //    else if (kind == "model")
    //        GameManager.instance.AddModel(this, transform.up);
    //}
    public Vector3Int GetPos(Vector3 dir)
    {
        Vector3 nextPos = planet.transform.InverseTransformDirection(dir);
        return new Vector3Int(pos.x + Mathf.RoundToInt(nextPos.x), pos.y + Mathf.RoundToInt(nextPos.y), pos.z + Mathf.RoundToInt(nextPos.z));
    }
}

/// <summary>
/// 地块类型
/// </summary>
[System.Serializable]
public class NftLandData : NftObject
{
    public static Dictionary<string, NftLandData> landDataDic=new Dictionary<string, NftLandData>();
    public string landKind;
    public Texture landTexture;

    public NftLandData() { }

    public NftLandData(string _landKind)
    {
        nftKind = "land";
        landKind = _landKind;
        landDataDic.Add(landKind,this);
    }

    public bool IsSurface()
    {
        return !(landKind == "empty" || landKind == "underground");
    }

    public override JObject Serialize()
    {
        JObject res = new JObject();
        res["tokenID"] = tokenId.ToString();
        if (landKind == "rock_land")
            res["landKind"] = "grass_land";
        else
            res["landKind"] = landKind;
        return res;
    }

    public override void DeSerialize(Nft nft)
    {
        Debug.Log(nft.name);
        tokenId = nft.tokenId;
        landKind = nft.name;
        nftData = nft;
        nftKind = "land";

        //Nft nft = BlockchainManager.instance.nfts[0].Select(x => x.tokenId = tokenId) as Nft;
        AssetBundle nftBundle = AssetBundle.LoadFromMemory(nft.nftData);
        landTexture = nftBundle.LoadAllAssets<Texture>()[0];
        //AssetBundle nftBundle2 = AssetBundle.LoadFromMemory(nft.nftData);
        //landTexture = nftBundle2.LoadAllAssets<Texture>()[0];
        nftObjList.Add(this);
        Debug.Log(nftObjList[0]);
        landDataDic.Add(landKind,this);
    }
}