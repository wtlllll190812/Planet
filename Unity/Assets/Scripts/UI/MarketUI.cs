using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MarketUI : MonoBehaviour
{
    public List<CommodityHandler> handlers;

    public void OnEnable()
    {
        int i = 0;
        for (; i < Mathf.Min(BlockchainManager.instance.commodities.Count,handlers.Count); i++)
        {
            handlers[i].SetCommodity(BlockchainManager.instance.commodities[i]);
            handlers[i].gameObject.SetActive(true);
        }
        if(i < handlers.Count-1)
        {
            for (; i < handlers.Count; i++)
            {
                handlers[i].gameObject.SetActive(false);
            }
        }
    }

    public void UpdateData()
    {
        BlockchainManager.instance.GetUnsoldItems();
    }
}
