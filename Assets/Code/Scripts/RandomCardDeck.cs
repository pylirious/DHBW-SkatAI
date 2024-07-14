using System.Collections.Generic;
using Code.Scripts;
using UnityEngine;


public class RandomCardDeck
{
    readonly Dictionary<string, CardValue> _cardValues = new Dictionary<string, CardValue>()
    {
        {"7", CardValue.Seven},
        {"8", CardValue.Eight},
        {"9", CardValue.Nine},
        {"10", CardValue.Ten},
        {"1", CardValue.Ace},
        {"11", CardValue.Jack},
        {"12", CardValue.Queen},
        {"13", CardValue.King},
    };
    
    private List<Card> _deck = new List<Card>();

    public int Count => _deck.Count;

    public RandomCardDeck()
    {
        GenerateDeck();
        ShuffleDeck();
    }
    
    public void GenerateDeck()
    {
        // The sprites of all cards are located in Resources/Texures/Cards
        
        // The name of each Each sprite contains all necessary information. It has the following format:
        // [CardType] [CardValue]

        List<Card> deck = new List<Card>();

        Sprite[] cardSprites = Resources.LoadAll<Sprite>("Textures/Cards");
        
        foreach (Sprite cardSprite in cardSprites)
        {
            string cardName = cardSprite.name;
            string[] cardNameParts = cardName.Split(' ');
            
            CardType cardType = (CardType) System.Enum.Parse(typeof(CardType), cardNameParts[0]);
            CardValue cardValue = _cardValues[cardNameParts[1]];
            
            Card card = new Card()
            {
                cardType = cardType,
                cardValue = cardValue,
                cardImage = cardSprite
            };
            
            deck.Add(card);
        }

        _deck = deck;

    }
    
    
   
    public void ShuffleDeck()
    {
        for (int i = 0; i < _deck.Count; i++)
        {
            Card temp = _deck[i];
            int randomIndex = Random.Range(i, _deck.Count);
            _deck[i] = _deck[randomIndex];
            _deck[randomIndex] = temp;
        }
    }
    
    public Card DrawCard()
    {
        if(Count == 0)
        {
            Debug.LogError("Deck is empty");
            return null;
        }
        
        Card card = _deck[0];
        _deck.RemoveAt(0);
        return card;
    }
}