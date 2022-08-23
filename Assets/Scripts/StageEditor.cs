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
    public int sectionIndex=0;
    public int stageIndex=0;

    [Button("Save")]
    void Save()
    {
        StageHandler stage = new StageHandler("Assets/Resources/Scenes/InnerStages/" + path, sceneHandler.transform, sectionIndex, stageIndex);
        stage.Save();
    }

    [Button("Load")]
    void Load()
    {
        StageHandler stage = new StageHandler("Assets/Resources/Scenes/InnerStages/" + path,sceneHandler.transform, sectionIndex, stageIndex);
        stage.Load();
    }
}