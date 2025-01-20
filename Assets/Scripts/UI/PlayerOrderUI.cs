using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerOrderUI : NetworkBehaviour
{
    public static PlayerOrderUI Instance { get; private set; }

    [SerializeField] private List<GameObject> currentIndicator;
    [SerializeField] private List<GameObject> skippedShadow;
    [SerializeField] private List<TextMeshProUGUI> nameText;
    [SerializeField] private List<TextMeshProUGUI> placementText;
    [SerializeField] private List<Sprite> characterBackgrounds;
    [SerializeField] private List<Image> playerBackground;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Hide();
        Table.Instance.OnPlayerTurn += Table_OnPlayerTurn;
    }

    private void Table_OnPlayerTurn(object sender, Table.OnPlayerTurnEventArgs e)
    {
        Debug.Log("Current player: " + e.currentPlayer);
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

    [ServerRpc(RequireOwnership = false)]
    public void ChairStateChangedServerRpc()
    {
        for (int i = 1; i <= 4; i++)
        {
            Chair chair = Table.Instance.GetChair(i);
            switch (chair.GetPlayerType())
            {
                case PlayerType.AI:
                    ChairStateChangedClientRpc(i - 1, "AI", true, Table.Instance.GetAIOnChair(i).GetComponent<AI>().modelNum.Value);
                    break;
                case PlayerType.Player:
                    ChairStateChangedClientRpc(i - 1, PlayerManager.Instance.Players[(int)chair.playerID].playerName.ToString(), true, PlayerManager.Instance.Players[(int)chair.playerID].modelNum);
                    break;
                case PlayerType.None:
                    ChairStateChangedClientRpc(i - 1, "", false, 0);
                    break;
            }
        }
    }

    [ClientRpc]
    private void ChairStateChangedClientRpc(int chairNum, string nameText, bool playerBackgroundToggle, int modelNum)
    {
        this.nameText[chairNum].text = nameText;
        playerBackground[chairNum].gameObject.SetActive(playerBackgroundToggle);
        playerBackground[chairNum].sprite = characterBackgrounds[modelNum];
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
