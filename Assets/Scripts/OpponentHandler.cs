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

    /// <summary>
    /// Builds opponents with AI values interpolated from their leader's preference values.
    /// </summary>
    /// <param name="leaders"></param>
    public void BuildOpponentsFromLeaders(List<Leader> leaders) {
        foreach (Leader leader in leaders) {
            float politicsBias = (float) (leader.PoliticsPreference + 4) / 8;
            float affluenceBias = (float) (leader.AffluencePreference + 4) / 8;
            float intellectBias = (float) (leader.IntellectPreference + 4) / 8;
            Opponent opponent = new(leader, (float) random.NextDouble(), (float) random.NextDouble(), affluenceBias, politicsBias, intellectBias);
            opponents.Add(leader.Name, opponent);
        }
    }

    public void ProcessOpponentOnlyTradeAction(TradeAction tradeAction) {
        Opponent targetOpponent = opponents[tradeAction.TargetLeader.Name];
        Opponent originOpponent = opponents[tradeAction.OriginLeader.Name];
        targetOpponent.ProcessIncomingTrade(tradeAction);
        originOpponent.ProcessOutgoingTradeOutcome(tradeAction);
        /*string accepted = (tradeAction.Accepted) ? "accepted" : "refused";
        Debug.Log($"{originOpponent.Leader.Name} sent {targetOpponent.Leader.Name} a trade offer. They asked for:\n{tradeAction.RequestedAffluence} money\n{tradeAction.RequestedIntellect} intellect" +
            $"\nand {tradeAction.RequestedPolitics} politics.\nThey offered:\n{tradeAction.OfferedAffluence} money\n{tradeAction.OfferedIntellect} intellect\nand {tradeAction.OfferedPolitics} politics.\n" +
            $"{targetOpponent.Leader.Name} {accepted} the deal.");*/
    }

    public void ProcessPlayerInitiatedTradeAction(TradeAction tradeAction) {
        Opponent targetOpponent = opponents[tradeAction.TargetLeader.Name];
        targetOpponent.ProcessIncomingTrade(tradeAction);
    }

    public void HandleLeaderElimination(string leaderName) { 
        foreach (Opponent opponent in opponents.Values) {
            if (opponent.Leader.Name == leaderName) continue;
            opponent.ProcessLeaderElimination(leaderName);
            opponent.Leader.ProcessLeaderElimination(leaderName);
        }
    }

    public void EliminateOpponent(string leaderName) {
        opponents.Remove(leaderName);
    }
}
