using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public bool winnerStart;
    public bool suits;
    public TurnOrder turnOrder;

    private void Awake()
    {
        Instance = this;

        winnerStart = false;
        suits = false;
        turnOrder = TurnOrder.Clockwise;

        DontDestroyOnLoad(gameObject);
    }
}

public enum TurnOrder
{
    Clockwise,
    CounterClockwise,
    FirstPlay,
}


