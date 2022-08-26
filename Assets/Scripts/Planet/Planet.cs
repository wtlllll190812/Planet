using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class Planet : SerializedMonoBehaviour,IDragable,IClickable,IScalable
{
    public PlanetData data;

    public float cameraSize;
    public float rotationSpeed;
    public float revolutionSpeed;

    public CameraData cameraData ;

    public virtual void Awake()
    {
        Init();
        GenPlanet();
        cameraData = new CameraData(transform,cameraSize);
    }
    
    public void FixedUpdate()
    {
        Move();
    }
    
    [Button("Init")]
    public void Init()
    {
        data.Init();
    }

    /// <summary>
    /// 生成星球
    /// </summary>
    public virtual void GenPlanet()
    {
        foreach (EKindData item in data)
        {
            if (item != EKindData.underground && item != EKindData.empty)
            {
                LandPool.Instance.GetLand(data.currentPos,this);
            }
        }
    }

    /// <summary>
    /// 设置摄像机跟随
    /// </summary>
    [Button("MoveCamera")]
    public void CameraFollow()
    {
        CameraControl.instance.SetTarget(cameraData);
    }

    /// <summary>
    /// 星球公转和自转
    /// </summary>
    public void Move()
    {
        if (!GameManager.instance.CompareState(EGameState.Editor))
        {
            transform.RotateAround(Sun.instance.transform.position, Vector3.forward, Time.deltaTime * revolutionSpeed);
            transform.Rotate(transform.up, Time.deltaTime * rotationSpeed, Space.Self);
        }
    }
    
    //实现拖动接口
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

    public void OnScaling(Vector2 scale)
    {
        Debug.Log(scale);
        cameraData.zPos += scale.y;
    }
}

/// <summary>
/// 星球数据类
/// </summary>
[System.Serializable]
public class PlanetData: IEnumerator,IEnumerable
{
    public Vector3Int currentPos;
    public static Vector3 center;
    public object Current => data[currentPos.x, currentPos.y, currentPos.z];

    [SerializeField]private EKindData[,,] data;
    private int totalSize=16;
    private int planetSize=6;

    public static Vector3Int[] direction =
            {Vector3Int.up,Vector3Int.down
            ,Vector3Int.left,Vector3Int.right
            ,Vector3Int.forward,Vector3Int.back};

    /// <summary>
    /// 索引器
    /// </summary>
    public EKindData this[Vector3Int index]
    {
        get
        {
            return data[index.x,index.y,index.z];
        }
        set
        {
            data[index.x, index.y, index.z] = value;
        }
    }
    
    /// <summary>
    /// 初始化数据
    /// </summary>
    public void Init()
    {
        data = new EKindData[totalSize, totalSize, totalSize];
        center = new Vector3((totalSize - 1) / 2, (totalSize - 1) / 2, (totalSize - 1) / 2);
        //计算星球范围
        for (int x = 0; x < totalSize; x++)
        {
            for (int y = 0; y < totalSize; y++)
            {
                for (int z = 0; z < totalSize; z++)
                {
                    if (Vector3.Distance(new Vector3(x, y, z), center) > planetSize)
                        data[x, y, z] = EKindData.empty;
                    else
                        data[x, y, z] = EKindData.underground;
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
                        data[x, y, z] = EKindData.grass;
                }
            }
        }
    }

    /// <summary>
    /// 是否在范围内
    /// </summary>
    public bool InRange(Vector3Int pos)
    {
        bool res=true;
        res &= pos.x < totalSize;
        res &= pos.x > 0;
        res &= pos.y < totalSize;
        res &= pos.y > 0;
        res &= pos.z < totalSize;
        res &= pos.z > 0;
        return res;
    }

    /// <summary>
    /// 是否在星球边界(是否可见)
    /// </summary>
    public bool IsBoundary(Vector3Int pos)
    {
        if (data[pos.x, pos.y, pos.z] == EKindData.empty)
            return false;
        bool res = false;
        res |= (pos.x + 1 > totalSize-1 || data[pos.x + 1, pos.y, pos.z] == EKindData.empty);
        res |= (pos.x - 1 < 0 || data[pos.x - 1, pos.y, pos.z] == EKindData.empty);
        res |= (pos.y + 1 > totalSize-1 || data[pos.x, pos.y + 1, pos.z] == EKindData.empty);
        res |= (pos.y - 1 < 0 || data[pos.x, pos.y - 1, pos.z] == EKindData.empty);
        res |= (pos.z + 1 > totalSize-1 || data[pos.x, pos.y, pos.z + 1] == EKindData.empty);
        res |= (pos.z - 1 < 0 || data[pos.x, pos.y, pos.z - 1] == EKindData.empty);
        return res;
    }
    
    /// <summary>
    /// 更新周边地块数据
    /// </summary>
    public void Update(Land land)
    {
        foreach (var item in direction)
        {
            var kind = this[item + land.pos];
            if (kind == EKindData.underground && this[land.pos] == EKindData.empty)
            {
                this[item + land.pos] = EKindData.grass;
                LandPool.Instance.GetLand(item + land.pos, land.planet);
            }
        }
    }
    
    /// <summary>
    /// 设定地块类型
    /// </summary>
    public void SetLandKind(Land land, EKindData kind)
    {
        this[land.pos] = kind;
    }

    //实现IEnumerator,IEnumerable接口
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

    public IEnumerator GetEnumerator()
    {
        return this;
    }
}