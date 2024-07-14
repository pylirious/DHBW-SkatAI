using System.Collections.Generic;

namespace Code.Scripts.PlayerControls
{
    public class GuiPlayerController : PlayerController
    {
        private List<Card> _discardedCards = new List<Card>();
        public GuiPlayerController(Player player) : base(player)
        {
            
        }


        public override void Bid(int currentHighestBid, int optimalBid, int playersLeft, int biddingRound,
            List<List<int>> list)
        {
            _player.Flags.IsReadyToBid = false;
            HumanUiManager.instance.DisplayBidPanel();
        }

        public override void BidFeedback(int optimalBid, int ownBid, bool isLonePlayer, int[] otherPlayersHighestBids)
        {
            return;
        }

        public override void DecideTakeSkat(List<Card> hand)
        {
            HumanUiManager.instance.DisplayTakeSkatPanel();
        }

        public override void ChooseDiscardCards(List<Card> newHand)
        {
            HumanUiManager.instance.DisplayDiscardCardsPanel();
            
            GameManager.instance.cardDisplayManager.RevealMainHand(_player.Hand);
            
            _discardedCards.Clear();
            
            GameManager.instance.currentCardInteraction = DiscardCardLogic;
        }

        public override void DeclareGame(List<Card> hand, int[] otherPlayersHighestBids, int ownBid)
        {
            HumanUiManager.instance.DisplayCallGameTypePanel();
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
            int playersLeftToPlay, int i)
        {
            _player.Flags.IsReadyToPlayCard = false;
            GameManager.instance.currentCardInteraction = PlayCardLogic;
        }

        public override void PlayCardFeedBack(bool trickWon, int howManyTrickPoints, List<Card> playedCardsInCurrentTrick, Card playedCard,
            List<Card> hand)
        {
            return;
        }

        public override void PromptContinueGame()
        {
            _player.Flags.IsReadyToContinue = false;
            HumanUiManager.instance.DisplayReadyToContinue();
        }

        public void DiscardCardLogic(Card card)
        {
            if(_discardedCards.Contains(card))
            {
                _discardedCards.Remove(card);
            }else if(_discardedCards.Count < 2)
            {
                _discardedCards.Add(card);
                if(_discardedCards.Count == 2)
                {
                    _player.DiscardedCards = _discardedCards;
                    GameManager.instance.currentCardInteraction = null;
                    HumanUiManager.instance.discardCardsPanel.SetActive(false);
                }
            }
        }
        
        public void PlayCardLogic(Card card)
        {
            GameManager.instance.currentMainPlayer.PlayedCard = card;
        }
    }
}