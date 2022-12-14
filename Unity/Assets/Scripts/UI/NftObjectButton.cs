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
    public int index;
    public UnityEvent<NftObjectButton> OnClickEvent;
    public TMPro.TextMeshProUGUI text;
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

    public void SetNftObj(NftObject nftobj)
    {
        obj= nftobj;
        GetComponent<Image>().sprite = nftobj.nftImage;
        text.text = obj.nftData.name;
    }
}
