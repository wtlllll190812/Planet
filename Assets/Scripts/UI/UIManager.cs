using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignMode.Singleton;

public class UIManager : Singleton<UIManager>
{
    [SerializeField]private Stack<Windows> windows;
    [SerializeField]private Windows currentWindows;

    /// <summary>
    /// 向栈中添加窗口
    /// </summary>
    /// <param name="w"></param>
    public void AddWindows(Windows w)
    {
        var count = w.transform.parent.childCount;
        w.transform.SetSiblingIndex(count - 1);
        w.gameObject.SetActive(true);

        if (!currentWindows)
        {
            currentWindows = w;
        }
        else
        {
            windows.Push(currentWindows);
            currentWindows = w;
        }
    }

    /// <summary>
    /// 切换到下一个窗口
    /// </summary>
    public void NextWindows()
    {
        currentWindows?.gameObject.SetActive(false);
        
        if (windows.Count > 0) 
            currentWindows = windows.Pop();
        else
            currentWindows = null;

        if(currentWindows)
        {
            var count = currentWindows.transform.parent.childCount;
            currentWindows.transform.SetSiblingIndex(count - 1);
            currentWindows.gameObject.SetActive(true);
        }
    }
}
