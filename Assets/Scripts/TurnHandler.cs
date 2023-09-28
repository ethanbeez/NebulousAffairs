#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnHandler {
    private int turnLimit; // TODO: Remove
    private int startingYear;
    private int yearsPerTurn;
    private int turnsPerElection;

    private GameTurns gameTurns;
    public delegate void TurnChangeHandler(GameTurns gameTurns);
    public event TurnChangeHandler? TurnChanged;
    public delegate void ElectionOccurredHandler(GameTurns gameTurns, Election election);
    public event ElectionOccurredHandler? ElectionOccurred;
    public TurnHandler(int turnLimit = 20, int startingYear = 3000, int yearsPerTurn = 50, int turnsPerElection = 5) { 
        gameTurns = new(startingYear: startingYear, yearsPerTurn: yearsPerTurn, turnLimit: turnLimit);
        this.turnsPerElection = turnsPerElection;
        this.turnLimit = turnLimit;
        this.startingYear = startingYear;
        this.yearsPerTurn = yearsPerTurn;
    }

    public void AdvanceTurn() {
        gameTurns.AdvanceTurn();
        // TODO: Don't hardcode the -1; do something like gameTurns.CurrentTurn - starting turn to get a proper adaptable measure
         if ((gameTurns.CurrentTurn - 1) % turnsPerElection == 0) {
            ElectionOccurred?.Invoke(gameTurns, new Election(gameTurns.CurrentTurn - 1, gameTurns.CurrentYear - gameTurns.YearsPerTurn));
            return;
        }
        TurnChanged?.Invoke(gameTurns);
    }

    public struct GameTurns {
        #region Fields
        private int currentTurn;
        private int startingYear;
        private int currentYear;
        private int yearsPerTurn;
        private int turnLimit;
        private int yearLimit;
        #endregion

        #region Properties
        public int CurrentTurn => currentTurn;
        public int StartingYear => startingYear;
        public int CurrentYear => currentYear;
        public int YearsPerTurn => yearsPerTurn;
        public int TurnLimit => turnLimit;
        public int YearLimit => yearLimit;
        #endregion

        #region Constructors
        public GameTurns(int currentTurn = 1, int startingYear = 3000, int yearsPerTurn = 1, int turnLimit = 20) { // No parameterless struct constructors in C#9, not ideal syntax ):
            this.currentTurn = currentTurn;
            this.startingYear = startingYear;
            this.currentYear = startingYear;
            this.yearsPerTurn = yearsPerTurn;
            this.turnLimit = turnLimit;
            this.yearLimit = startingYear + (turnLimit * yearsPerTurn);
        }
        #endregion

        #region Turn Advancement
        public void AdvanceTurn(int turnsToAdvanceBy = 1) {
            currentTurn += turnsToAdvanceBy;
            currentYear += (turnsToAdvanceBy * yearsPerTurn);
        }
        #endregion

        #region ToStrings
        public override string ToString() {
            return $"Turn: {currentTurn}/{turnLimit}, Year: {currentYear}/{yearLimit}";
        }
        #endregion
    }
}
