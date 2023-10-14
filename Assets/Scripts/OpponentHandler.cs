using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentHandler {
    private System.Random random;
    public Dictionary<string, Opponent> opponents;
    public OpponentHandler() {
        random = new();
        opponents = new();
    }

    public void BuildRandomOpponents(int opponentsToCreate) {
        for (int i = 1; i <= opponentsToCreate; i++) {
            Leader leader = new("Leader " + (i + 1), 10, 10, 10);

            Opponent opponent = new(leader, (float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble());
            opponents.Add(leader.Name, opponent);
        }
    }

    public void ProcessOpponentOnlyTradeAction(TradeAction tradeAction) {
        Opponent targetOpponent = opponents[tradeAction.TargetLeader.Name];
        Opponent originOpponent = opponents[tradeAction.OriginLeader.Name];
        targetOpponent.ProcessIncomingTrade(tradeAction);
        originOpponent.ProcessOutgoingTradeOutcome(tradeAction);
        string accepted = (tradeAction.Accepted) ? "accepted" : "refused";
        Debug.Log($"{originOpponent.Leader.Name} sent {targetOpponent.Leader.Name} a trade offer. They asked for {tradeAction.RequestedAffluence} money, {tradeAction.RequestedIntellect} intellect, " +
            $"and {tradeAction.RequestedPolitics} politics; they offered {tradeAction.OfferedAffluence} money, {tradeAction.OfferedIntellect} intellect, and {tradeAction.RequestedPolitics} politics. " +
            $"{targetOpponent.Leader.Name} {accepted} the deal.");
    }

    public void ProcessPlayerOpponentTradeAction(TradeAction tradeAction) { 
        
    }
}
