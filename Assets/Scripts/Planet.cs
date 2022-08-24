using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class Planet : MonoBehaviour
{
    public float size;
    public float rotationSpeed;
    public float revolutionSpeed;

    [Button("MoveCamera")]
    public void CameraMove()
    {
        CameraControl.instance.SetTarget(new CameraData(transform,size));
    }

    public void FixedUpdate()
    {
        transform.RotateAround(Sun.instance.transform.position, Vector3.forward, Time.deltaTime*revolutionSpeed);
        transform.Rotate(Vector3.forward,Time.deltaTime*rotationSpeed);
    }
}
