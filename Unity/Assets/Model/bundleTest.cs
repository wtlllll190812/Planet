using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bundleTest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ABBaoJiaZai());
    }

    /// <summary>
    /// AB包的加载
    /// </summary>
    private IEnumerator ABBaoJiaZai()
    {
        string path = "AssetBundles/fruiter.asset";//AB包的位置
        //异步加载
        AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(path));
        yield return request;
        AssetBundle ab = request.assetBundle;

        //使用里面的资源
        Instantiate(ab.LoadAllAssets<GameObject>()[0]);

        StopCoroutine(ABBaoJiaZai());
    }
}
//https://ipfs.moralis.io:2053/ipfs/QmPfdA8KXZ571m8nPHb2k5t6ipsXSzw2S6LswnbYMemhqV/fruiter_object_637980790326929449.json
//https://ipfs.moralis.io:2053/ipfs/QmeD6SN17BTzb3G6D2MgztqRkG7euLYxdXKwXboyVYTFut/cypress_object_637980789765096146.json