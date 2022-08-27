using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class MainMap : SerializedMonoBehaviour
{
    public List<EvoluteLayer> layers;
    public float updateTime=1;

    private Material mainMaterial;
    public float _temperature
    {
        set
        {
            if(value<1&&value>0)
                mainMaterial.SetFloat("_MoutainThreshold", value);
        }
        get
        {
            return mainMaterial.GetFloat("_MoutainThreshold");
        }
    }

    void Start()
    {
        mainMaterial = GetComponent<SpriteRenderer>().material;
        mainMaterial.SetTexture("_MapTex",Plan1GameManager.instance.mainTexture);
        foreach (var item in layers)
            item.Init();
        StartCoroutine(OnChange());
    }

    [Button("Change")]
    public IEnumerator OnChange()
    {
        while(true)
        {
            foreach (var item in layers)
            {
                item.Excute(Plan1GameManager.instance.mainTexture, this);
            }
            mainMaterial.SetTexture("_MapTex", Plan1GameManager.instance.mainTexture);
            yield return new WaitForSeconds(updateTime);
        }
    }
}
