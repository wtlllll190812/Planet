using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class GameManager : SerializedMonoBehaviour
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