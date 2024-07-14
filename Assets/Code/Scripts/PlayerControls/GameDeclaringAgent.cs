using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

namespace Code.Scripts.PlayerControls
{
    public class GameDeclaringAgent : Agent
    {
        private AiController _aiController;

        public void InitializeAgent(AiController aiController)
        {
            _aiController = aiController;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_aiController.Hand.ConvertAll(card => (float)card.GetId()));
            sensor.AddObservation(_aiController.OwnBid);
            sensor.AddObservation(new List<int>(_aiController.OtherPlayersHighestBids).ConvertAll(bid => (float)bid));
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var game = actions.DiscreteActions[0];
            _aiController.Player.GameType = (GameType)game;
        }


        public void Feedback(GameType chosenGameType)
        {
            int matadors = 0;
            int[] suits = new int[4];
            
            // Count the matadors and the suits
            foreach (var card in _aiController.Hand)
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
                if (chosenGameType == GameType.Grand)
                {
                    SetReward(1);
                }
                else
                {
                    SetReward(-1);
                }
                return;
            }
            
            // Check if the player has 3 or more cards of the same suit
            int maxNumberSuit = 2;
            for (int i = 0; i < suits.Length; i++)
            {
                if (suits[i] > maxNumberSuit)
                {
                    maxNumberSuit = i;
                    if (chosenGameType == (GameType)i)
                    {
                        SetReward(1);
                    }
                    else
                    {
                        SetReward(-1);
                    }
                }
            }
            if(maxNumberSuit != 2) return;
            
            // If no other option is available, declare a null game
            if (chosenGameType == GameType.NullGame)
            {
                SetReward(1);
            }
            else
            {
                SetReward(-1);
            }
        }
    }
}