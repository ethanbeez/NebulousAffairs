using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using static TurnHandler;

public enum GameState {
    Default = 0,
    MainMenu = 1,
    InMatch = 2,
    Paused = 3,
    End = 4,
}

public class GameManager : MonoBehaviour {
    public static GameState GameState { get; private set; }
    public const int NumLeaders = 6;
    public const int StartingPlanetsPerLeader = 2;
    private GameHandler gameHandler;
    private TurnHandler turnHandler;
    public NebulaController nebulaController;
    public UIController uiController;
    // Start is called before the first frame update
    void Start() {
        GameState = GameState.MainMenu;
        CheckMissingGameDataFiles();
        // gameHandler = new(StartingPlanetsPerLeader);
        // turnHandler = new();
        if (nebulaController == null) {
            nebulaController = GameObject.Find("NebulaController").GetComponent<NebulaController>();
        }
        if (uiController == null) {
            uiController = GameObject.Find("UIController").GetComponent<UIController>();
        }
        // uiController.InstantiateButtons(gameHandler.GetLeaderButtonData());
        // GenerateNebula();
        InputManager.MPressed += CameraToMapPosition;
        // InputManager.SPressed += ToggleNebulaOrbits;
        // InputManager.TPressed += ToggleGalaxyMotionTrails;
        InputManager.EscapePressed += uiController.Back;
        InputManager.SpacePressed += HandleTurnAdvancement;
        LeaderButton.LeaderButtonPressed += HandleLeaderClick;
        LeaderButton.LeaderButtonEnterOrExit += HandleLeaderHover;
        PlanetController.PlanetClicked += HandlePlanetClick;
        TradeUIController.TradeConfirmPressed += HandlePlayerTrade;
        Campaign.ConfirmCampaign += HandlePlayerCampaign;
        UIController.ConfirmEspionage += HandlePlayerEspionage;
        UIController.GameQuit += QuitGame;
        UIController.GameStart += StartMatch;
        DialogueController.DialogueRequest += HandleDialogue;
        UIController.GamePaused += PauseGame;
        UIController.GameUnpaused += UnpauseGame;

        // COMMENTED OUT FOR MAIN MENU LOGIC
        /*gameHandler.gameHistory.LogGameEvent(new("Turn: 1/20, Year: 3000/ 4000"));
        uiController.playerLeader = gameHandler.GetPlayerLeader();
        uiController.UpdateTurnDisplay(turnHandler.GetCurrentTurnInfo(), gameHandler.GetNotifications());
        uiController.UpdateLog(gameHandler.GetEventHistory());
        uiController.player = gameHandler.player;*/
        // turnHandler.TurnChanged += AdvanceTurn;
        // turnHandler.ElectionOccurred += AdvanceElectionTurn;
    }

    private void PauseGame() { 
        GameState = GameState.Paused;
    }

    private void UnpauseGame() {
        GameState = GameState.InMatch;
    }

    private void StartMatch() {
        GameState = GameState.InMatch;
        gameHandler = new(StartingPlanetsPerLeader);
        turnHandler = new();
        GenerateNebula();
        gameHandler.gameHistory.LogGameEvent(new("Turn: 1/20, Year: 3000/ 4000"));
        uiController.playerLeader = gameHandler.GetPlayerLeader();
        uiController.player = gameHandler.player;
        uiController.UpdateTurnDisplay(turnHandler.GetCurrentTurnInfo(), gameHandler.GetNotifications());
        uiController.UpdateLog(gameHandler.GetEventHistory());
        uiController.InstantiateButtons(gameHandler.GetLeaderButtonData());
    }

    private void HandleLeaderHover(string leaderName, bool isEnter)
    {
        if(isEnter) {
            nebulaController.AddLeaderHovers(leaderName);
            return;
        }
        nebulaController.RemoveLeaderHovers(leaderName);
    }

    private void HandleLeaderClick(string leaderName)
    {
        Leader opponentLeader = gameHandler.GetOpponentLeader(leaderName);
        if (opponentLeader != null) {
            uiController.RenderLeaderInfo(gameHandler.GetOpponentLeader(leaderName));
        }
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
        // nebulaController.ToggleGalaxyMotionTrails();
    }

    private void GenerateNebula() {
        if (nebulaController.nebulaGenerated == false) {
            GeneratePlanets();
            nebulaController.nebulaGenerated = true;
        }
    }

    private void CameraToMapPosition() {
        if (GameState == GameState.InMatch) {
            uiController.RenderMainScene();
        }
    }

    private void ToggleNebulaOrbits() {
        // nebulaController.ToggleNebulaMotion();
    }

    private void GeneratePlanets() {
        List<(int ID, string planetName)> planetsInfo = new();
        foreach (Planet planet in gameHandler.GetPlanets()) {
            planetsInfo.Add((planet.ID, planet.Name));
        }
        nebulaController.InstantiateNebula(NumLeaders, StartingPlanetsPerLeader, planetsInfo);
    }

    private void HandlePlanetClick(int planetID, GameObject focusTarget, string planetName) {
        if (GameState == GameState.InMatch) {
            Planet clickedPlanet = gameHandler.GetPlanet(planetName);
            uiController.RenderPlanetInfo(clickedPlanet, gameHandler.GetPlanetInfluenceRatios(planetName), focusTarget);
        }
    }


    private void HandleTurnAdvancement() {
        if (GameState == GameState.InMatch) {
            nebulaController.StartTurnTransitionAnim();
            TurnHandler.GameTurns gameTurnInfo = turnHandler.AdvanceTurn();
            gameHandler.AdvanceTurn(gameTurnInfo);
            if (gameTurnInfo.ElectionTurn) {
                Election election = new(gameTurnInfo.CurrentTurn - 1, gameTurnInfo.CurrentYear - gameTurnInfo.YearsPerTurn);
                List<string> eliminatedLeaders = gameHandler.ProcessElection(election);

                if (eliminatedLeaders.Contains(gameHandler.player.Leader.Name)) {
                    HandlePlayerLoss();
                }
                foreach (string leader in eliminatedLeaders) {
                    uiController.UpdateEliminatedLeaderButton(leader);
                }
            }
            Debug.Log(gameTurnInfo.CurrentTurn);

            if (gameTurnInfo.CurrentTurn > gameTurnInfo.TurnLimit) {
                gameHandler.CheckLeaderWon();
                uiController.EndGame(gameHandler.GetEventHistory());
            }
            uiController.UpdateTurnDisplay(gameTurnInfo.ToString(), gameHandler.GetNotifications());
            gameHandler.gameHistory.LogGameEvent(new(gameTurnInfo.ToString()));
            uiController.UpdateLog(gameHandler.GetEventHistory());
        }
    }

    private void HandlePlayerLoss() {
        // gameHandler.gameHistory.LogGameEvent(new("You lost control of all planets! Press ESC to quit and relaunch the game to try again."));
        // Application.Quit();
    }

    private void HandlePlayerTrade(int[] vals, Leader enemyLeader) {
        TradeAction trade = new TradeAction(0, gameHandler.GetPlayerLeader(), enemyLeader, vals[2], vals[0], vals[1], vals[5], vals[3], vals[4] );
        if(gameHandler.CheckPlayerCanAffordTrade() && gameHandler.GetPlayerActionsLeft() > 0) {
            if(gameHandler.ProcessPlayerInitiatedTrade(trade)) {
                uiController.AddToConverse("Trade Successful!");
            }
            else {
                uiController.AddToConverse("Trade Failed");
            }
        }
        else {
            uiController.AddToConverse("Trade Failed");
        }
        uiController.RenderLeaderInfo(enemyLeader);
        uiController.UpdateActionDisplay(gameHandler.GetPlayerActionsLeft());
        uiController.UpdateLog(gameHandler.GetEventHistory());
    }

    private void HandlePlayerCampaign(CurrencyType increased, CurrencyType decreased, Planet planet)
    {
        if(gameHandler.CheckPlayerCanAffordDiplomacy() && gameHandler.GetPlayerActionsLeft() > 0) {
            gameHandler.ProcessPlayerDiplomacy(planet.Name, increased, decreased);
            uiController.UpdateActionDisplay(gameHandler.GetPlayerActionsLeft());
            uiController.RenderPlanetInfo(planet, gameHandler.GetPlanetInfluenceRatios(planet.Name), null);
        }
        else
        {
            uiController.AddToLog("Campaign Failed, Not enough resources");
        }
    }

    private void HandlePlayerEspionage(CurrencyType resource, Leader leader) {

        if(gameHandler.CheckPlayerCanAffordEspionage() && gameHandler.GetPlayerActionsLeft() > 0) {
            gameHandler.ProcessPlayerEspionage(leader.Name, resource);
            uiController.RenderLeaderInfo(leader);
            uiController.UpdateActionDisplay(gameHandler.GetPlayerActionsLeft());
            uiController.UpdateLog(gameHandler.GetEventHistory());
        }
        else {
            uiController.AddToLog("Espionage Failed - Not Enough Resources");
        }
    }

    private void HandleDialogue(DialogueController dialogueController, string leaderName)
    {
        dialogueController.EngageDialogue(gameHandler.GetValidDialogueQuestions(leaderName));
    }



    /*private void AdvanceElectionTurn(TurnHandler.GameTurns gameTurns, Election electionData) {
        AdvanceTurn(gameTurns);
        gameHandler.ProcessElection(electionData);
    }*/

    // Update is called once per frame
    void Update() {
    }
}
