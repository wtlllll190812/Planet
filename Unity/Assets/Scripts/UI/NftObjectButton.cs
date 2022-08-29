using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

public class NftObjectButton : MonoBehaviour
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
