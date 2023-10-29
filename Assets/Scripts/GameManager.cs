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
        CheckMissingGameDataFiles();
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
        ButtonManager.LeaderButtonPressed += HandleLeaderClick;
        PlanetController.PlanetClicked += HandlePlanetClick;

        uiController.playerLeader = gameHandler.GetPlayerLeader();
        uiController.updateTurnDisplay(turnHandler.GetCurrentTurnInfo());
        // turnHandler.TurnChanged += AdvanceTurn;
        // turnHandler.ElectionOccurred += AdvanceElectionTurn;
    }

    private void HandleLeaderClick(int index)
    {
        //access leader info
        //uiController.RenderLeaderInfo();
    }

    private void CheckMissingGameDataFiles() {
        List<string> missingFiles = FileManager.GetMissingGameDataFiles();
        if (missingFiles.Count == 0) return;
        string missingFilesError = "GameManager.CheckMissingGameDataFiles: Game data files were missing! The following files were missing: ";
        foreach (string file in missingFiles) {
            missingFilesError += $"\n{file}";
        }
        Debug.LogError(missingFilesError);
        QuitGame();
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
        uiController.RenderPlanetInfo(clickedPlanet, gameHandler.GetPlanetInfluenceRatios(planetName));
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
        uiController.updateTurnDisplay(gameTurnInfo.ToString());
    }

    /*private void AdvanceElectionTurn(TurnHandler.GameTurns gameTurns, Election electionData) {
        AdvanceTurn(gameTurns);
        gameHandler.ProcessElection(electionData);
    }*/

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Aris Yve"));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Krayxic"));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Mother Stacy"));
        }


        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Nyco Harp"));
        }


        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Fortuna"));
        }


        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Foran Jes"));
        }
    }
}
