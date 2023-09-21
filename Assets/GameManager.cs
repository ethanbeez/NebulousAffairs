using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour {
    public const int StartingPlanetsPerLeader = 2; 
    GameHandler gameHandler;
    TurnHandler turnHandler;
    // Start is called before the first frame update
    void Start() {
        gameHandler = new(StartingPlanetsPerLeader);
        turnHandler = new();
        turnHandler.TurnChanged += AdvanceTurn;
        turnHandler.ElectionOccurred += AdvanceElectionTurn;
    }

    private void AdvanceTurn(TurnHandler.GameTurns gameTurns) {
        Debug.Log(gameTurns.ToString());
        gameHandler.ExecuteBotTurns(gameTurns);
    }

    private void AdvanceElectionTurn(TurnHandler.GameTurns gameTurns, Election electionData) {
        AdvanceTurn(gameTurns);
        gameHandler.ProcessElection(electionData);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            turnHandler.AdvanceTurn();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 1"));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 2"));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 3"));
        }


        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 4"));
        }


        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 5"));
        }


        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 6"));
        }
    }
}
