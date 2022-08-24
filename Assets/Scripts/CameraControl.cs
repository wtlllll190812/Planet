using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraControl : MonoBehaviour
{
    public CameraData target;
    public float speed;

    private Vector2 vPos;
    private float vSize;
    private Camera mainCamera;

    public void Start()
    {
        mainCamera = GetComponent<Camera>();
        target = new CameraData(transform.position,mainCamera.orthographicSize);
    }

    public void Update()
    {
        Vector3 newPos = Vector2.SmoothDamp(transform.position, target.pos, ref vPos, speed);
        newPos.z = -10;
        transform.position = newPos;

        mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize, target.size, ref vSize, speed);
    }

    public void SetTarget(CameraData newTarget)
    {
        target = newTarget;
    }
}

[System.Serializable]
public struct CameraData
{
    public Vector2 pos;
    public float size;
    public CameraData(Vector2 pos,float size)
    {
        this.pos = pos;
        this.size = size;
    }
}
