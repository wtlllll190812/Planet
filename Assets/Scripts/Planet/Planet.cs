using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class Planet : SerializedMonoBehaviour,IDragable,IClickable
{
    public PlanetData data;

    public float cameraSize;
    public float rotationSpeed;
    public float revolutionSpeed;

    public virtual void Awake()
    {
        Init();
        GenPlanet();
    }

    [Button("Init")]
    public void Init()
    {
        data.Init();
    }

    public virtual void GenPlanet()
    {
        foreach (ELandData item in data)
        {
            if (item != ELandData.underground && item != ELandData.empty)
            {
                var land = LandPool.Instance.GetLand(data.currentPos,transform);
            }
        }
    }

    [Button("MoveCamera")]
    public void CameraFollow()
    {
        CameraControl.instance.SetTarget(new CameraData(transform, cameraSize));
    }

    public void FixedUpdate()
    {
        if(!GameManager.instance.CompareState(EGameState.Editor))
        {
            transform.RotateAround(Sun.instance.transform.position, Vector3.forward, Time.deltaTime * revolutionSpeed);
            transform.Rotate(transform.up, Time.deltaTime * rotationSpeed,Space.Self);
        }
    }

    public void DragStart(Vector3 startPos)
    {
        Debug.Log("start");
    }

    public void OnDrag(Vector3 currentPos, Vector3 deltaPos)
    {
        transform.Rotate(Vector3.down, deltaPos.x*10,Space.World);
        transform.Rotate(Vector3.right, deltaPos.y*10,Space.World);
    }

    public void DragEnd()
    {
        Debug.Log("end");
    }

    public bool OnClick(Vector3 startPos)
    {
        if(GameManager.instance.CompareState(EGameState.PlanetView))
            GameManager.instance.SetState(EGameState.Editor,this);
        else if(GameManager.instance.CompareState(EGameState.GlobalView))
            GameManager.instance.SetState(EGameState.PlanetView, this);

        return GameManager.instance.CompareState(EGameState.Editor);
    }
}

[System.Serializable]
public class PlanetData: IEnumerator,IEnumerable
{
    [SerializeField]private ELandData[,,] data;
    private int totalSize=16;
    private int planetSize=6;

    public Vector3Int currentPos;
    public static Vector3 center;
    public object Current => data[currentPos.x,currentPos.y,currentPos.z];

    public void Init()
    {
        data = new ELandData[totalSize, totalSize, totalSize];
        center = new Vector3((totalSize - 1) / 2, (totalSize - 1) / 2, (totalSize - 1) / 2);
        //计算星球范围
        for (int x = 0; x < totalSize; x++)
        {
            for (int y = 0; y < totalSize; y++)
            {
                for (int z = 0; z < totalSize; z++)
                {
                    if (Vector3.Distance(new Vector3(x, y, z), center) > planetSize)
                        data[x, y, z] = ELandData.empty;
                    else
                        data[x, y, z] = ELandData.underground;
                }
            }
        }
        //查找边界
        for (int x = 0; x < totalSize; x++)
        {
            for (int y = 0; y < totalSize; y++)
            {
                for (int z = 0; z < totalSize; z++)
                {
                    if (IsBoundary(new Vector3Int(x, y, z)))
                        data[x, y, z] = ELandData.grass;
                }
            }
        }
    }

    public bool IsBoundary(Vector3Int pos)
    {
        if (data[pos.x, pos.y, pos.z] == ELandData.empty)
            return false;
        bool res = false;
        res |= (pos.x + 1 > totalSize-1 || data[pos.x + 1, pos.y, pos.z] == ELandData.empty);
        res |= (pos.x - 1 < 0 || data[pos.x - 1, pos.y, pos.z] == ELandData.empty);
        res |= (pos.y + 1 > totalSize-1 || data[pos.x, pos.y + 1, pos.z] == ELandData.empty);
        res |= (pos.y - 1 < 0 || data[pos.x, pos.y - 1, pos.z] == ELandData.empty);
        res |= (pos.z + 1 > totalSize-1 || data[pos.x, pos.y, pos.z + 1] == ELandData.empty);
        res |= (pos.z - 1 < 0 || data[pos.x, pos.y, pos.z - 1] == ELandData.empty);
        return res;
    }

    public bool MoveNext()
    {
        if (currentPos.x == 15)
        {
            if (currentPos.y == 15)
            {
                if (currentPos.z == 15)
                    return false;
                else
                    currentPos.z++;
                currentPos.y = 0;
            }
            else
                currentPos.y++;
            currentPos.x = 0;
        }
        else
            currentPos.x++;
        return true;
    }

    public void Reset()
    {
        currentPos = Vector3Int.zero;
    }

    public void Update(Land land)
    {

    }

    public IEnumerator GetEnumerator()
    {
        return this;
    }
}