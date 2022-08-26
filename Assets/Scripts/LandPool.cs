using UnityEngine;
using System.Collections;
using DesignMode.Singleton;
using System.Collections.Generic;

public class LandPool:Singleton<LandPool>
{
    public GameObject landPref;
    public List<Land> landEnabled = new List<Land>();
    public List<Land> landDisEnbled = new List<Land>();

    public Land GetLand(Vector3Int pos,Transform tr)
    {
        if (landDisEnbled.Count==0)
            landDisEnbled.Add(Instantiate(landPref).GetComponent<Land>());
        var land = landDisEnbled[0];
        landDisEnbled.Remove(land);
        landEnabled.Add(land);

        land.pos = pos;
        Vector3 newPos = tr.position + (pos - PlanetData.center) * landPref.transform.localScale.x;
        land.gameObject.transform.position = newPos;
        land.transform.parent = tr;

        return land;
    }

    public void DisenbleLand(Land land)
    {
        land.gameObject.SetActive(false);
        landDisEnbled.Add(land);
        landEnabled.Remove(land);
    }
}
