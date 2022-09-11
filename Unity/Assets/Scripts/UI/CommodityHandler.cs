using System;
using System.Net;
using System.Text;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;

public class CommodityHandler : MonoBehaviour
{
    public Commodity commodity;

    public TMPro.TextMeshProUGUI priceText;
    public TMPro.TextMeshProUGUI amountText;
    public Image itemImage;

    public async void SetCommodity(Commodity commodity)
    {
        this.commodity = commodity;
        try
        {
            WebClient MyWebClient = new WebClient();
            byte[] metaData;
            metaData = await MyWebClient.DownloadDataTaskAsync(commodity.tokenURI);
            string metaDataString = Encoding.UTF8.GetString(metaData);
            NftMetaData metaDataObject = JsonConvert.DeserializeObject<NftMetaData>(metaDataString);
            if (metaDataObject == null) return;

            var imgData = await MyWebClient.DownloadDataTaskAsync(metaDataObject.imgUrl);
            var sprite = NftObject.nftObjList.FirstOrDefault(x => x.tokenId == commodity.tokenId).nftImage;
            if(sprite!=null)
                itemImage.sprite = sprite;
            else
            {
                AssetBundle nftBundle = AssetBundle.LoadFromMemory(imgData);
                itemImage.sprite = nftBundle.LoadAllAssets<Sprite>()[0];
            }
            

            priceText.text = $"Price: {(commodity.price/System.Numerics.BigInteger.Pow(10,18)).ToString()}Eth";
            amountText.text = $"Amount: {commodity.amount.ToString()}";
        }
        catch (Exception)
        {
        }
    }
    public void BuyItem()
    {
        BlockchainManager.instance.BuyItemAndTransferOwnership(commodity.itemId,1,commodity.price);
    }
}
