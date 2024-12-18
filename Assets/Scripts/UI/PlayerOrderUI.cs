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
        //Table.Instance.OnPlayerTurn += Table_OnPlayerTurn;
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

    [ServerRpc(RequireOwnership = false)]
    public void ChairStateChangedServerRpc()
    {
        ChairStateChangedClientRpc();
    }

    [ClientRpc]
    private void ChairStateChangedClientRpc()
    {
        for (int i = 1; i <= 4; i++)
        {
            Chair chair = Table.Instance.GetChair(i);
            switch (chair.GetPlayerType())
            {
                case PlayerType.AI:
                    nameText[i - 1].text = "AI";
                    playerBackground[i - 1].gameObject.SetActive(true);
                    playerBackground[i - 1].sprite = characterBackgrounds[Table.Instance.GetAIOnChair(i).GetComponent<AI>().modelNum];
                    break;
                case PlayerType.Player:
                    nameText[i - 1].text = PlayerManager.Instance.Players[(int)chair.playerID].playerName.ToString();
                    playerBackground[i - 1].gameObject.SetActive(true);
                    playerBackground[i - 1].sprite = characterBackgrounds[PlayerManager.Instance.Players[(int)chair.playerID].modelNum];
                    break;
                case PlayerType.None:
                    nameText[i - 1].text = "";
                    playerBackground[i - 1].gameObject.SetActive(false);
                    break;
            }
        }
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
