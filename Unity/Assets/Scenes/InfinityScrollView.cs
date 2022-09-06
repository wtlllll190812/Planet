using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class InfinityScrollView : MonoBehaviour
{
    public RectTransform content;
    public GameObject buttonPref;
    public List<RectTransform> buttonList;
    public Vector2 currentPos;
    public Vector2 lastPos;

    public Vector2Int range;
    private int width = 160;
    public void Awake()
    {
        range = Vector2Int.zero;
        currentPos = content.anchoredPosition;
        lastPos = content.anchoredPosition;
        for (int i = 0; i < 10; i++)
        {
            buttonList.Add(Instantiate(buttonPref, content.anchoredPosition + currentPos, transform.rotation, content).GetComponent<RectTransform>());
            currentPos.x += width;
            range.y++;
        }
        range.y--;
    }

    public void Add()
    {
        buttonList.Add(Instantiate(buttonPref, content.anchoredPosition + currentPos, transform.rotation, content).GetComponent<RectTransform>());
    }

    public void OnValueChange(Vector2 pos)
    {
        if (Mathf.Abs(lastPos.x - content.anchoredPosition.x) > width)
        {
            Move(lastPos.x - content.anchoredPosition.x>0);
            lastPos = content.anchoredPosition;
        }
    }

    public void Move(bool dir)
    {
        if(dir)
        {
            buttonList[range.x].anchoredPosition = new Vector2(buttonList[range.y].anchoredPosition.x + width, buttonList[range.x].anchoredPosition.y);
            range.x++;
            range.y++;
            if (range.x == buttonList.Count)
                range.x = 0;
            else if (range.y == buttonList.Count)
                range.y = 0;
            content.sizeDelta = new Vector2(content.sizeDelta.x+width, content.sizeDelta.y);
        }
        else
        {
            buttonList[range.y].anchoredPosition = new Vector2(buttonList[range.x].anchoredPosition.x - width, buttonList[range.y].anchoredPosition.y);
            range.x--;
            range.y--;
            if (range.x == -1)
                range.x = buttonList.Count-1;
            else if (range.y == -1)
                range.y = buttonList.Count-1;
        }
    }
}
