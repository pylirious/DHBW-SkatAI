using System.Collections.Generic;
using Code.Scripts;
using Code.Scripts.PlayerControls;
using UnityEngine;


public class Player
{
    public StatusFlags Flags { get; }
    
    public int ID { get; }
    public List<Card> Hand { get; }
    public int RoundScore { get; set; }
    public int TotalScore { get; set; }

    Card _selectedCard;
    

    public Card SelectedCard
    {
        get
        {
            return _selectedCard;
        }
        set
        {
            _selectedCard = value;
            Flags.IsReadyToContinue = true;
        }
    }

    private int _currentBid;
    public int CurrentBid
    {
        get => _currentBid;
        set
        {
            _currentBid = value;
            Flags.IsReadyToBid = true;
        }
    } 

    List<Card> _discardedCards;
    public List<Card> DiscardedCards
    {
        get
        {
            return _discardedCards;
        }
        set
        {
            _discardedCards = value;
            
            foreach (var card in _discardedCards)
            {
                Hand.Remove(card);
            }
            
            Flags.IsReadyToDiscard = true;
        }
    }

    GameType _gameType;
    public GameType GameType
    {
        get => _gameType;
        set
        {
            _gameType = value;
            Flags.IsReadyToDeclareGame = true;
        }
    } 

    private Card _playedCard;
    public Card PlayedCard
    {
        get => _playedCard;
        set
        {
            _playedCard = value;
            Hand.Remove(_playedCard);
            Flags.IsReadyToPlayCard = true;
        }
    } 
    
    private bool _takesSkat;
    public bool TakesSkat
    {
        get => _takesSkat;
        set
        {
            _takesSkat = value;
            Flags.IsReadyToDecideTakeSkat = true;
        }
    } 

    public List<Card> WonTricks { get; set; } 
    
    public ControllerMode ControllerMode { get; set; } 

    private PlayerController _playerController;

    private int optimalBid = 0;

    public Player(int id, ControllerMode controllerMode)
    {
        ID = id;
        Flags = new StatusFlags();
        Flags.Reset();
        Hand = new List<Card>();
        WonTricks = new List<Card>();
        RoundScore = 0;
        
        
        ControllerMode = controllerMode;
        
        switch (controllerMode)
        {
            case ControllerMode.Human:
                _playerController = new GuiPlayerController(this);
                break;
            case ControllerMode.RandomAI:
                _playerController = new RandomAiPlayerController(this);
                break;
            case ControllerMode.GreedyAI:
                _playerController = new GreedyAiPlayerController(this);
                break;
            case ControllerMode.AI:
                _playerController = new AiController(this);
                break;
            default:
                Debug.LogError("THIS TYPE OF PLAYER CONTROLLER IS NOT IMPLEMENTED YET");
                break;
        }
    }
    
    
    public void PlayCard(List<Card> playedCardsInCurrentTrick, List<Card> allCardsPlayedInRound, int pointsToWinRound)
    {
        _playedCard = null;
        
        List<Card> optimalCards = GetOptimalCards(playedCardsInCurrentTrick);
        
        int pointsSecured = GameManager.CalculateRoundScore(WonTricks);

        int biddingRound = 10 - Hand.Count;
        
        int playersLeftToPlay = 3 - playedCardsInCurrentTrick.Count;

        bool isLonePlayer = GameManager.instance.Declarer == this;
        
        
        _playerController.PlayCard(playedCardsInCurrentTrick,
            Hand,
            optimalCards,
            allCardsPlayedInRound,
            GameManager.instance.GameType,
            pointsToWinRound,
            isLonePlayer,
            pointsSecured,
            biddingRound,
            playersLeftToPlay);
    }
    

    private List<Card> GetOptimalCards(List<Card> playedCardsInCurrentTrick)
    {
        List<Card> optimalCards;
        // Determine Cards allowed to play
        if(playedCardsInCurrentTrick.Count != 0){
            CardType targetType = playedCardsInCurrentTrick[0].cardType;
            
            optimalCards = Hand.FindAll(card => card.cardType == targetType);

            // You can play any card if you don't have the target type
            if (optimalCards.Count == 0)
            {
                optimalCards = Hand;
            }
        }
        else // First one chooses the type
        {
            optimalCards = Hand;
        }

        return optimalCards;
    }

    public void Bid(int currentHighestBid, int playersLeft, int biddingRound, List<List<int>> allBids)
    {
        int baseValue = GetTrumpBaseValue();
        int matadors = CountMatadors();
        int multipliers = matadors;

        int bestPossibleBid = baseValue * multipliers;

        optimalBid = bestPossibleBid;

        _playerController.Bid(currentHighestBid, bestPossibleBid, playersLeft, biddingRound, allBids);
    }
    
    private int GetTrumpBaseValue()
    {
        int baseValue = 0;
        foreach (var card in Hand)
        {
            if (card.cardValue == CardValue.Jack || card.cardValue == CardValue.Queen || card.cardValue == CardValue.King || card.cardValue == CardValue.Ace)
            {
                baseValue += card.cardValue switch
                {
                    CardValue.Jack => 2,
                    CardValue.Queen => 3,
                    CardValue.King => 4,
                    CardValue.Ace => 11,
                    _ => 0
                };
            }
        }

        return baseValue;
    }
    
    private int CountMatadors()
    {
        int matadors = 0;

        foreach (var card in Hand)
        {
            int cardType = (int) card.cardType;
            int gameType = (int) GameManager.instance.GameType;
            
            bool isTrump = cardType == gameType;
            if (isTrump && card.cardValue == CardValue.Jack)
            {
                matadors++;
            }
            else if (isTrump && card.cardValue == CardValue.Queen)
            {
                matadors++;
            }
            else if (isTrump && card.cardValue == CardValue.King)
            {
                matadors++;
            }
            else if (isTrump && card.cardValue == CardValue.Ace)
            {
                matadors++;
            }
        }

        return matadors;
    }
    
    public void DecideTakeSkat()
    {
        _playerController.DecideTakeSkat(Hand);
    }
    
    public void ResetNewRound()
    {
        Flags.Reset();
        Hand.Clear();
        WonTricks = new List<Card>();
        CurrentBid = 0;
    }

    public void DiscardCards()
    {
        _playerController.ChooseDiscardCards(Hand);

        string logMessage = "Hand after discard:";
        foreach (var card in Hand)
        {
            logMessage += " " + card.CardName;
        }
        Debug.Log(logMessage);
    }
    
    public void DeclareGame(int[] otherPlayersHighestBids)
    {
        _playerController.DeclareGame(Hand, otherPlayersHighestBids, _currentBid);
    }
    
    public void PromptContinueGame()
    {
        _playerController.PromptContinueGame();
    }


    public void BidFeedback(int[] otherPlayersHighestBids)
    {
        bool isLonePlayer = GameManager.instance.Declarer == this;
        
        _playerController.BidFeedback(optimalBid, _currentBid, isLonePlayer, otherPlayersHighestBids);
    }

    public void DeclarerFeedback()
    {
        _playerController.DeclarerFeedback(_gameType);
    }
    
    public void PlayCardFeedBack(bool trickWon, int howManyTrickPoints, List<Card> playedCardsInCurrentTrick)
    {
        _playerController.PlayCardFeedBack(trickWon, howManyTrickPoints, playedCardsInCurrentTrick, _playedCard, Hand);
    }

}