using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GenNoise genNoise;
    public MainMap mainMap;

    public RenderTexture mainTexture
    {
        get { return genNoise.renderTexture; }
    }

    public void Awake()
    {
        if(instance==null)
            instance = this;
        else
           Destroy(gameObject);
    }
}
