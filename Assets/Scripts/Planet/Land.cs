using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Land : MonoBehaviour, IClickable
{
    public Vector3Int pos;
    public Planet planet;

    //public void OnMouseDown()
    //{
    //    Debug.Log("SDSD");
    //    Destroy(gameObject);
    //}
    public bool OnClick(Vector3 startPos)
    {
        Debug.Log("SDSD");
        Destroy(gameObject);
        planet.data.Update(this);
        return false;
    }
}
public enum ELandData
{
    empty,
    underground,
    grass
}