using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TurnHandler : MonoBehaviour {
    [Header("Turn Settings")]
    [Range(1, 500)]
    public int TurnLimit;
    [Range(1, 10_000)]
    public int StartingYear;
    [Range(1, 1000)]
    public int YearsPerTurn;

    private GameTurns gameTurns;
    public static UnityAction<GameTurns> OnTurnChanged;
    // Start is called before the first frame update
    void Start() {
        gameTurns = new(startingYear: StartingYear, yearsPerTurn: YearsPerTurn, turnLimit: TurnLimit);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            AdvanceTurn();
            Debug.Log(gameTurns.ToString());
        }
    }

    void AdvanceTurn() {
        gameTurns.AdvanceTurn();
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
        public GameTurns(int currentTurn = 1, int startingYear = 3000, int yearsPerTurn = 1, int turnLimit = 100) { // No parameterless struct constructors in C#9, not ideal syntax ):
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
