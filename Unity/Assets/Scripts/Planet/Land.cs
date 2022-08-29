using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Land : MonoBehaviour, IClickable
//地块类
{
    public Vector3Int pos;
    public Planet planet;

    public bool OnClick(Vector3 startPos,Vector3 activeData)
    {
        GameManager.instance.EditPlanet(this,activeData);
        return false;
    }

    public Vector3Int GetPos(Vector3 dir)
    {
        Vector3 nextPos = transform.InverseTransformDirection(dir);
        return new Vector3Int(pos.x + Mathf.RoundToInt(nextPos.x), pos.y + Mathf.RoundToInt(nextPos.y), pos.z + Mathf.RoundToInt(nextPos.z));
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