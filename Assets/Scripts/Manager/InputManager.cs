using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public Camera mainCam;

    private IDragable currentObj;
    private Vector2 lastPos;
    private List<RaycastHit> hitObjs = new List<RaycastHit>();

    public void Update()
    {
        //鼠标按下判断
        if(Input.GetMouseButtonDown(0)&& !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCam.transform.position.z));
            Vector3 startPos = new Vector3(pos.x, pos.y, -4);
            hitObjs=new List<RaycastHit>(Physics.RaycastAll(startPos, Vector3.forward));

            hitObjs.Sort((x,y)=>x.distance>y.distance? 1:-1);

            bool hitSth=false;
            foreach (var item in hitObjs)
            {
                IClickable clickable= item.collider.GetComponent<MonoBehaviour>() as IClickable;
                if(clickable!=null)
                {
                    hitSth = true;
                    if (!clickable.OnClick(item.point,item.normal))
                        return;
                }
            }
            if(!hitSth)
                GameManager.instance.SetState(EGameState.GlobalView,null);
        }

        //鼠标持续按下
        if(Input.GetMouseButton(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCam.transform.position.z));
            Vector3 startPos = new Vector3(pos.x, pos.y, -4);
            hitObjs = new List<RaycastHit>(Physics.SphereCastAll(startPos, 1f, Vector3.forward));
            hitObjs.Sort((x, y) => x.distance > y.distance ? 1 : -1);

            foreach (var item in hitObjs)
            {
                IDragable newDragObj = item.collider.GetComponent<MonoBehaviour>() as IDragable;
                if (newDragObj != null)
                {
                    if (currentObj != null)
                    {
                        if (newDragObj == currentObj)
                        {
                            newDragObj.OnDrag(pos, pos - lastPos);
                        }
                        else
                        {
                            currentObj.DragEnd();
                            newDragObj.DragStart(pos);
                            currentObj = newDragObj;
                        }
                    }
                    else
                    {
                        newDragObj.DragStart(pos);
                        currentObj = newDragObj;
                    }
                    lastPos = pos;
                    return;
                }
            }
        }
        
        //鼠标抬起
        if(Input.GetMouseButtonUp(0))
        {
            if (currentObj != null)
            {
                currentObj.DragEnd();
                currentObj = null;
            }
        }

        GameManager.instance.currentPlanet?.OnScaling(Input.mouseScrollDelta);
    }
}

/// <summary>
/// 缩放接口
/// </summary>
public interface IScalable
{
    public void OnScaling(Vector2 scale);
}

/// <summary>
/// 可拖拽对象接口
/// </summary>
public interface IDragable
{
    public void DragStart(Vector3 startPos);
    public void OnDrag(Vector3 currentPos,Vector3 deltaPos);
    public void DragEnd();
}

/// <summary>
/// 可点击对象接口
/// </summary>
public interface IClickable
{
    public bool OnClick(Vector3 startPos,Vector3 activeDir);
}