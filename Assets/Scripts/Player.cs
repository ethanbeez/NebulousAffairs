using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player {
    
    public Leader Leader { get; }

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
    public int PlayerTurnActionsLeft { get; set; }

    public void AddOutstandingTrade(TradeAction outstandingTrade) {
        OutstandingTrades.Add(outstandingTrade.OriginLeader.Name, outstandingTrade);
    }

    public void ProcessOutstandingTrade(TradeAction outstandingTrade) {
        if (!OutstandingTrades.ContainsKey(outstandingTrade.OriginLeader.Name)) { 
            Debug.LogError("Player.ProcessOutstandingTrade: There was no outstanding trade from the leader " +  outstandingTrade.OriginLeader.Name + ".");
        }

    }

    public void ResetPlayerActionsLeft() {
        PlayerTurnActionsLeft = ActionsPerTurn;
    }

    public Player(Leader leader) {
        Leader = leader;
    }



    public void ProcessPlayerAction(GameAction action) { 
    
    }
}

