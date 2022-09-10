using System.IO;
using UnityEngine;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<Planet> planetList;
    public Planet currentPlanet;
    public UnityEvent<EGameState> onStateChange;
    public GameObject planetPref;
    private EGameState currentState;

    public void Awake() 
    {
        instance = this;
    }

    public void Start()
    {
        SetState(EGameState.GlobalView,Sun.instance);
        //StartCoroutine(Load());
    }

    /// <summary>
    /// 判断游戏状态
    /// </summary>
    public bool CompareState(EGameState state)
    {
        return currentState == state;
    }

    /// <summary>
    /// 设定状态机
    /// </summary>
    [Button("SetState")]
    public void SetState(EGameState state,Planet planet)
    {
        onStateChange?.Invoke(state);
        switch (state)
        {
            case EGameState.GlobalView:
                CameraControl.instance.SetTarget(new CameraData(Sun.instance.transform,-24));
                break;
            case EGameState.PlanetView:
                planet.CameraFollow();
                break;
            case EGameState.Editor:
                planet.CameraFollow();
                break;
            default:
                break;
        }
        currentState = state;
        currentPlanet = planet;
    }

    /// <summary>
    /// 编辑地块
    /// </summary>
    public void EditLand(Land land,Vector3 activeDir)
    {
        switch (UIManager.Instance.editorPanel.currentState)
        {
            case EditorState.add:
                if (!land.planet.planetData.InRange(land.GetPos(activeDir)))
                    break;
                land.planet.planetData[land.GetPos(activeDir)]= NftLandData.landDataDic[UIManager.Instance.editorPanel.selectedObj.nftData.name];
                var newLand =LandPool.Instance.GetLand(land.GetPos(activeDir), land.planet);
                newLand.planet.planetData.Update(land);
                break;
            case EditorState.remove:
                land.planet.planetData.SetLandKind(land, "empty");
                land.planet.planetData.Update(land);
                LandPool.Instance.DestoryLand(land);
                break;
            default :
                break;
        }
    }
    [Button("sdsd")]
    public void testc(Vector3Int t)
    {
        //Quaternion rot = Quaternion.FromToRotation(transform.TransformDirection(Vector3.up), transform.TransformDirection(PlanetData.GetDir(t)));
        //transform.rotation = rot;
    }
    /// <summary>
    /// 添加装饰物
    /// </summary>
    public void AddModel(Land land, Vector3 activeDir)
    {
        if (UIManager.Instance.editorPanel.currentState == EditorState.add)
        {
            string objName = UIManager.Instance.editorPanel.selectedObj.nftData.name;
            //land.planet.AddNftObject(UIManager.Instance.editorPanel.selectedObj.nftData.name, land.GetPos(activeDir));
            var gObj = Instantiate(NftModel.nftModelDic[objName].nftGameObject);
            gObj.transform.parent = land.planet.transform;
            gObj.transform.localPosition = (land.GetPos(activeDir)- PlanetData.center)/4;
            gObj.transform.localRotation = PlanetData.GetDir(land.GetPos(activeDir),true);
            gObj.SetActive(true);

            var model = gObj.GetComponent<NftHandler>();
            model.nftModel.pos = land.pos;
            land.planet.planetData.nftGobj.Add(model);
        }
    }

    [Button("Save")]
    public void Save()
    {
        using (FileStream file = new FileStream("planet2.json", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
        {
            JObject output = new JObject();
            BinaryFormatter formatter = new BinaryFormatter();
            foreach (var item in planetList)
            {
                output[item.name] = item.planetData.Serialize();
            }
            
            //file.Write(Encoding.Default.GetBytes(output.ToString()));
            formatter.Serialize(file, output.ToString());
        }
        Debug.Log("Save Success");
    }

    [Button("Load")]
    public IEnumerator Load()
    {
        //while (BlockchainManager.instance.nfts == null)
        //    yield return null;
        foreach (var item in BlockchainManager.instance.nfts)
        {
            if (item.name.Split("_")[1] == "land")
            {
                NftLandData data = new NftLandData();
                data.DeSerialize(item);
            }
            else
            {
                NftModel model = new NftModel();
                model.DeSerialize(item);
            }
        }

        using (FileStream file = new FileStream("planet2.json", FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            JObject output = JObject.Parse(formatter.Deserialize(file) as string);
            foreach (var item in output)
            {
                var planet = Instantiate(planetPref).GetComponent<Planet>();
                planet.name = item.Key;
                planet.planetData.owner = planet;
                planet.planetData.Deserialize(item.Value as JObject);
                planet.transform.position = Sun.instance.transform.position + Vector3.right * planet.planetData.trackRadius;
                planet.GenPlanet();
            }
        }
        Debug.Log("Load Success");
        UIManager.Instance.editorPanel.scrollView.Init();
        yield return null;
    }
}

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum EGameState
{
    GlobalView,
    PlanetView,
    Editor
}