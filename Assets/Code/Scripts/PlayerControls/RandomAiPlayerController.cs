using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Scripts.PlayerControls
{
    public class RandomAiPlayerController : PlayerController
    {
        public RandomAiPlayerController(Player player) : base(player)
        {
            
        }

        public override void Bid(int currentHighestBid, int optimalBid, int playersLeft, int biddingRound,
            List<List<int>> list)
        {
            int newBid = Random.Range(0, (int) (optimalBid * 1.2f));
            if(newBid > _player.CurrentBid)
                _player.CurrentBid = newBid;
            else
            {
                // set current bid to same value to update the flag
                _player.CurrentBid = _player.CurrentBid;
            }
        }

        public override void BidFeedback(int optimalBid, int ownBid, bool isLonePlayer, int[] otherPlayersHighestBids)
        {
            return;
        }

        public override void DecideTakeSkat(List<Card> hand)
        {
            _player.TakesSkat = Random.Range(0, 2) == 0;
        }

        public override void ChooseDiscardCards(List<Card> newHand)
        {
            _player.DiscardedCards = new List<Card> {newHand[0], newHand[1]}; }

        public override void DeclareGame(List<Card> hand, int[] otherPlayersHighestBids, int ownBid)
        {
            _player.GameType = (GameType)Random.Range(0, 4);
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
             _player.PlayedCard = hand[Random.Range(0, hand.Count)];
        }

        public override void PlayCardFeedBack(bool trickWon, int howManyTrickPoints, List<Card> playedCardsInCurrentTrick, Card playedCard,
            List<Card> hand)
        {
            return;
        }
    } 
}