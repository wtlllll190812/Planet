using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class Plan1GameManager : SerializedMonoBehaviour
{
    public static Plan1GameManager instance;

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