using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class StageEditor :SerializedMonoBehaviour
{
    public GameObject sceneHandler;
    public string path="sds";
    public string playerName;
    public int planetIndex;

    [Button("Save")]
    void Save()
    {
        StageHandler stage = new StageHandler("Assets/Resources/Planets/" + path, sceneHandler.transform, playerName, planetIndex);
        stage.Save();
    }

    [Button("Load")]
    void Load()
    {
        StageHandler stage = new StageHandler("Assets/Resources/Planets/" + path,sceneHandler.transform, playerName, planetIndex);
        stage.Load();
    }
}