using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class Windows : SerializedMonoBehaviour
{
    public bool isTop;
    public UnityEvent onOpen;
    public UnityEvent onClose;

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public virtual void CloseWindows()
    {
        Time.timeScale = 1;
        UIManager.Instance.NextWindows();
    }
    
    /// <summary>
     /// 关闭窗口
     /// </summary>
    public virtual void OpenWindows()
    {
        Time.timeScale = 0;
        UIManager.Instance.AddWindows(this);
    }

    public virtual void OnEnable()
    {
        onOpen?.Invoke();
    }

    public virtual void OnDisable()
    {
        onClose?.Invoke();
    }
}
