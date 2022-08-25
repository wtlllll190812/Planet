using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraControl : MonoBehaviour
{
    public static CameraControl instance; 
    public CameraData target;
    public float speed;

    private Vector2 vPos;
    private float vZpos;
    private Camera mainCamera;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    public void FixedUpdate()
    {
        Vector3 newPos = Vector2.SmoothDamp(transform.position, target.pos.position, ref vPos, speed);
        newPos.z = Mathf.SmoothDamp(transform.position.z, target.zPos, ref vZpos, speed);
        transform.position = newPos;
        //mainCamera.fieldOfView = ;
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
    public float zPos;
    public CameraData(Transform pos,float size)
    {
        this.pos = pos;
        this.zPos = size;
    }
}
