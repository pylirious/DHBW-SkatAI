using System;
using System.Collections.Generic;
using Code.Scripts;
using Code.Scripts.PlayerControls;
using UnityEngine;

public class CardDisplayManager : MonoBehaviour
{
    public CardDisplayer[] CardDisplayers = new CardDisplayer[3];
    public SpriteRenderer[] PlayedCards = new SpriteRenderer[2];
    public GameObject[] Backgrounds = new GameObject[3];
    public Sprite HiddenCardSprite;
    
    public static Card HiddenCard;

    private void Awake()
    {
        HiddenCard = new Card
        {
            cardType = CardType.Clubs,
            cardValue = CardValue.Eight,
            cardImage = HiddenCardSprite
        };
    }

    public void GiveCard(int playerId, Card card)
    {
        if(GameManager.instance.TRAINING_MODE_FAST_NO_EPILEPSY) return;
        CardDisplayers[playerId].AddCard(CardDisplayManager.HiddenCard);
    }
    
    public void ResetCards()
    {
        foreach (var cardDisplayer in CardDisplayers)
        {
            cardDisplayer.ResetCards();
        }
    }
    
    public void SwitchMainPlayer(int playerID, Player[] players)
    {
        CardDisplayers[0].DisplayNewHand(players[playerID].Hand);
        Backgrounds[playerID].SetActive(true);
        
        
        CardDisplayers[1].DisplayNewHand(players[(playerID + 1) % 3].Hand);
        Backgrounds[(playerID + 1) % 3].SetActive(false);
        if(players[(playerID + 1) % 3].PlayedCard != null)
            PlayedCards[0].sprite = players[(playerID + 1) % 3].PlayedCard.cardImage;
        else
            PlayedCards[0].sprite = null;
        
        CardDisplayers[2].DisplayNewHand(players[(playerID + 2) % 3].Hand);
        Backgrounds[(playerID + 2) % 3].SetActive(false);
        if(players[(playerID + 2) % 3].PlayedCard != null)
            PlayedCards[1].sprite = players[(playerID + 2) % 3].PlayedCard.cardImage;
        else
            PlayedCards[1].sprite = null;
    }

    public void RevealMainHand(List<Card> hand)
    {
        CardDisplayers[0].DisplayNewHand(hand, exposeCards:true);
    }
}
