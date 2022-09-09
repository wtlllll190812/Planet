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