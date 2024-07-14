using UnityEngine;

namespace Code.Scripts
{
    public class Card
    {
        
        public string CardName => cardType.ToString() + " " + cardValue.ToString();

        public CardType cardType;
    
        public CardValue cardValue;
    
        public Sprite cardImage;
    
        public int GetId()
        {
            var typeIndex = (int)cardType+1;
            var valueIndex = (int)cardValue - 1; // Assuming Ace is 1 and King is 13
            return typeIndex * 13 + valueIndex;
        }

    
    }
}