using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CommodityHandler : MonoBehaviour
{
    public Commodity commodity;

    public TMPro.TextMeshProUGUI priceText;
    public TMPro.TextMeshProUGUI amountText;

    public void SetCommodity(Commodity commodity)
    {
        this.commodity=commodity;
        priceText.text=commodity.price.ToString();
        amountText.text=commodity.price.ToString();
        //price.name = commodity..ToString();
    }
    public void BuyItem()
    {
        BlockchainManager.instance.BuyItemAndTransferOwnership(commodity.itemId,1,commodity.price);
    }
}
