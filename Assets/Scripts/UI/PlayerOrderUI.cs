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
    [SerializeField] private List<TextMeshProUGUI> placementText;

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

    public void PlayerHandEmptied(int playerID, int placement)
    {
        List<string> placements = new List<string>() { "1st", "2nd", "3rd", "4th" };

        placementText[playerID - 1].text = placements[placement - 1];
        placementText[playerID - 1].transform.parent.gameObject.SetActive(true);
        currentIndicator[playerID - 1].SetActive(false);
    }

    public void PlayerSkipped(int playerID) => skippedShadow[playerID - 1].SetActive(true);
    public void RemoveSkipOverlay(int playerID) => skippedShadow[playerID - 1].SetActive(false);
    public void ResetUI()
    {
        foreach (TextMeshProUGUI text in placementText)
            text.transform.parent.gameObject.SetActive(false);

        foreach (GameObject shadow in skippedShadow)
            shadow.SetActive(false);
    }

}
