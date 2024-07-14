using System.Collections.Generic;
using UnityEngine.XR;

namespace Code.Scripts.PlayerControls
{
    public abstract class PlayerController
    {
        protected Player _player;

        public PlayerController(Player player)
        {
            _player = player;
        }
        
        public abstract void Bid(int currentHighestBid, int optimalBid, int playersLeft, int biddingRound, List<List<int>> allBids);
        public abstract void BidFeedback(int optimalBid, int ownBid, bool isLonePlayer, int[] otherPlayersHighestBids);
        
        public abstract void DecideTakeSkat(List<Card> hand);
        public abstract void ChooseDiscardCards(List<Card> newHand);
        public abstract void DeclareGame(List<Card> hand, int[] otherPlayersHighestBids, int ownBid);
        public abstract void DeclarerFeedback(GameType chosenGameType);
        
        public abstract void PlayCard(
            List<Card> playedCardsInCurrentTrick,
            List<Card> hand,
            List<Card> optimalCards,
            List<Card> allCardsPlayedInRound,
            GameType gameType,
            int highestBid,
            bool isLonePlayer,
            int pointsSecured,
            int round, int playersLeftToPlay);
        public abstract void PlayCardFeedBack(bool trickWon,
            int howManyTrickPoints,
            List<Card> playedCardsInCurrentTrick,
            Card playedCard,
            List<Card> hand);
        
        public virtual void PromptContinueGame()
        {
            _player.Flags.IsReadyToContinue = true;
        }
    }
}