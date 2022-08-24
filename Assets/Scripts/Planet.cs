using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class Planet : MonoBehaviour
{
    public float size;
    public float rotationSpeed;
    public float revolutionSpeed;
    public bool isSun;
    public Transform plantTr;

    public void Start()
    {
        if(transform.childCount!=0)
            plantTr = transform.GetChild(0);
    }

    [Button("MoveCamera")]
    public void CameraFollow()
    {
        if(plantTr)
            CameraControl.instance.SetTarget(new CameraData(plantTr, size));
    }

    public void SetEditorView()
    {
        CameraControl.instance.SetTarget(new CameraData(transform, size));
    }

    public void FixedUpdate()
    {
        if(GameManager.instance.currentState!=GameState.Editor)
        {
            plantTr.RotateAround(Sun.instance.transform.position, Vector3.forward, Time.deltaTime * revolutionSpeed);
            plantTr?.Rotate(Vector3.forward, Time.deltaTime * rotationSpeed,Space.Self);
        }
    }
}
