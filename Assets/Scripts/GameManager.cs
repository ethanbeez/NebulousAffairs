using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using static TurnHandler;

public class GameManager : MonoBehaviour {
    public const int NumLeaders = 6;
    public const int StartingPlanetsPerLeader = 2; 
    private GameHandler gameHandler;
    private TurnHandler turnHandler;
    public CameraController cameraController;
    public NebulaController nebulaController;
    public UIController uiController;
    // Start is called before the first frame update
    void Start() {
        gameHandler = new(StartingPlanetsPerLeader);
        turnHandler = new();
        if (cameraController == null) {
            cameraController = Camera.main.GetComponent<CameraController>();
        }
        if (nebulaController == null) {
            nebulaController = GameObject.Find("NebulaController").GetComponent<NebulaController>();
        }
        if (uiController == null) {
            uiController = GameObject.Find("UIController").GetComponent<UIController>();
        }

        GenerateNebula();
        InputManager.MPressed += CameraToMapPosition;
        InputManager.SPressed += ToggleNebulaOrbits;
        InputManager.TPressed += ToggleGalaxyMotionTrails;
        InputManager.EscapePressed += QuitGame;
        InputManager.SpacePressed += HandleTurnAdvancement;
        PlanetController.PlanetClicked += HandlePlanetClick;

        uiController.updateTurnDisplay(turnHandler.GetCurrentTurnInfo(), gameHandler.GetPlayerPlanetsControlled(), 0);
        uiController.playerLeader = gameHandler.GetPlayerLeader();
        // turnHandler.TurnChanged += AdvanceTurn;
        // turnHandler.ElectionOccurred += AdvanceElectionTurn;
    }

    private void QuitGame() {
        Application.Quit();
    }

    private void ToggleGalaxyMotionTrails() {
        nebulaController.ToggleGalaxyMotionTrails();
    }

    private void GenerateNebula() {
        GeneratePlanets();
    }

    private void CameraToMapPosition() {
        cameraController.StartMapFly();
        uiController.RenderMainScene(1.8f);
    }

    private void ToggleNebulaOrbits() {
        nebulaController.ToggleNebulaMotion();
    }

    private void GeneratePlanets() {
        List<(int ID, string planetName)> planetsInfo = new();
        foreach (Planet planet in gameHandler.GetPlanets()) {
            planetsInfo.Add((planet.ID, planet.Name));
        }
        nebulaController.InstantiateNebula(NumLeaders, StartingPlanetsPerLeader, planetsInfo);
    }

    private void HandlePlanetClick(int planetID, GameObject focusTarget, string planetName) {
        Planet clickedPlanet = gameHandler.GetPlanet(planetName);
        uiController.RenderPlanetInfo(clickedPlanet, 3.4f, gameHandler.GetPlanetInfluenceRatios(planetName));
        cameraController.StartFly(focusTarget);
    }

    private void HandleTurnAdvancement() {


        nebulaController.StartTurnTransitionAnim();
        TurnHandler.GameTurns gameTurnInfo = turnHandler.AdvanceTurn();
        gameHandler.AdvanceTurn(gameTurnInfo);
        if (gameTurnInfo.ElectionTurn) {
            Election election = new(gameTurnInfo.CurrentTurn - 1, gameTurnInfo.CurrentYear - gameTurnInfo.YearsPerTurn);
            gameHandler.ProcessElection(election);
        }
        // TODO: Remove following, for rush proto
        int won = 0;
        if (gameTurnInfo.CurrentTurn > 20) {
            if (gameHandler.GetPlayerPlanetsControlled() < 7) won = 1;
            if (gameHandler.GetPlayerPlanetsControlled() >= 7) won = 2;
        } else {
            if (gameHandler.GetPlayerPlanetsControlled() == 0) won = 1;
            if (gameHandler.GetPlayerPlanetsControlled() == 12) won = 2;
        }
        uiController.updateTurnDisplay(gameTurnInfo.ToString(), gameHandler.GetPlayerPlanetsControlled(), won);
    }

    /*private void AdvanceElectionTurn(TurnHandler.GameTurns gameTurns, Election electionData) {
        AdvanceTurn(gameTurns);
        gameHandler.ProcessElection(electionData);
    }*/

    // Update is called once per frame
    void Update() {
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
