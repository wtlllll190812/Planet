using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NftHandler : MonoBehaviour,IClickable
{
    public Planet planet;
    public NftModel nftModel;

    public bool OnClick(Vector3 startPos, Vector3 activeDir)
    {
        if(UIManager.Instance.editorPanel.currentState==EditorState.remove)
        {
            planet.planetData.nftGobj.Remove(this);
            Destroy(gameObject);
        }
        return false;
    }
}
