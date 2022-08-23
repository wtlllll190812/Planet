using UnityEngine;
using System.Linq;
using System.Xml.Linq;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class StageHandler
{
    public string fileName;
    public Transform sceneHandler;
    public string playerName;
    public int planetIndex = 0;

    public StageHandler(string path, Transform transform, string _playerName, int _planetIndex)
    {
        fileName = path;
        sceneHandler = transform;
        playerName = _playerName;
        planetIndex = _planetIndex;
    }

    public StageHandler(string path, Transform transform)
    {
        fileName=path;
        sceneHandler=transform;

        XDocument _doc = XDocument.Load(fileName + ".xml");
        XElement root = _doc.Root;
        playerName = root.Attribute("playerName").Value;
        planetIndex = int.Parse(root.Attribute("planetIndex").Value);
    }
    /// <summary>
    /// 加载场景数据
    /// </summary>
    public void Load()
    {
        sceneHandler = GameObject.Find("SceneHandler").transform;
        GameObject.DestroyImmediate(sceneHandler.gameObject);
        sceneHandler = new GameObject("empty").transform;
        sceneHandler.name = "SceneHandler";
        XDocument _doc = XDocument.Load(fileName + ".xml");
        XElement root = _doc.Root;
        CreateGobjInScene(root, sceneHandler);
    }

    /// <summary>
    /// 保存场景数据
    /// </summary>
    public void Save()
    {
        //实例化XDocument对象
        //创建根节点
        XDocument xdoc = new XDocument();
        XElement root = new XElement("Stage");
        root.SetAttributeValue("planetIndex", planetIndex);
        root.SetAttributeValue("playerName", playerName);

        GetGobjFromScene(root,sceneHandler);
        xdoc.Add(root);
        xdoc.Save(fileName+".xml");
        Debug.Log($"创建XML文件成功！路径为{fileName}");
    }
   
    /// <summary>
    /// 递归创建GameObject 
    /// </summary>
    /// <param name="root">根节点</param>
    /// <param name="parentTr">父物体</param>
    private void CreateGobjInScene(XElement root, Transform parentTr)
    {
        foreach (var item in root.Elements())
        {
            string path= item.Attribute("Path").Value.ToString();
            string name = item.Attribute("Name").Value.ToString();

            path = path.Replace("Assets/Resources/","");
            path = path.Replace(".prefab","").Trim();

            GameObject go;

            if (parentTr.Find(name))
            {
                go = parentTr.Find(name).gameObject;
            }
            else if (path == "Empty")
                go = new GameObject("Empty");
            else
            {
                Debug.Log(path);
                go = GameObject.Instantiate(Resources.Load<GameObject>(path));
            }
            go.transform.parent = parentTr;
            SetGobjData(item, go);

            CreateGobjInScene(item, go.transform);
        }
    }
    
    /// <summary>
    /// 递归添加节点 
    /// </summary>
    /// <param name="root">根节点</param>
    /// <param name="parent">父物体</param>
    private void GetGobjFromScene(XElement root,Transform parent )
    {
        for (int index = 0; index < parent.childCount; index++)
        {
            var child = parent.GetChild(index);

            XElement newElement = new XElement("class");
            LoadableObject loadableObject = child.GetComponent<LoadableObject>();
            
            newElement.SetAttributeValue("Name", child.name);
            newElement.SetAttributeValue("PositionX", child.position.x);
            newElement.SetAttributeValue("PositionY", child.position.y);
            newElement.SetAttributeValue("PositionZ", child.position.z);
            newElement.SetAttributeValue("RotationX", child.eulerAngles.x);
            newElement.SetAttributeValue("RotationY", child.eulerAngles.y);
            newElement.SetAttributeValue("RotationZ", child.eulerAngles.z);
            newElement.SetAttributeValue("ScaleX", child.localScale.x);
            newElement.SetAttributeValue("ScaleY", child.localScale.y);
            newElement.SetAttributeValue("ScaleZ", child.localScale.z);

            newElement.SetAttributeValue("Path", loadableObject ? loadableObject.prefabPath : "Empty");
            
            GetGobjFromScene(newElement, child);
            root.Add(newElement);
        }
    }

    /// <summary>
    /// 根据节点配置物体
    /// </summary>
    /// <param name="node"></param>
    /// <param name="go"></param>
    private void SetGobjData(XElement node,GameObject go)
    {
        Transform tr = go.transform;
        go.name = node.Attribute("Name").Value;
        tr.position=new Vector3( float.Parse(node.Attribute("PositionX").Value), float.Parse(node.Attribute("PositionY").Value), float.Parse(node.Attribute("PositionZ").Value)) ;
        tr.eulerAngles= new Vector3(float.Parse(node.Attribute("RotationX").Value), float.Parse(node.Attribute("RotationY").Value), float.Parse(node.Attribute("RotationZ").Value));
        tr.localScale= new Vector3(float.Parse(node.Attribute("ScaleX").Value), float.Parse(node.Attribute("ScaleY").Value), float.Parse(node.Attribute("ScaleZ").Value)) ;
    }
}