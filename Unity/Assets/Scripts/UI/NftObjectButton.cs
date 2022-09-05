using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class NftObjectButton : SerializedMonoBehaviour
{
    private Button button;

    public NftObject obj;
    public UnityEvent<NftObjectButton> OnClickEvent;

    public void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        UIManager.Instance.editorPanel.selectedObj = obj;
        OnClickEvent?.Invoke(this);
    }
}
