using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GenNoise genNoise;
    public MainMap mainMap;

    [HideInInspector]public RenderTexture mainTexture;

    public void Awake()
    {
        if(instance==null)
            instance = this;
        else
           Destroy(gameObject);
    }
}
