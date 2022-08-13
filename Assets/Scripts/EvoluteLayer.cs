using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[System.Serializable]
public abstract class EvoluteLayer
{
    public ComputeShader computeShader;

    public abstract void Init();

    public abstract void Excute(RenderTexture texture, Material mainMaterial);
}
