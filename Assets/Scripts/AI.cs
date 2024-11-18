using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Playables;

public class AI : Player
{
    [SerializeField] private List<CardData> isolatedSingles = new List<CardData>();
    [SerializeField] private List<CardData> twos = new List<CardData>();
    [SerializeField] private List<CardData> hand = new List<CardData>();
    [SerializeField] private List<CardData> pairs = new List<CardData>();
    public override void SitOnChair(Chair chair)
    {
        transform.position = chair.GetSitPoint().transform.position;
        transform.rotation = chair.GetSitPoint().transform.rotation;
        this.chair = chair;

        playerVisual.PlayAnimation("Sitting");
    }

    public override void InitializePlayer(int playerPos)
    {
        playerID = playerPos;
        Table.Instance.OnPlayerTurn += Table_OnPlayerTurn;
        Table.Instance.GetChair(playerPos).Interact(gameObject);
    }

    private void Table_OnPlayerTurn(object sender, Table.OnPlayerTurnEventArgs e)
    {
        if (e.currentPlayer.Contains(playerID))
        {
            List<CardData> cards = ConvertHandToCardData();
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (cards[i].rank == Rank.Two)
                {
                    twos.Add(cards[i]);
                    cards.Remove(cards[i]);
                }
            }

            isolatedSingles = FindIsolatedSingles(cards);
            hand = cards.Except(isolatedSingles).ToList();
            pairs = FindDouble(hand);
        }
    }

    private void PlayCards()
    {
        List<Card> cards = new List<Card>();
        chair.PlayCards(cards);
    }

    #region Hand Processing
    private List<CardData> ConvertHandToCardData()
    {
        List<CardData> cardDatas = new List<CardData>();

        foreach (Card card in chair.GetHand())
            cardDatas.Add(new CardData(card.GetRank(), card.GetSuit()));

        return cardDatas;
    }

    // NOTE: ensure list being passed in contains no Twos
    private List<CardData> FindIsolatedSingles(List<CardData> cardDatas)
    {
        // Dictionary to count quantity of each rank
        var rankQuantities = cardDatas.GroupBy(card => (int)card.rank).ToDictionary(group => group.Key, group => group.Count());

        // Identify isolated singles
        var isolatedSingles = cardDatas.Where(card =>
            {
                // Check if this rank is isolated
                return rankQuantities[(int)card.rank] == 1 && // Only one card of this rank
                       !(rankQuantities.ContainsKey((int)card.rank - 4) && rankQuantities.ContainsKey((int)card.rank + 4)) && // No adjacent cards
                       !(rankQuantities.ContainsKey((int)card.rank - 4) && rankQuantities.ContainsKey((int)card.rank - 8)) && // No previous two cards
                       !(rankQuantities.ContainsKey((int)card.rank + 4) && rankQuantities.ContainsKey((int)card.rank + 8));  // No next two cards
            }).ToList();

        return isolatedSingles;
    }

    private List<CardData> FindDouble(List<CardData> cardDatas)
    {
        //     
        List<CardData> pairs = cardDatas.GroupBy(card => card.rank)
            .Where(group => group.Count() == 2).SelectMany(group => group).ToList();

        return pairs;
    }
    #endregion
}
