using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WormHole : Planet,IClickable
{
    public void Awake()
    {
        
    }

    public override void Start()
    {

    }

    public override void GenPlanet()
    {
    }

    public override bool OnClick(Vector3 startPos, Vector3 activeDir)
    {
        base.OnClick(startPos, activeDir);
        BlockchainManager.instance.GetRandomKItems(1);
        return true;
    }
}