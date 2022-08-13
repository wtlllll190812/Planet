using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Evotemper : EvoluteLayer
{
    public float deltaTem;

    public override void Init()
    {
    }

    public override void Excute(RenderTexture texture, MainMap map)
    {
        map._temperature += deltaTem;
    }
}
