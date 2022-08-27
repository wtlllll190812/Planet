using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<Planet> planetList;
    public Planet currentPlanet;
    public UnityEvent<EGameState> onStateChange;

    private EGameState currentState;

    public void Awake() {
        instance = this;
    }

    public void Start()
    {
        SetState(EGameState.GlobalView,Sun.instance);
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
    /// 编辑星球
    /// </summary>
    public void EditPlanet(Land land,Vector3 activeDir)
    {
        switch (UIManager.Instance.editorPanel.currentState)
        {
            case EditorState.add:
                var newLand=LandPool.Instance.GetLand(land.GetPos(activeDir), land.planet);
                newLand.planet.data.SetLandKind(land, EKindData.grass);
                newLand.planet.data.Update(land);
                break;
            case EditorState.remove:
                land.planet.data.SetLandKind(land, EKindData.empty);
                land.planet.data.Update(land);
                LandPool.Instance.DestoryLand(land);
                break;
        }
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