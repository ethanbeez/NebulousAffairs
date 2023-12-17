using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player {
    public Leader Leader { get; private set; }

    #region Properties
    public Dictionary<string, TradeAction> OutstandingTrades { get; }
    public List<GameAction> PlayerActionHistory { get; }
    public bool CanAdvanceTurn {
        get {
            return PlayerTurnActionsLeft == 0 && OutstandingTrades.Count == 0;
        }
    }
    #endregion

    #region Game Constants
    private static int ActionsPerTurn = 3;
    #endregion

    public Player() {
        OutstandingTrades = new();
    }

    public Player(Leader leader) {
        Leader = leader;
        OutstandingTrades = new();
    }

    public void AssignLeader(Leader leader) {
        Leader = leader;
    }

    public int PlayerTurnActionsLeft { get; set; } = 3;

    public bool AddOutstandingTrade(TradeAction outstandingTrade) {
        if (!OutstandingTrades.ContainsKey(outstandingTrade.originLeader.Name)) {
            OutstandingTrades.Add(outstandingTrade.OriginLeader.Name, outstandingTrade);
            return true;
        }
        return false;
    }

    public void ProcessOutstandingTrade(string originLeader, bool accepted) {
        if (!OutstandingTrades.ContainsKey(originLeader)) { 
            Debug.LogError("Player.ProcessOutstandingTrade: There was no outstanding trade from the leader " + originLeader + ".");
        }
        TradeAction trade = OutstandingTrades[originLeader];
        trade.Accepted = accepted;
        if (accepted) Leader.AcceptIncomingTrade(trade);
        else Leader.RefuseIncomingTrade(trade);
        OutstandingTrades.Remove(originLeader);
    }

    public void ClearOutstandingTrades() { 
        foreach (TradeAction tradeAction in OutstandingTrades.Values) {
            Leader.RefuseIncomingTrade(tradeAction);
        }
        OutstandingTrades.Clear();
    }

    public void ResetPlayerActionsLeft() {
        PlayerTurnActionsLeft = ActionsPerTurn;
    }

    public void ProcessOutgoingTradeOutcome(TradeAction tradeAction) {
        Leader.ProcessOutgoingTradeOutcome(tradeAction);
    }

    public void HandleLeaderElimination(string leaderName) {
        Leader.ProcessLeaderElimination(leaderName);
        if (OutstandingTrades.ContainsKey(leaderName)) {
            OutstandingTrades.Remove(leaderName);
        }
    }
}

