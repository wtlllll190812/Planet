using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Land : MonoBehaviour, IClickable
{
    //public void OnMouseDown()
    //{
    //    Debug.Log("SDSD");
    //    Destroy(gameObject);
    //}
    public bool OnClick(Vector3 startPos)
    {
        Debug.Log("SDSD");
        Destroy(gameObject);
        return false;
    }
}
public enum ELandData
{
    empty,
    underground,
    grass
}