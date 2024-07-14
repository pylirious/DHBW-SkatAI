using System;
using System.Collections;
using System.Collections.Generic;
using Code.Scripts;
using Code.Scripts.PlayerControls;
using Unity.VisualScripting;
using UnityEngine;

public enum LoopPhase
{
    COMPLETE_GAME,
    BiddingPhase,
    HandSelectionPhase,
    GameDeclarationPhase,
    PlayingPhase,
}
public class GameManager : MonoBehaviour
{
    private int _roundsPlayed = 0;
    
    
    RandomCardDeck _randomCardDeck;

    private bool _skatGame = false;
    private GameType _gameType = GameType.Clubs;
    public GameType GameType => _gameType;

    public Player Declarer => _declarer;
    
    private int _bidPoints = 0;
    private Player _declarer;
    private int _minPointsToWin = 61;
    List<Card> allCardsPlayedInRound = new List<Card>();
    
    
    // Players
    private readonly Player[] _players = new Player[3];

    
    
    [Header("Game Settings")]
    
    public ControllerMode player0ControllerMode = ControllerMode.RandomAI;
    public ControllerMode player1ControllerMode = ControllerMode.RandomAI;
    public ControllerMode player2ControllerMode = ControllerMode.RandomAI;
    
    [Space(20)]
    public int roundsToPlay = 4;
    public LoopPhase currentLoopPhase = LoopPhase.COMPLETE_GAME;
    public bool showAnimations = true;
    public bool TRAINING_MODE_FAST_NO_EPILEPSY = false;
    
    
    
    [Space(100)]
    [Header("References")]
    public CardDisplayManager cardDisplayManager;
    public TMPro.TMP_Text roundCounter;
    public TMPro.TMP_Text phaseDisplayer;
    public TMPro.TMP_Text activePlayerDisplayer;
    
    public TMPro.TMP_Text[] playerInfoTexts = new TMPro.TMP_Text[3];
    
    public BiddingAgent BiddingAgent;
    public PlayingAgent PlayingAgent;
    public GameDeclaringAgent GameDeclaringAgent;

    public delegate void CardInteraction(Card card);
    
    // Change from discard to play card
    public CardInteraction currentCardInteraction;
    
    
    List<Card> _skatCards;
    
    [DoNotSerialize]
    public Player currentMainPlayer;

    public static GameManager instance;

    int[] allPlayerHighestBids = new int[3];

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        _players[0] = new Player(0, player0ControllerMode);
        _players[1] = new Player(1, player1ControllerMode);
        _players[2] = new Player(2, player2ControllerMode);
        
        _roundsPlayed = 0;
        
        StartCoroutine(DealCards());
    }
    
    // The Phases in order:
    // 1. Deal Cards
    // 2. Bidding Phase
    // 3. Hand Selection Phase
    // 4. Game Declaration Phase
    // 5. Playing Phase
    // 6. Scoring Phase
    // 7. Check Game End
    
    

    IEnumerator DealCards()
    {
        //CHECK IF LOOP SHOULD STOP
        if (_roundsPlayed >= roundsToPlay)
        {
            Debug.Log("Game Over");
            yield break;
        }
        
        _roundsPlayed++;
        
        roundCounter.text = "Round: " + _roundsPlayed;
        
        phaseDisplayer.text = "Dealing Cards";
        
        ResetRoundData();
        
        _randomCardDeck = new RandomCardDeck();
        
        Debug.Log("Dealing Cards");
        
        while (_players[0].Hand.Count < 10)
        {
            GiveCard(_randomCardDeck, 0);
            GiveCard(_randomCardDeck, 1);
            GiveCard(_randomCardDeck, 2);

            if (showAnimations && !TRAINING_MODE_FAST_NO_EPILEPSY)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        if (showAnimations && !TRAINING_MODE_FAST_NO_EPILEPSY)
        {
            yield return new WaitForSeconds(1f);
        }
        

        // Once cards are dealt, proceed to bidding
        InitNextPhase(BiddingPhase());
    }

    IEnumerator BiddingPhase()
    {
        Debug.Log("Bidding Phase");
        
        phaseDisplayer.text = "Bidding Phase";
        
        int highestBid = 0, numberOfBidsInRound = 0;
        Player highestBidder = null;

        int biddingRound = 0;
        int playersLeft = 3;
        
        List<List<int>> allBids = new List<List<int>>();
        
        for (int i = 0; i < 3; i++)
        {
            allBids.Add(new List<int>());
        }
        
        // Loop through each player to collect their bids
        while (numberOfBidsInRound != 1)
        {
            biddingRound++;
            
            numberOfBidsInRound = 0;
            
            for (int i = 0; i < _players.Length; i++)
            {
                Player player = _players[i];
                SwitchMainPlayer(player);
                
                if (showAnimations && !TRAINING_MODE_FAST_NO_EPILEPSY)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                if (player.ControllerMode == ControllerMode.Human)
                {
                    player.PromptContinueGame();
                    yield return new WaitUntil(() => player.Flags.IsReadyToContinue);
                    cardDisplayManager.RevealMainHand(player.Hand);
                }

                player.Bid(highestBid, playersLeft, biddingRound, allBids);
                
                yield return new WaitUntil(() => player.Flags.IsReadyToBid); // Wait for player bid

                int bid = player.CurrentBid;

                Debug.Log("Player " + player.ID + " has bid " + bid + "\nCurrent highest bid is " + highestBid);
                
                if (bid > highestBid)
                {
                    highestBid = bid;
                    highestBidder = player;

                    numberOfBidsInRound++;
                }
                else
                {
                    playersLeft--;
                }
                
                allBids[i].Add(bid);

                allPlayerHighestBids[i] = bid;
            }

            if (numberOfBidsInRound == 0)
            {
                Debug.Log("No valid bids were made.");
                break;
            }
        }

        if (highestBidder != null)
        {
            Debug.Log("Highest bidder is Player " + highestBidder.ID + " with a bid of " + highestBid);
            
            _declarer = highestBidder;
            _bidPoints = highestBid;
            _minPointsToWin = Mathf.Max(61, highestBid);

            foreach (var player in _players)
            {
                player.BidFeedback(allPlayerHighestBids);
            }
        }
        else
        {
            Debug.Log("No valid bids were made. Restarting Game");
            
        }
        


        // THIS IS FOR AI TRAINING. LOOP ONE PHASE TO TRAIN MODEL FOR THAT PHASE
        if (IsLoopPhase(LoopPhase.BiddingPhase) && _declarer != null)
        {
            InitNextPhase(DealCards());
        }
        else
        {
            InitNextPhase(HandSelectionPhase());
        }
        
        
        
    }

    IEnumerator HandSelectionPhase()
    {
        
        Debug.Log("Hand Selection Phase");
        
        phaseDisplayer.text = "Hand Selection Phase";
        
        if (_declarer != null)
        {
            SwitchMainPlayer(_declarer);
            
            if (showAnimations && !TRAINING_MODE_FAST_NO_EPILEPSY)
            {
                yield return new WaitForSeconds(0.5f);
            }
            if (_declarer.ControllerMode == ControllerMode.Human)
            {
                _declarer.PromptContinueGame();
                yield return new WaitUntil(() => _declarer.Flags.IsReadyToContinue);
                cardDisplayManager.RevealMainHand(_declarer.Hand);
            }
            
            List<Card> skatCards = PickSkatCards();
            
            _declarer.DecideTakeSkat();
            
            yield return new WaitUntil(() => _declarer.Flags.IsReadyToDecideTakeSkat);

            if (_declarer.TakesSkat)
            {
                _skatGame = true;
                
                _declarer.Hand.AddRange(skatCards);
                _declarer.DiscardCards();
                
                yield return new WaitUntil(() => _declarer.Flags.IsReadyToDiscard);
                
                _skatCards = _declarer.DiscardedCards;
                Debug.Log("Declarer " + _declarer.ID + " has picked up the Skat and discarded two cards.");
            }
            else
            {
                Debug.Log("Declarer didnt take Skat");
            }
            
        }
        else
        {
            Debug.Log("No declarer found for hand selection phase.");
            InitNextPhase(DealCards());
            yield break;
        }

        
        // THIS IS FOR AI TRAINING. LOOP ONE PHASE TO TRAIN MODEL FOR THAT PHASE
        if (IsLoopPhase(LoopPhase.HandSelectionPhase))
        {
            InitNextPhase(DealCards());
        }
        else
        {
            InitNextPhase(GameDeclarationPhase());
        }
    }

    List<Card> PickSkatCards()
    {
        return new List<Card> { _randomCardDeck.DrawCard(), _randomCardDeck.DrawCard() };
    }

    IEnumerator GameDeclarationPhase()
    {
        Debug.Log("Game Declaration Phase");
        
        phaseDisplayer.text = "Game Declaration Phase";
        
        if (_declarer != null)
        {
            cardDisplayManager.RevealMainHand(_declarer.Hand);
            
            _declarer.DeclareGame(allPlayerHighestBids);
            
            yield return new WaitUntil(() => _declarer.Flags.IsReadyToDeclareGame);
            GameType gameType = _declarer.GameType;
            _gameType = gameType;
            
            Debug.Log("Declarer " + _declarer.ID + " has declared a " + gameType + " game.");
            
            _declarer.DeclarerFeedback();
        }
        
        // THIS IS FOR AI TRAINING. LOOP ONE PHASE TO TRAIN MODEL FOR THAT PHASE
        if (IsLoopPhase(LoopPhase.GameDeclarationPhase))
        {
            InitNextPhase(DealCards());
        }
        else
        {
            InitNextPhase(PlayingPhase());
        }
    }

    IEnumerator PlayingPhase()
    {
        Debug.Log("Playing Phase");
        
        phaseDisplayer.text = "Playing Phase\n" + GameType.ToString();
        
        Player currentLeader = _declarer; 
        int numberOfTricks = 10; 

       

        for (int i = 0; i < numberOfTricks; i++)
        {
            foreach (var player in _players)
            {
                player.PlayedCard = null;
            }
            
            yield return StartCoroutine(PlayTrick(currentLeader, i));
        }

        InitNextPhase(ScoringPhase());
    }

    IEnumerator PlayTrick(Player startingPlayer, int trickNUmber)
    {
        List<Card> trickCards = new List<Card>();

        for (int i = 0; i < 3; i++)
        {
            Player currentPlayer = GetNextPlayer(startingPlayer, i);
            
            SwitchMainPlayer(currentPlayer);
            
            if (showAnimations && !TRAINING_MODE_FAST_NO_EPILEPSY)
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            if (currentPlayer.ControllerMode == ControllerMode.Human)
            {
                currentPlayer.PromptContinueGame();
                yield return new WaitUntil(() => currentPlayer.Flags.IsReadyToContinue);
                cardDisplayManager.RevealMainHand(currentPlayer.Hand);
            }
            
            currentPlayer.PlayCard(trickCards, allCardsPlayedInRound, _minPointsToWin);

            yield return new WaitUntil(() => currentPlayer.Flags.IsReadyToPlayCard);
            
            trickCards.Add(currentPlayer.PlayedCard);
            allCardsPlayedInRound.Add(currentPlayer.PlayedCard);
        }
        
        int winnerID = DetermineTrickWinner(trickCards);

        Player winner = GetNextPlayer(startingPlayer, winnerID);
        
        winner.WonTricks.AddRange(trickCards);

        foreach (var player in _players)
        {
            player.PlayCardFeedBack(player == winner, CalculateRoundScore(trickCards), trickCards);
        }

    }

    int DetermineTrickWinner(List<Card> trickCards)
    {
        
        int highestValue = 0;
        bool highestCardIsTrump = false;
        int winner = 0;
        for (int i = 0; i < trickCards.Count; i++)
        { 
            if (trickCards[i].cardValue == CardValue.Jack && GameType == GameType.Grand ||
                (int)trickCards[i].cardType == (int)GameType)
            {
                if (!highestCardIsTrump)
                {
                    highestValue = (int)trickCards[i].cardValue;
                    winner = i;
                    highestCardIsTrump = true;
                    continue;
                }

                if ((int)trickCards[i].cardValue > highestValue)
                {
                    highestValue = (int)trickCards[i].cardValue;
                    winner = i;
                    continue;
                }
            }
            // If not, check if there is one already and continue otherwise
            if(highestCardIsTrump) continue;
            
            if ((int)trickCards[i].cardValue > highestValue)
            {
                highestValue = (int)trickCards[i].cardValue;
                winner = i;
            }
        }

        return winner;
    }
    
    IEnumerator ScoringPhase()
    {
        Debug.Log("Scoring Phase");
        
        phaseDisplayer.text = "Scoring Phase";
        
        bool declarerWonRound = false;
        
        foreach (var player in _players)
        {
            player.RoundScore = CalculateRoundScore(player.WonTricks);

            if (player == _declarer)
            {
                if (_gameType == GameType.NullGame && player.RoundScore == 0)
                {
                    declarerWonRound = true;
                }
                else if (_gameType != GameType.NullGame && player.RoundScore >= _minPointsToWin)
                {
                    declarerWonRound = true;
                }
            }
        }
        if(declarerWonRound)
        {
            _declarer.TotalScore += _bidPoints;
        }
        else
        {
            // Subtract double the bid points if the declarer picked up the skat
            _declarer.TotalScore -= _skatGame ? 2 * _bidPoints : _bidPoints;
        }
        
        currentMainPlayer.PromptContinueGame();
        
        yield return new WaitUntil(() => currentMainPlayer.Flags.IsReadyToContinue);
        
        // THIS IS FOR AI TRAINING. LOOP ONE PHASE TO TRAIN MODEL FOR THAT PHASE
        if (IsLoopPhase(LoopPhase.PlayingPhase))
        {
            InitNextPhase(DealCards());
        }
        else
        {
            InitNextPhase(CheckGameEnd());
        }
    }

    IEnumerator CheckGameEnd()
    {

        foreach (var player in _players)
        {
            Debug.Log("Player " + player.ID + ": \n IsDeclarer: " + (player == _declarer).ToString() + " \n Bid: " + player.CurrentBid + " \n Points: " + player.TotalScore + "\n Round Score: " + player.RoundScore + "\n Won Tricks: " + player.WonTricks.Count/3);
        }
        if (_roundsPlayed >= roundsToPlay)
        {
            int highestScore = _players[0].TotalScore;
            Player highestScorePlayer = _players[0];
            
            foreach (var player in _players)
            {
                if (player.TotalScore > highestScore)
                {
                    highestScore = player.TotalScore;
                    highestScorePlayer = player;
                }
            }

            Debug.Log("Game Over. " + highestScorePlayer.ID + " has won the game with " + highestScore + " points.");
            
            HumanUiManager.instance.EnableWinnerText();
            
            yield break;
        }
        InitNextPhase(DealCards());
    }

    private void GiveCard(RandomCardDeck deck, int playerId)
    {
        Card card = deck.DrawCard();
        _players[playerId].Hand.Add(card);
        cardDisplayManager.GiveCard(playerId, card);
    }
    
    
    private Player GetNextPlayer(Player startingPlayer, int offset)
    {
        int nextPlayerID = (startingPlayer.ID + offset) % 3;

        return _players[nextPlayerID];
    }
    
    
    // Clear all Flags and go into the next phase
    private void InitNextPhase(IEnumerator nextUpdate)
    {
        foreach (var player in _players)  
        {
            player.Flags.Reset();
        }
        
        StartCoroutine(nextUpdate);
    }
    
    private void SwitchMainPlayer(Player player)
    {
        currentMainPlayer = player;
        
        if(TRAINING_MODE_FAST_NO_EPILEPSY) return;
        
        cardDisplayManager.SwitchMainPlayer(player.ID, _players);
        activePlayerDisplayer.text = "Current Player: " + player.ID;
        
        ChangePlayerTexts(playerInfoTexts[0], player);
        ChangePlayerTexts(playerInfoTexts[1], _players[(player.ID + 1) % 3]);
        ChangePlayerTexts(playerInfoTexts[2], _players[(player.ID + 2) % 3]);
        
    }
    
    private void ChangePlayerTexts(TMPro.TMP_Text tmpText, Player player)
    {
        if(TRAINING_MODE_FAST_NO_EPILEPSY) return;
        
        tmpText.text = "Player: " + player.ID + "\nBid: " + _players[player.ID].CurrentBid + "\n Trick Score: " + CalculateRoundScore(player.WonTricks) + "\nTotal Score: " + _players[player.ID].TotalScore;

        if (player == _declarer)
        {
            tmpText.text += "\nDeclarer";
            tmpText.color = Color.yellow;
        }
        else
        {
            tmpText.color = Color.white;
        } 
    }
    
    public static int CalculateRoundScore(List<Card> wonTricks)
    {
        int roundScore = 0;
        
        foreach (var card in wonTricks)
        {
            roundScore += (int)card.cardValue;
        }

        return roundScore;
    }

    void ResetRoundData()
    {
        _skatGame = false;
        _gameType = GameType.Clubs;
        _bidPoints = 0;
        _declarer = null;
        _skatCards = null;
        _minPointsToWin = 61;
        allCardsPlayedInRound.Clear();
        
        cardDisplayManager.ResetCards();

        foreach (var player in _players) player.ResetNewRound();
    }

    bool IsLoopPhase(LoopPhase currentPhase)
    {
        return currentPhase == currentLoopPhase;
    }
}
