using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public Camera mainCam;

    private IDragable currentObj;
    private Vector2 lastPos;
    private RaycastHit[] hits = new RaycastHit[0];

    public void Update()
    {
        if(Input.GetMouseButtonDown(0)&& !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCam.transform.position.z));
            Vector3 startPos = new Vector3(pos.x, pos.y, -4);
            hits = Physics.SphereCastAll(startPos, 0.1f, Vector3.forward);
            foreach (var item in hits)
            {
                IClickable clickable= item.collider.GetComponent<Planet>() as IClickable;
                if(clickable!=null)
                {
                    clickable.OnClick(pos);
                    return;
                }
            }
            GameManager.instance.SetState(EGameState.GlobalView,null);
        }

        if(Input.GetMouseButton(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -mainCam.transform.position.z));
            Vector3 startPos = new Vector3(pos.x, pos.y, -4);
            hits = Physics.SphereCastAll(startPos, 1f, Vector3.forward);

            foreach (var item in hits)
            {
                IDragable newDragObj = item.collider.GetComponent<Planet>() as IDragable;
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
        
        if(Input.GetMouseButtonUp(0))
        {
            if (currentObj != null)
            {
                currentObj.DragEnd();
                currentObj = null;
            }
        }
    }
}

public interface IDragable
{
    public void DragStart(Vector3 startPos);
    public void OnDrag(Vector3 currentPos,Vector3 deltaPos);
    public void DragEnd();
}

public interface IClickable
{
    public void OnClick(Vector3 startPos);
}