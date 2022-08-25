using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public RaycastHit[] hits=new RaycastHit[0];
    public Camera mainCam;

    private IDragable currentObj;
    private Vector2 lastPos;

    public void Update()
    {
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
                            lastPos = pos;
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
                    return;
                }
            }
        }
        
        if(currentObj!=null)
        {
            currentObj.DragEnd();
            currentObj = null;
        }
    }
}

public interface IDragable
{
    public void DragStart(Vector3 startPos);
    public void OnDrag(Vector3 currentPos,Vector3 deltaPos);
    public void DragEnd();
}

public interface IPoint
{

}