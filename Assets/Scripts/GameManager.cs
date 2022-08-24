using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameState currentState;

    public void Awake()
    {
        instance = this;
    }
}
public enum GameState
{
    GlobalView,
    PlanetView,
    Editor
}