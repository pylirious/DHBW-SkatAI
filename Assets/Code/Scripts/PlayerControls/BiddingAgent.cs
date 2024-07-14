using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Code.Scripts.PlayerControls
{
    public class BiddingAgent : Agent
    {
        private AiController _aiController;

        public void InitializeAgent(AiController aiController)
        {
            _aiController = aiController;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_aiController.Hand.ConvertAll(card => (float)card.GetId()));
            sensor.AddObservation(_aiController.CurrentHighestBid);
            sensor.AddObservation(_aiController.OptimalBid);
            sensor.AddObservation(_aiController.PlayersLeft);
            sensor.AddObservation(_aiController.BiddingRound);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            var bid = actions.DiscreteActions[0]; 
            _aiController.Player.CurrentBid = bid;
        }

        
        public void Feedback(int optimalBid, int ownBid, bool isLonePlayer, int[] otherPlayersHighestBids)
        {
            if (isLonePlayer)
            { 
                float reward = optimalBid - ownBid; 
                SetReward(reward);
            }
            else
            {
                float reward = otherPlayersHighestBids.Max() - ownBid;
                SetReward(reward);
            }
            EndEpisode();
        }
    }
}