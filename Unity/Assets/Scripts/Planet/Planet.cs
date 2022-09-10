using System.IO;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class Planet : SerializedMonoBehaviour,IDragable,IClickable,IScalable
{
    public PlanetData planetData;
    protected CameraData cameraData;

    public virtual void Start()
    {
        //Init();
        //GenPlanet();
        GameManager.instance.planetList.Add(this);
        cameraData = new CameraData(transform,planetData.cameraRadius);
    }

    public void FixedUpdate()
    {
        Move();
    }
    
    [Button("Init")]
    public void Init()
    {
        planetData.Init();
    }

    /// <summary>
    /// 生成星球
    /// </summary>
    [Button("GenPlanet")]
    public virtual void GenPlanet()
    {
        foreach (NftLandData item in planetData)
        {
            if (item.IsSurface())
            {
                LandPool.Instance.GetLand(planetData.currentPos,this);
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
            transform.RotateAround(Sun.instance.transform.position, Vector3.forward, Time.deltaTime * planetData.revolutionSpeed);
            transform.Rotate(transform.up, Time.deltaTime * planetData.rotationSpeed, Space.Self);
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

    public bool OnClick(Vector3 startPos,Vector3 activeDir)
    {
        if(GameManager.instance.CompareState(EGameState.PlanetView))
            GameManager.instance.SetState(EGameState.Editor,this);
        else if(GameManager.instance.CompareState(EGameState.GlobalView))
            GameManager.instance.SetState(EGameState.PlanetView, this);

        return GameManager.instance.CompareState(EGameState.Editor);
    }

    public void OnScaling(Vector2 scale)
    {
        if (cameraData.zPos + scale.y < planetData.nearDis && cameraData.zPos + scale.y > planetData.farDis)
            cameraData.zPos += scale.y;
    }

    public NftModel AddNftObject(string objName, Vector3Int pos)
    {
        var gObj=Instantiate(NftModel.nftModelDic[objName].nftGameObject);
        gObj.transform.parent = transform;
        gObj.transform.position = planetData.GetWorldSpacePos(pos);
        gObj.SetActive(true);
        var model = gObj.GetComponent<NftHandler>();
        model.nftModel.pos = pos;
        planetData.nftGobj.Add(model);
        return null;
    }
}

/// <summary>
/// 星球数据类
/// </summary>
[System.Serializable]
public class PlanetData: IEnumerator,IEnumerable
{
    [SerializeField]private NftLandData[,,] data;

    public Planet owner;
    public Vector3Int currentPos;
    public static Vector3 center;
    public object Current => data[currentPos.x, currentPos.y, currentPos.z];
    public List<NftHandler> nftGobj = new List<NftHandler>();

    public float nearDis;
    public float farDis;
    public float trackRadius;
    public float cameraRadius;
    public float rotationSpeed;
    public float revolutionSpeed;

    private readonly int totalSize = 16;
    private readonly int planetSize =6;

    public static Vector3Int[] direction =
            {Vector3Int.up,Vector3Int.down
            ,Vector3Int.left,Vector3Int.right
            ,Vector3Int.forward,Vector3Int.back};

    /// <summary>
    /// 索引器
    /// </summary>
    public NftLandData this[Vector3Int index]
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
        data = new NftLandData[totalSize, totalSize, totalSize];
        center = new Vector3((totalSize - 1) / 2, (totalSize - 1) / 2, (totalSize - 1) / 2);
        //计算星球范围
        for (int x = 0; x < totalSize; x++)
        {
            for (int y = 0; y < totalSize; y++)
            {
                for (int z = 0; z < totalSize; z++)
                {
                    NftLandData newLand = new NftLandData();
                    if (Vector3.Distance(new Vector3(x, y, z), center) > planetSize)
                        newLand.landKind = "empty";
                    else
                        newLand.landKind = "underground";
                    data[x, y, z]=newLand;
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
                        data[x, y, z].landKind= "surface";
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
        if (data[pos.x, pos.y, pos.z].landKind=="empty")
            return false;
        bool res = false;
        res |= (pos.x + 1 > totalSize-1 || data[pos.x + 1, pos.y, pos.z].landKind=="empty");
        res |= (pos.x - 1 < 0 || data[pos.x - 1, pos.y, pos.z].landKind=="empty");
        res |= (pos.y + 1 > totalSize-1 || data[pos.x, pos.y + 1, pos.z].landKind=="empty");
        res |= (pos.y - 1 < 0 || data[pos.x, pos.y - 1, pos.z].landKind=="empty");
        res |= (pos.z + 1 > totalSize-1 || data[pos.x, pos.y, pos.z + 1].landKind=="empty");
        res |= (pos.z - 1 < 0 || data[pos.x, pos.y, pos.z - 1].landKind=="empty");
        return res;
    }
    
    /// <summary>
    /// 更新周边地块数据
    /// </summary>
    public void Update(Land land)
    {
        foreach (var item in direction)
        {
            if (!InRange(item + land.pos))
                continue;
            var kind = this[item + land.pos];
            if (kind.landKind=="underground" && this[land.pos].landKind=="empty")
            {
                this[item + land.pos].landKind = "surface";
                LandPool.Instance.GetLand(item + land.pos, land.planet);
            }
            //if(kind==EKindData.grass&&this[land.pos]==EKindData.grass)
        }
    }
    
    /// <summary>
    /// 设定地块类型
    /// </summary>
    public void SetLandKind(Land land, string kind)
    {
        this[land.pos]=NftLandData.landDataDic[kind];
    }
    
    /// <summary>
    /// 星球数据序列化
    /// </summary>
    public JObject Serialize()
    {
        JObject planet = new JObject();
        JArray landData = new JArray();
        JArray modelData = new JArray();
        planet["nearDis"] = nearDis;
        planet["farDis"] = farDis;
        planet["trackRadius"] = trackRadius;
        planet["cameraRadius"] = cameraRadius;
        planet["rotationSpeed"] = rotationSpeed;
        planet["revolutionSpeed"] = revolutionSpeed;

        for (int x = 0; x < totalSize; x++)
        {
            JArray jarX = new JArray();
            for (int y = 0; y < totalSize; y++)
            {
                JArray jarY = new JArray();
                for (int z = 0; z < totalSize; z++)
                {
                    jarY.Add(data[x, y, z].Serialize());
                }
                jarX.Add(jarY);
            }
            landData.Add(jarX);
        }
        planet["data"] = landData;

        foreach (var item in nftGobj)
        {
            modelData.Add(item.nftModel.Serialize());
            Debug.Log(item.nftModel.Serialize());
        }
        planet["model"] = modelData;

        return planet;
    }

    /// <summary>
    /// 星球数据反序列化
    /// </summary>
    /// <param name="jobj"></param>
    public void Deserialize(JObject jobj)
    {
        data = new NftLandData[totalSize, totalSize, totalSize];
        center = new Vector3((totalSize - 1) / 2, (totalSize - 1) / 2, (totalSize - 1) / 2);

        nearDis = float.Parse(jobj["nearDis"].ToString());
        farDis = float.Parse(jobj["farDis"].ToString());
        trackRadius = float.Parse(jobj["trackRadius"].ToString());
        cameraRadius = float.Parse(jobj["cameraRadius"].ToString());
        rotationSpeed = float.Parse(jobj["rotationSpeed"].ToString());
        revolutionSpeed = float.Parse(jobj["revolutionSpeed"].ToString());

        JArray landData = jobj["data"] as JArray;
        for (int x = 0; x < landData.Count; x++)
        {
            JArray jarX = landData[x] as JArray;
            for (int y = 0; y < jarX.Count; y++)
            {
                JArray jarY = jarX[y] as JArray;
                for (int z = 0; z < jarY.Count; z++)
                {
                    string kind = jarY[z]["landKind"].ToString();
                    if (NftLandData.landDataDic.ContainsKey(kind))
                    {
                        data[x, y, z] = NftLandData.landDataDic[kind];
                    }
                    else
                        data[x, y, z] = new NftLandData(kind);
                }
            }
        }

        JArray modelData = jobj["model"] as JArray;
        Debug.Log(modelData.ToString());
        foreach (JObject item in modelData)
        {
            string modelName = item["name"].ToString();
            Vector3Int newPos = new Vector3Int();
            newPos.x = int.Parse(item["px"].ToString());
            newPos.y = int.Parse(item["py"].ToString());
            newPos.z = int.Parse(item["pz"].ToString());
            owner.AddNftObject(modelName, newPos);
        }
    }
    
    public static Quaternion GetDir(Vector3Int pos,bool ismodel)
    {
        int res=-1;
        float temp = 0f;
        for (int i = 0; i < 6; i++)
        {
            float s = Vector3.Dot(direction[i], Vector3.Normalize(pos - center));
            if ( s >= temp)
            {
                res= i;
                temp = s;
            }
        }
        if(ismodel)
        {
            switch (res)
            {
                case 0: return Quaternion.Euler(90, 0, 0);
                case 1: return Quaternion.Euler(-90, 0, 0);
                case 2: return Quaternion.Euler(0, 90, 0);
                case 3: return Quaternion.Euler(0, -90, 0);
                case 4: return Quaternion.Euler(-180, 0, 0);
                case 5: return Quaternion.Euler(0, 0, 0);
                default: return Quaternion.Euler(0, 0, 0);
            }
        }
        switch (res)
        {
            case 0:return Quaternion.Euler(0,0,0);
            case 1:return Quaternion.Euler(0,0,180);
            case 2:return Quaternion.Euler(0,0,90);
            case 3:return Quaternion.Euler(0,0,-90);
            case 4:return Quaternion.Euler(90,0,0);
            case 5:return Quaternion.Euler(-90,0,0);
            default:return Quaternion.Euler(0,0,0);
        }
    }

    public Vector3 GetWorldSpacePos(Vector3Int pos)
    {
        return owner.transform.TransformPoint(pos - PlanetData.center);
    }

    //实现IEnumerator,IEnumerable接口
    public bool MoveNext()
    {
        if (currentPos.x == 15)
        {
            if (currentPos.y == 15)
            {
                if (currentPos.z == 15)
                {
                    return false;
                }
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