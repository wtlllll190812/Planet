using UnityEngine;
using System.Linq;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public EGameState currentState;
    public List<Planet> planetList;
    public Planet currentPlanet;

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        SetState(EGameState.GlobalView,null);
    }

    [Button("SetState")]
    public void SetState(EGameState state,Planet planet)
    {
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
}
public enum EGameState
{
    GlobalView,
    PlanetView,
    Editor
}