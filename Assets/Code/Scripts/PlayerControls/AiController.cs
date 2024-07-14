using System.Collections.Generic;

namespace Code.Scripts.PlayerControls
{
    public class AiController : PlayerController
    {
        private BiddingAgent _biddingAgent;
        private PlayingAgent _playingAgent;
        private GameDeclaringAgent _gameDeclaringAgent;
        
        private int _currentHighestBid;
        private int _optimalBid;
        private int _playersLeft;
        private int _biddingRound;
        private List<List<int>> _allBids = new List<List<int>>();
        private List<Card> _playedCardsInCurrentTrick;
        private List<Card> _optimalCards;
        private List<Card> _allCardsPlayedInRound;
        private GameType _gameType;
        private int _highestBid;
        private bool _isLonePlayer;
        private int _pointsSecured;
        private int _round;
        private int _playersLeftToPlay;
        private int[] _otherPlayersHighestBids;
        private int _ownBid;

        public AiController(Player player) : base(player)
        {
            _biddingAgent = GameManager.instance.BiddingAgent;
            _playingAgent = GameManager.instance.PlayingAgent;
            _gameDeclaringAgent = GameManager.instance.GameDeclaringAgent;
            _biddingAgent.InitializeAgent(this);
            _playingAgent.InitializeAgent(this);
            _gameDeclaringAgent.InitializeAgent(this);
        }

        public override void Bid(int currentHighestBid, int optimalBid, int playersLeft, int biddingRound, List<List<int>> allBids)
        {
            _player.Flags.IsReadyToBid = false;
            this.CurrentHighestBid = currentHighestBid;
            this.OptimalBid = optimalBid;
            this.PlayersLeft = playersLeft;
            this.BiddingRound = biddingRound;
            this.AllBids = allBids;
            
            _biddingAgent.RequestDecision();
        }

        public override void BidFeedback(int optimalBid, int ownBid, bool isLonePlayer, int[] otherPlayersHighestBids)
        {
            _biddingAgent.Feedback(optimalBid, ownBid, isLonePlayer, otherPlayersHighestBids);
        }

        public override void DecideTakeSkat(List<Card> hand)
        {
            _player.TakesSkat = false;
        }

        public override void ChooseDiscardCards(List<Card> newHand)
        {
            return;
        }

        public override void DeclareGame(List<Card> hand, int[] otherPlayersHighestBids, int ownBid)
        {
            _otherPlayersHighestBids = otherPlayersHighestBids;
            _ownBid = ownBid;
            _gameDeclaringAgent.RequestDecision();
        }

        public override void DeclarerFeedback(GameType chosenGameType)
        {
            _gameDeclaringAgent.Feedback(chosenGameType);
        }

        public override void PlayCard(List<Card> playedCardsInCurrentTrick, List<Card> hand, List<Card> optimalCards, List<Card> allCardsPlayedInRound,
            GameType gameType, int highestBid, bool isLonePlayer, int pointsSecured, int round, int playersLeftToPlay)
        {
            _player.Flags.IsReadyToPlayCard = false;
            _playedCardsInCurrentTrick = playedCardsInCurrentTrick;
            _optimalCards = optimalCards;
            _allCardsPlayedInRound = allCardsPlayedInRound;
            _gameType = gameType;
            _highestBid = highestBid;
            _isLonePlayer = isLonePlayer;
            _pointsSecured = pointsSecured;
            _round = round;
            _playersLeftToPlay = playersLeftToPlay;
            
            _playingAgent.RequestDecision();
            
        }

        public override void PlayCardFeedBack(bool trickWon, int howManyTrickPoints, List<Card> playedCardsInCurrentTrick, Card playedCard,
            List<Card> hand)
        {
            _playingAgent.Feedback(trickWon, howManyTrickPoints, playedCardsInCurrentTrick, playedCard, hand);
        }

        public int CurrentHighestBid { get; set; }

        public int OptimalBid { get; set; }

        public int PlayersLeft { get; set; }

        public int BiddingRound { get; set; }

        public List<List<int>> AllBids { get; set; } = new List<List<int>>();

        public Player Player
        {
            get => _player;
        }
        
        public List<Card> PlayedCardsInCurrentTrick { get => _playedCardsInCurrentTrick; }
        public List<Card> Hand { get => _player.Hand; }
        public List<Card> OptimalCards { get => _optimalCards; }
        public List<Card> AllCardsPlayedInRound { get => _allCardsPlayedInRound; }
        public GameType GameType { get => _gameType; }
        public int HighestBid { get => _highestBid; }
        public bool IsLonePlayer { get => _isLonePlayer; }
        public int PointsSecured { get => _pointsSecured; }
        public int Round { get => _round; }
        public int PlayersLeftToPlay { get => _playersLeftToPlay; }
        public int[] OtherPlayersHighestBids { get => _otherPlayersHighestBids; }
        public int OwnBid { get => _ownBid; }

    }
}
