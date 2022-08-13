using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMap : MonoBehaviour
{
    private Material m_Material;

    void Start()
    {
        m_Material = GetComponent<SpriteRenderer>().material;
        m_Material.SetTexture("_MapTex",GameManager.instance.mainTexture);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
