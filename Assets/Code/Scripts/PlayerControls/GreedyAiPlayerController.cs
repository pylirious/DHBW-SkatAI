using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace Code.Scripts.PlayerControls
{
    public class GreedyAiPlayerController : PlayerController
    {
        public GreedyAiPlayerController(Player player) : base(player)
        {
            
        }

        public override void Bid(int currentHighestBid, int optimalBid, int playersLeft, int biddingRound,
            List<List<int>> list)
        {
            _player.CurrentBid = optimalBid;
        }

        public override void BidFeedback(int optimalBid, int ownBid, bool isLonePlayer, int[] otherPlayersHighestBids)
        {
            return;
        }

        public override void DecideTakeSkat(List<Card> hand)
        {
            _player.TakesSkat = true;
        }

        public override void ChooseDiscardCards(List<Card> newHand)
        {
            // Discard the two lowest cards
            // Choose the lowest cards using a stack with the length of 2 and "push" the lowest cards onto the stack
            
            int[] stack = new int[2];
            stack[0] = 0;
            stack[1] = 1;
            
            for (int i = 2; i < newHand.Count; i++)
            {
                if (newHand[i].cardValue < newHand[stack[0]].cardValue)
                {
                    stack[1] = stack[0];
                    stack[0] = i;
                }
                else if (newHand[i].cardValue < newHand[stack[1]].cardValue)
                {
                    stack[1] = i;
                }
            }
            
            _player.DiscardedCards = new List<Card> {newHand[stack[0]], newHand[stack[1]]};
        }

        public override void DeclareGame(List<Card> hand, int[] otherPlayersHighestBids, int ownBid)
        {
            // Decide on a game Type using those rules in Order:
            // 1. If the player has 3 or more matadors, declare a grand game
            // 2. If the player has 3 cards of the same suit(cardType), declare a suit game with the suit of the most cards
            // 3. If no other option is available, declare a null game
            
            int matadors = 0;
            int[] suits = new int[4];
            
            // Count the matadors and the suits
            foreach (var card in hand)
            {
                if (card.cardValue == CardValue.Jack)
                {
                    matadors++;
                }
                
                suits[(int) card.cardType]++;
            }
            
            // Check if the player has 3 or more matadors
            if (matadors >= 3)
            {
                _player.GameType = GameType.Grand;
                return;
            }
            
            // Check if the player has 3 or more cards of the same suit
            int maxNumberSuit = 2;
            for (int i = 0; i < suits.Length; i++)
            {
                if (suits[i] > maxNumberSuit)
                {
                    maxNumberSuit = i;
                    _player.GameType = (GameType) i;
                }
            }
            if(maxNumberSuit != 2) return;
            
            // If no other option is available, declare a null game
            _player.GameType = GameType.NullGame;
        }

        public override void DeclarerFeedback(GameType chosenGameType)
        {
            return;
        }

        public override void PlayCard(List<Card> playedCardsInCurrentTrick,
            List<Card> hand,
            List<Card> allCardsPlayedInRound,
            List<Card> cards,
            GameType gameType,
            int highestBid,
            bool isLonePlayer,
            int round,
            int playersLeftToPlay,
            int i)
        {
            int highestCard = 0;
            
             // A different strategies for each GameType
             // 1. If the game is a null game, play the lowest card
             
             bool isNullGame = gameType == GameType.NullGame;
             
             if(isNullGame)
             {
                 int lowestCard = 0;
                 for (int j = 1; j < hand.Count; j++)
                 {
                     if (hand[j].cardValue < hand[lowestCard].cardValue)
                     {
                         lowestCard = j;
                     }
                 }
                 
                 _player.PlayedCard = hand[lowestCard];
                 return;
             }
             
             
             // 2. In any other case, check the following (Grand):
             // 2.1. Is Matador in possession? If yes, play it
             // 2.3. If no, play the highest valued card
             
             bool isGrandGame = gameType == GameType.Grand;

             if (isGrandGame)
             {
                    int matador = -1;
                    for (int j = 0; j < hand.Count; j++)
                    {
                        if (hand[j].cardValue == CardValue.Jack)
                        {
                            matador = j;
                        }
                    }
    
                    if (matador != -1)
                    {
                        _player.PlayedCard = hand[matador];
                        return;
                    }
                    
                    highestCard = 0;
                    for (int j = 1; j < hand.Count; j++)
                    {
                        if (hand[j].cardValue > hand[highestCard].cardValue)
                        {
                            highestCard = j;
                        }
                    }
                    
                    _player.PlayedCard = hand[highestCard];
                    return;
             }
             
             
             
             
             
             
             // Additional rule when gametype is none of the above (suitgame)
             // Play highest trump card if possible
             // Otherwise choose highest card
             
             int trump = (int) gameType;
             
             int highestTrump = -1;
             for (int j = 0; j < hand.Count; j++)
             {
                 if ((int) hand[j].cardType == trump)
                 {
                     if (highestTrump == -1)
                     {
                         highestTrump = j;
                     }
                     else if (hand[j].cardValue > hand[highestTrump].cardValue)
                     {
                         highestTrump = j;
                     }
                 }
             }
             
             if(highestTrump != -1)
             {
                 _player.PlayedCard = hand[highestTrump];
                 return;
             }
             
             highestCard = 0;
             for (int j = 1; j < hand.Count; j++)
             {
                 if (hand[j].cardValue > hand[highestCard].cardValue)
                 {
                     highestCard = j;
                 }
             }
                 
             _player.PlayedCard = hand[highestCard];
             return;
        }

        public override void PlayCardFeedBack(bool trickWon, int howManyTrickPoints, List<Card> playedCardsInCurrentTrick, Card playedCard,
            List<Card> hand)
        {
            return;
        }
    } 
}