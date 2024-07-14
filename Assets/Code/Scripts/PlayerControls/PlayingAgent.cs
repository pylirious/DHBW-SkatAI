using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

namespace Code.Scripts.PlayerControls
{
    public class PlayingAgent : Agent
    {
        private AiController _aiController;

        public void InitializeAgent(AiController aiController)
        {
            _aiController = aiController;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_aiController.PlayedCardsInCurrentTrick.ConvertAll(card => (float)card.GetId()));
            sensor.AddObservation(_aiController.Hand.ConvertAll(card => (float)card.GetId()));
            sensor.AddObservation(_aiController.AllCardsPlayedInRound.ConvertAll(card => (float)card.GetId()));
            sensor.AddObservation(_aiController.OptimalCards.ConvertAll(card => (float)card.GetId()));
            sensor.AddObservation(_aiController.HighestBid);
            sensor.AddObservation(_aiController.IsLonePlayer);
            sensor.AddObservation(_aiController.PointsSecured);
            sensor.AddObservation(_aiController.Round);
            sensor.AddObservation(_aiController.PlayersLeftToPlay);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var cardIndex = actions.DiscreteActions[0] % _aiController.Player.Hand.Count;
            var card = _aiController.Player.Hand[cardIndex];
            _aiController.Player.PlayedCard = card;
        }
        

        public void Feedback(bool trickWon, int howManyTrickPoints, List<Card> playedCardsInCurrentTrick, Card playedCard, List<Card> hand)
        {
            // Nullspiel-Szenario prüfen
            if (_aiController.GameType == GameType.NullGame && _aiController.IsLonePlayer)
            {
                if (trickWon)
                {
                    SetReward(-2); // Strafe für das Gewinnen eines Stiches im Nullspiel
                }
                return;
            }

            // Wenn der Stich gewonnen wurde
            if (trickWon)
            {
                // Alle möglichen Karten filtern, die den Stich gewinnen könnten
                List<Card> winningCards = hand.FindAll(card => 
                    (int)_aiController.GameType == (int)card.cardType && 
                    !playedCardsInCurrentTrick.Any(pCard => pCard.cardType == card.cardType && pCard.cardValue > card.cardValue)
                );

                // Prüfen, ob die gespielte Karte die niedrigste gewinnende Karte ist
                if (playedCard == winningCards.OrderBy(card => card.cardValue).FirstOrDefault())
                {
                    // Belohnung basierend auf der Position des Spielers im Stich
                    if (playedCardsInCurrentTrick.Count < 2 && (int)playedCard.cardValue > 3) // Frühe Position und hohe Karte
                    {
                        SetReward(-howManyTrickPoints / 2); // Strafe
                    }
                    else if (playedCardsInCurrentTrick.Count == 2 && howManyTrickPoints > 15) // Späte Position und viele Punkte
                    {
                        SetReward(howManyTrickPoints * 2); // Höhere Belohnung
                    }
                    else
                    {
                        SetReward(howManyTrickPoints); // Normale Belohnung
                    }
                }
                else
                {
                    SetReward(howManyTrickPoints / 2); // Teilbelohnung
                }
            }
            else
            {
                SetReward(-howManyTrickPoints); // Strafe für verlorenen Stich
            }
            EndEpisode();
        }
    }
}