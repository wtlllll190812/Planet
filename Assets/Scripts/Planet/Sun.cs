using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sun : Planet
{
    public static Sun instance;

    public override void Awake()
    {
        if(instance==null)
            instance = this;
        else
            Destroy(this);
    }

    public override void Start()
    {

    }

    public override void GenPlanet()
    {
    }
}
