using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    private bool winnerStart;
    private bool suits;
    private TurnOrder turnOrder;

    private void Awake()
    {
        Instance = this;

        winnerStart = false;
        suits = false;
        turnOrder = TurnOrder.Clockwise;
    }
}

public enum TurnOrder
{
    Clockwise,
    CounterClockwise,
    FirstPlay,
}


