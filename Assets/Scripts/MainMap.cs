using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class MainMap : MonoBehaviour
{
    public List<EvoluteLayer> layers;
    public float updateTime=1;
    private Material mainMaterial;

    void Start()
    {
        mainMaterial = GetComponent<SpriteRenderer>().material;
        foreach (var item in layers)
        {
            item.Init();
            item.Excute(GameManager.instance.mainTexture,mainMaterial);
        }
        mainMaterial.SetTexture("_MapTex",GameManager.instance.mainTexture);
        StartCoroutine(OnChange());
    }

    [Button("Change")]
    public IEnumerator OnChange()
    {
        while(true)
        {
            foreach (var item in layers)
            {
                item.Excute(GameManager.instance.mainTexture, mainMaterial);
            }
            mainMaterial.SetTexture("_MapTex", GameManager.instance.mainTexture);
            yield return new WaitForSeconds(updateTime);
        }
    }
}
