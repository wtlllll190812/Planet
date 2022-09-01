using UnityEngine;
using UnityEditor;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class LoadableObject : MonoBehaviour
{
    public string prefabPath;

    //[Button("GetPath")]
    //public void Reset()
    //{
    //    if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
    //    {
    //        // 预制体资源就是自身
    //        prefabPath = AssetDatabase.GetAssetPath(gameObject);
    //    }

    //    // Scene中的Prefab Instance是Instance不是Asset
    //    if (PrefabUtility.IsPartOfPrefabInstance(gameObject))
    //    {
    //        // 获取预制体资源
    //        var prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(gameObject);
    //        prefabPath = AssetDatabase.GetAssetPath(prefabAsset);
    //    }

    //    // PrefabMode中的GameObject既不是Instance也不是Asset
    //    var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject);
    //    if (prefabStage != null)
    //    {
    //        // 预制体资源：prefabAsset = prefabStage.prefabContentsRoot
    //        prefabPath = prefabStage.prefabAssetPath;
    //    }
}
