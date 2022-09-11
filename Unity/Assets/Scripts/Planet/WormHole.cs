using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WormHole : Planet,IClickable
{
    public GameObject confirmUi;

    public override void Start()
    {
        cameraData = new CameraData(transform, planetData.cameraRadius);
    }

    public override void GenPlanet()
    {
    }

    public override bool OnClick(Vector3 startPos, Vector3 activeDir)
    {
        if (GameManager.instance.currentPlanet == this)
            confirmUi.SetActive(true);
        base.OnClick(startPos, activeDir);
        return true;
    }

    public void GetRandomItem()
    {
        BlockchainManager.instance.GetRandomKItems(System.Numerics.BigInteger.Pow(10,18));
    }
}