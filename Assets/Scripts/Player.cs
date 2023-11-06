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
        
    }

    public Player(Leader leader) {
        Leader = leader;
    }

    public void AssignLeader(Leader leader) {
        Leader = leader;
    }

    public int PlayerTurnActionsLeft { get; set; }

    public void AddOutstandingTrade(TradeAction outstandingTrade) {
        OutstandingTrades.Add(outstandingTrade.OriginLeader.Name, outstandingTrade);
    }

    public void ProcessOutstandingTrade(string originLeader, bool accepted) {
        if (!OutstandingTrades.ContainsKey(originLeader)) { 
            Debug.LogError("Player.ProcessOutstandingTrade: There was no outstanding trade from the leader " + originLeader + ".");
        }
        TradeAction trade = OutstandingTrades[originLeader];
        trade.Accepted = accepted;
        if (accepted) Leader.AcceptIncomingTrade(trade);
        else Leader.RefuseIncomingTrade(trade);
    }

    public void ResetPlayerActionsLeft() {
        PlayerTurnActionsLeft = ActionsPerTurn;
    }

    public void ProcessPlayerAction(GameAction action) { 
    
    }

    public void HandleLeaderElimination(string leaderName) {
        Leader.ProcessLeaderElimination(leaderName);
    }
}

