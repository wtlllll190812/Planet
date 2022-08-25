using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraControl : MonoBehaviour
{
    public static CameraControl instance; 
    public CameraData target;
    public float speed;

    private Vector2 vPos;
    private float vSize;
    private Camera mainCamera;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        mainCamera = GetComponent<Camera>();
        target = new CameraData(transform,mainCamera.orthographicSize);
    }

    public void FixedUpdate()
    {
        Vector3 newPos = Vector2.SmoothDamp(transform.position, target.pos.position, ref vPos, speed);
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
    public Transform pos;
    public float size;
    public CameraData(Transform pos,float size)
    {
        this.pos = pos;
        this.size = size;
    }
}
