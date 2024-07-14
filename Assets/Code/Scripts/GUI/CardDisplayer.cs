using System.Collections;
using System.Collections.Generic;
using Code.Scripts;
using PlayerControls;
using UnityEngine;

public class CardDisplayer : MonoBehaviour
{
    
    public GameObject cardPrefab;
    
    public Transform cardParent;
    
    private List<Card> _cards = new List<Card>();

    private int _maxhandcards = 11;

    public int degreeValue = 2;
    
    
    // The List of cards to display
    internal List<Card> Cards
    {
        get { return _cards; }
        set
        {
            _cards = value; 
            AddCard(_cards[_cards.Count - 1]);
        }
    }

    private List<SpriteRenderer> cardsSpriteRenderers = new List<SpriteRenderer>();


    public void AddCard(Card card, bool isInteractible = false)
    {
        Cards.Add(card);

        // Parameters for the arc
        float arcWidthDegrees = degreeValue * _maxhandcards; // Total angle covered by the hand of cards
        float radius = 30f; // Radius of the arc

        // Center the arc according to the number of cards
        float startAngle = -(arcWidthDegrees / 2);
        float angleIncrement = _maxhandcards > 1 ? arcWidthDegrees / (_maxhandcards - 1) : 0;

        
        
        // Calculate the angle for the current card
        float angleDegrees = startAngle + (angleIncrement * Cards.Count - 1);
        float angleRadians = angleDegrees * Mathf.Deg2Rad;

        // Position the card along an arc
        Vector3 position = new Vector3(Mathf.Sin(angleRadians) * radius, Mathf.Cos(angleRadians) * radius, 0f) + cardParent.position;
        
        
        // Calculate the rotation of the card to face upwards always
        Quaternion rotation = Quaternion.Euler(0f, 0f, -angleDegrees);

        // Instantiate the card display
        GameObject cardDisplay = Instantiate(cardPrefab, position, rotation, cardParent);
        cardDisplay.transform.localScale = transform.localScale;

        // Set the sprite of the card display
        SpriteRenderer spriteRenderer = cardDisplay.GetComponent<SpriteRenderer>();
        cardsSpriteRenderers.Add(spriteRenderer);
        spriteRenderer.sprite = card.cardImage;
        
        // Set the sorting order of the card display
        spriteRenderer.sortingOrder = Cards.Count;

        CardHoverEffect hoverEffect = cardDisplay.GetComponent<CardHoverEffect>();
        hoverEffect.Targetable = isInteractible;
        hoverEffect.targetSourcePosition = position;
        hoverEffect.card = card;

        StartCoroutine(hoverEffect.FlyToSourceTargetPos());
    }
    
    public void DisplayNewHand(List<Card> newHand, bool exposeCards = false)
    {
        if (GameManager.instance.TRAINING_MODE_FAST_NO_EPILEPSY) return;
        ResetCards();
        _maxhandcards = newHand.Count + 1;
        foreach (var card in newHand)
        {
            AddCard(exposeCards ? card : CardDisplayManager.HiddenCard, exposeCards);
        }
    }
    
    public void ResetCards()
    {
        foreach (SpriteRenderer spriteRenderer in cardsSpriteRenderers)
        {
            Destroy(spriteRenderer.gameObject);
        }
        cardsSpriteRenderers.Clear();
        Cards.Clear();
    }
    
    
}


