using UnityEngine;
using System.Linq;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameState currentState;
    public List<Planet> planetList;
    public Planet currentPlanet;

    public void Awake()
    {
        instance = this;
    }

    [Button("SetState")]
    public void SetState(GameState state,Planet planet)
    {
        switch (state)
        {
            case GameState.GlobalView:
                CameraControl.instance.SetTarget(new CameraData(Sun.instance.transform,10));
                break;
            case GameState.PlanetView:
                planet.CameraFollow();
                break;
            case GameState.Editor:
                planet.SetEditorView();
                break;
            default:
                break;
        }
        currentState = state;
    }
}
public enum GameState
{
    GlobalView,
    PlanetView,
    Editor
}