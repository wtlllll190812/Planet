using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EditorPanel : MonoBehaviour
{
    public EditorState currentState;

    public NftObject selectedObj;
    public InfinityScrollView scrollView;
    public Button noneButton;
    public Button addButton;
    public Button removeButton;

    public void Awake()
    {
        noneButton.onClick.AddListener(SetNoneState);
        addButton.onClick.AddListener(SetAddState);
        removeButton.onClick.AddListener(SetRemoveState);
    }
    
    public void SetAddState() => currentState = EditorState.add;
    public void SetRemoveState() => currentState = EditorState.remove;
    public void SetNoneState() => currentState = EditorState.none;

    /// <summary>
    /// 游戏状态改变时调用
    /// </summary>
    public void OnGameStateChange(EGameState state)
    {
        if(state!=EGameState.Editor)
            gameObject.SetActive(false);
        else
            gameObject.SetActive(true);
    }
}

public enum EditorState
{
    none,
    add,
    remove
}