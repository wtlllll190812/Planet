using UnityEngine;
using System.Collections;
using DesignMode.Singleton;
using System.Collections.Generic;

public class LandPool:Singleton<LandPool>
/// <summary>
/// 地块对象池
/// </summary>
{
    public GameObject landPref;
    private List<Land> landEnabled = new List<Land>();
    private List<Land> landDisEnbled = new List<Land>();

    /// <summary>
    /// 从对象池中获取地块
    /// </summary>
    /// <param name="pos">地块位置</param>
    /// <param name="planet">地块所属星球</param>
    public Land GetLand(Vector3Int pos,Planet planet)
    {
        if (landDisEnbled.Count==0)
            landDisEnbled.Add(Instantiate(landPref).GetComponent<Land>());
        var land = landDisEnbled[0];
        land.gameObject.SetActive(true);
        landDisEnbled.Remove(land);
        landEnabled.Add(land);

        Vector3 newPos = planet.transform.TransformPoint((pos - PlanetData.center) * landPref.transform.localScale.x);
        land.gameObject.transform.position = newPos;
        
        land.transform.parent = planet.transform;
        land.planet = planet;
        land.transform.localRotation = PlanetData.GetDir(pos,false);
        
        land.pos = pos;

        NftLandData landData = planet.planetData[pos];
        land.transform.GetChild(0).GetComponent<MeshRenderer>().material.mainTexture = landData.landTexture;
        return land;
    }

    /// <summary>
    /// 销毁地块
    /// </summary>
    public void DestoryLand(Land land)
    {
        land.gameObject.SetActive(false);
        landDisEnbled.Add(land);
        landEnabled.Remove(land);
    }
}
