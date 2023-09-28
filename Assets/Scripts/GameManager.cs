using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour {
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

        PlanetController.PlanetClicked += HandlePlanetClick;
        turnHandler.TurnChanged += AdvanceTurn;
        turnHandler.ElectionOccurred += AdvanceElectionTurn;
    }

    private void GenerateNebula() {
        GeneratePlanets();
    }

    private void GeneratePlanets() {
        List<(int ID, string planetName)> planetsInfo = new();
        foreach (Planet planet in gameHandler.GetPlanets()) {
            planetsInfo.Add((planet.ID, planet.Name));
        }
        nebulaController.InstantiatePlanets(planetsInfo);
    }

    private void HandlePlanetClick(int planetID, Vector3 location, string planetName) {
        // ORION: Insert logic that renders UI that pops up when a planet is clicked here.
        Planet clickedPlanet = gameHandler.GetPlanet(planetName);

        // Below are some example calls for getting Planet information.
        // clickedPlanet.AffluenceYield, clickedPlanet.IntelligenceYield, clickedPlanet.PoliticsYield (per turn yield values)
        // clickedPlanet.AffluencePriority, clickedPlanet.IntelligencePriority, clickedPlanet.PoliticsPriority (how much they like each yield)
        // clickedPlanet.CurrentLeader.Name; (the name of the current leader of the planet)
        // clickedPlanet.GetLeaderInfluenceValue(clickedPlanet.CurrentLeader.Name); (the influence value of the current leader of the planet)

        // If you can, please write your UI code such that it is rendered via a UIController method. For example, if you can write this method, this would be great:
        // uiController.RenderPlanetInfo(clickedPlanet)
        cameraController.StartFly(location, Quaternion.identity);
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
