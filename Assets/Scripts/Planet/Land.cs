using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Land : MonoBehaviour, IClickable
//地块类
{
    public Vector3Int pos;
    public Planet planet;

    public bool OnClick(Vector3 startPos)
    {
        planet.data.SetLandKind(this,EKindData.empty);
        planet.data.Update(this);
        LandPool.Instance.DestoryLand(this);
        return false;
    }
}
/// <summary>
/// 地块类型
/// </summary>
public enum EKindData
{
    empty,
    underground,
    grass
}