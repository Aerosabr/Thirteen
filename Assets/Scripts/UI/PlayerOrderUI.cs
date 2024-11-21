using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerOrderUI : MonoBehaviour
{
    public static PlayerOrderUI Instance { get; private set; }

    [SerializeField] private List<GameObject> currentIndicator;
    [SerializeField] private List<GameObject> skippedShadow;
    [SerializeField] private List<TextMeshProUGUI> nameText;

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
        foreach (GameObject temp in currentIndicator)
        {
            temp.SetActive(false);
        }

        currentIndicator[e.currentPlayer - 1].SetActive(true);
    }
}
