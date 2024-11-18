using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrderUI : MonoBehaviour
{
    public static PlayerOrderUI Instance { get; private set; }

    [SerializeField] private List<GameObject> playerImages;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Table.Instance.OnPlayerTurn += Table_OnPlayerTurn;
    }

    private void Table_OnPlayerTurn(object sender, Table.OnPlayerTurnEventArgs e)
    {
        foreach (GameObject temp in playerImages)
        {
            temp.SetActive(false);
        }

        foreach (int playerID in e.currentPlayer)
        {
            playerImages[playerID - 1].SetActive(true);
        }
    }
}
