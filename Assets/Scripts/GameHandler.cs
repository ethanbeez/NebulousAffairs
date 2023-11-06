using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEditor;
using UnityEngine;

public class GameHandler {
    #region Game Constants
    public const int TotalSessionPlayers = 6;
    #endregion
    Player player;
    OpponentHandler opponentHandler;
    PlanetHandler planetHandler;
    
    public GameHandler(int leadersPerPlanet) {
        opponentHandler = new();
        player = new();
        BuildLeadersFromGameData();
        planetHandler = new();
        BuildPlanetsFromGameData();
        BuildConnections(leadersPerPlanet);
    }

    private void BuildLeadersFromGameData() {
        List<Leader> leaders = new();
        Dictionary<string, Dictionary<CurrencyType, int>> leaderPreferences = GameDataManager.LeaderPreferences;
        foreach (KeyValuePair<string, Dictionary<CurrencyType, int>> leaderData in leaderPreferences) {
            Leader leader = new(leaderData.Key, leaderData.Value[CurrencyType.Affluence], leaderData.Value[CurrencyType.Politics], leaderData.Value[CurrencyType.Intellect]);
            leaders.Add(leader);
        }
        player.AssignLeader(leaders[5]); // TODO: Assuming the spreadsheet parse order is not changed, this should be Foran Jes (as of 10/14).
        opponentHandler.BuildOpponentsFromLeaders(leaders.GetRange(0, 5));
    }

    private void BuildPlanetsFromGameData() {
        Dictionary<string, Dictionary<CurrencyType, int>> planetPreferences = GameDataManager.PlanetPreferences;
        foreach (KeyValuePair<string, Dictionary<CurrencyType, int>> planetData in planetPreferences) {
            Planet planet = new(planetData.Key, planetData.Value[CurrencyType.Affluence], planetData.Value[CurrencyType.Politics], planetData.Value[CurrencyType.Intellect]);
            planetHandler.AddPlanet(planet);
        }
    }

    public List<(string, string)> GetLeaderButtonData() {
        List<(string, string)> leaderButtonData = new();
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            Leader opponentLeader = opponent.Leader;
            (string, string) buttonData = (opponentLeader.Name, opponentLeader.GetLeaderImagePath(LeaderResources.Perspectives.Portrait, LeaderResources.Expressions.Neutral)); 
            leaderButtonData.Add(buttonData);
        }
        return leaderButtonData;
    }

    public void BuildConnections(int leadersPerPlanet) {
        BuildInfluenceProfiles();
        BuildPlanetLeaders();
        BuildRelationshipProfiles();
        BuildVisibilities();
        BuildInitialPriorities();
    }

    private void BuildVisibilities() {
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            Leader opponentLeader = opponent.Leader;
            LeaderVisibility leaderVisibility = new(player.Leader, opponentLeader);
            player.Leader.AddNewLeaderVisiblity(leaderVisibility);
        }

        foreach (Opponent opponent1 in opponentHandler.opponents.Values) {
            Leader opponentLeader1 = opponent1.Leader;
            foreach (Opponent opponent2 in opponentHandler.opponents.Values) {
                if (opponent1 == opponent2) continue;
                Leader opponentLeader2 = opponent2.Leader;
                LeaderVisibility leaderVisibility = new(opponentLeader1, opponentLeader2);
                opponentLeader1.AddNewLeaderVisiblity(leaderVisibility);
            }
        }
    }

    public void BuildInfluenceProfiles() {
        // Adding influence relationships for the player's leader.
        foreach (Planet planet in planetHandler.planets.Values) {
            Influence influence = new(player.Leader, planet);
            player.Leader.AddNewInfluence(influence);
            planet.AddNewInfluence(influence);
        }

        // Adding influence relationships for each opponent's leader.
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            Leader opponentLeader = opponent.Leader;
            foreach (Planet planet in planetHandler.planets.Values) {
                Influence influence = new(opponentLeader, planet);
                opponentLeader.AddNewInfluence(influence);
                planet.AddNewInfluence(influence);
            }
        }
    }

    public void BuildRelationshipProfiles() {
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            Leader opponentLeader = opponent.Leader;
            Relationship relationship = new(player.Leader, opponentLeader);
            player.Leader.AddNewRelationship(relationship);
        }

        foreach (Opponent opponent1 in opponentHandler.opponents.Values) {
            Leader opponentLeader1 = opponent1.Leader;
            foreach (Opponent opponent2 in opponentHandler.opponents.Values) {
                if (opponent1 == opponent2) continue;
                Leader opponentLeader2 = opponent2.Leader;
                Relationship relationship = new(opponentLeader1, opponentLeader2);
                opponentLeader1.AddNewRelationship(relationship);
            }
        }
    }

    public void BuildPlanetLeaders() {
        List<Planet> planetList = planetHandler.planets.Values.ToList();
        Dictionary<string, string> initialPlanetLeaders = GameDataManager.InitialPlanetLeaders;
        foreach (Planet planet in planetList) {
            string initialLeaderName = initialPlanetLeaders[planet.Name];
            Leader initialLeader;
            if (initialLeaderName == player.Leader.Name) {
                initialLeader = player.Leader;
            } else {
                initialLeader = opponentHandler.opponents[initialLeaderName].Leader;
            }
            BuildPlanetLeader(initialLeader, planet);
        }
    }

    private void BuildPlanetLeader(Leader planetLeader, Planet planet) {
        planetLeader.SetPlanetInfluence(planet.Name, 0.6f);
        planet.SetCurrentLeader(planetLeader);
        planetLeader.GainPlanetControl(planet);
    }

    public void BuildInitialPriorities() {
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            opponent.UpdateYieldPriorities();
            opponent.UpdatePlanetPriorities();
            opponent.UpdateLeaderPriorities();
        }
    }
    #region Player Game Actions
    public void ProcessPlayerInitiatedTrade() { 
    
    }

    public void ProcessPlayerOutstandingTrade(string originLeader, bool accepted) {
        player.ProcessOutstandingTrade(originLeader, accepted);
    }

    public void UpdatePlanetLeader(Planet planet, Leader leader) {
        leader.GainPlanetControl(planet);
    }
    #endregion

    public List<Planet> GetPlanets() {
        return planetHandler.planets.Values.ToList();
    }


    /// <summary>
    /// Returns all outstanding trades sent to the player on the current turn. The outstanding trades are stored in a Dictionary
    /// mapping the origin leader's name (key) to their offered trade (value).
    /// </summary>
    /// <returns>All outstanding trades sent to the player on the current turn</returns>
    public Dictionary<string, TradeAction> GetOutstandingTrades() {
        return player.OutstandingTrades;
    }

    /// <summary>
    /// Returns the outstanding trade from the Leader corresponding to the leader name parameter, if one exists. If no such outstanding 
    /// trade exists, this method returns null.
    /// </summary>
    /// <param name="leaderName">The name of the Leader to get the outstanding treade of</param>
    /// <returns>The outstanding trade corresponding to the input Leader name</returns>
    public TradeAction GetOutstandingTrade(string leaderName) {
        return player.OutstandingTrades[leaderName];   
    }

    public Planet GetPlanet(string planetName) {
        if (planetHandler.planets.TryGetValue(planetName, out Planet planet)) {
            return planet;
        }
        Debug.LogError("GameHandler.GetPlanet failed to retrieve the planet with name " + planetName + "!");
        throw new Exception();
    }

    public Planet GetPlanet(int planetID) {
        throw new NotImplementedException();
    }

    public int GetPlayerPlanetsControlled() {
        return player.Leader.PlanetControlCount;
    }

    public void AdvanceTurn(TurnHandler.GameTurns gameTurns) {
        AccrueLeaderYields();
        ExecuteOpponentTurns(gameTurns);
        player.ResetPlayerActionsLeft();
    }

    private void AccrueLeaderYields() {
        player.Leader.AccrueYields();
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            opponent.Leader.AccrueYields();
        }
    }

    public void ExecuteOpponentTurns(TurnHandler.GameTurns gameTurns) {
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            opponent.UpdateYieldPriorities();
            if (gameTurns.ElectionTurn) {
                opponent.UpdatePlanetPriorities();
                opponent.UpdateLeaderPriorities();
            }
            for (int i = 0; i < Opponent.ActionsPerTurn; i++) {
                GameAction opponentAction = opponent.MakeDecision();
                switch (opponentAction) {
                    case DiplomacyAction diplomacy:
                        HandleDiplomacyAction(diplomacy);
                        break;
                    case EspionageAction espionage:
                        HandleEspionageAction(espionage);
                        break;
                    case TradeAction trade:
                        HandleTradeAction(trade);
                        break;
                }
            }
        }
    }

    public List<(Leader, float)> GetPlanetInfluenceRatios(string planetName) {
        return Election.GetPlanetInfluenceRatios(planetHandler.planets[planetName]);
    }

    public List<(Planet, float)> GetLeaderInfluences(string leaderName) {
        Leader leader = opponentHandler.opponents[leaderName].Leader;
        List<(Planet, float)> planetInfluences = new();
        // TODO: Not exactly optimal code here.
        List<Influence> influences = leader.GetAllInfluences();
        foreach (Influence influence in influences) {
            planetInfluences.Add((influence.Planet, influence.InfluenceValue));
        }
        planetInfluences.Sort((planetInfluence1, planetInfluence2) => planetInfluence2.Item2.CompareTo(planetInfluence1.Item2));
        return planetInfluences;
    }

    public bool ProcessElection(Election electionData) {
        player.Leader.IncurElectionTurnCosts();
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            opponent.Leader.IncurElectionTurnCosts();
        }
        foreach (Planet planet in planetHandler.planets.Values) {
            electionData.DeterminePlanetElectionOutcome(planet);
        }
        foreach (KeyValuePair<string, HashSet<Planet>> gainedPlanets in electionData.GainedPlanetLeaders) {
            foreach (Planet gainedPlanet in gainedPlanets.Value) {
                opponentHandler.opponents[gainedPlanets.Key].Leader.GainPlanetControl(gainedPlanet);
            }
        }
        foreach (KeyValuePair<string, HashSet<Planet>> lostPlanets in electionData.LostPlanetLeaders) {
            foreach (Planet lostPlanet in lostPlanets.Value) {
                if (lostPlanets.Key == player.Leader.Name) {
                    player.Leader.LosePlanetControl(lostPlanet);
                } else {
                    opponentHandler.opponents[lostPlanets.Key].Leader.LosePlanetControl(lostPlanet);
                }
            }
        }
        List<string> eliminatedLeaderNames = new();
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            if (opponent.Leader.PlanetControlCount == 0) {
                Debug.Log(opponent.Leader.Name + " WAS ELIMINATED!");
                planetHandler.HandleLeaderElimination(opponent.Leader.Name);
                player.HandleLeaderElimination(opponent.Leader.Name);
                opponentHandler.HandleLeaderElimination(opponent.Leader.Name);
                eliminatedLeaderNames.Add(opponent.Leader.Name);
            }
        }
        // Additional loop necessary to handle concurrent modification.
        foreach (string eliminatedLeaderName in eliminatedLeaderNames) {
            opponentHandler.EliminateOpponent(eliminatedLeaderName);
        }
        if (player.Leader.PlanetControlCount == 0) {
            return true;
        }
        return false;
        /*Dictionary<string, List<(Leader, float)>> ratios = Election.GetPlanetsInfluenceRatios(planetHandler.planets.Values.ToList());
        // TODO: FOLLOWING IS DEBUG ONLY!
        StringBuilder sb = new();
        foreach (KeyValuePair<string, List<(Leader, float)>> kvp in ratios) {
            sb.Append(kvp.Key);
            sb.Append(": ");
            foreach ((Leader, float) leaderWeight in kvp.Value) {
                sb.Append(leaderWeight.Item1.Name);
                sb.Append("\t");
                sb.Append(leaderWeight.Item2);
                sb.Append("\n");
            }
            sb.Append("\n");
        }
        Debug.Log(sb.ToString());*/
    }

    private void HandleDiplomacyAction(DiplomacyAction diplomacyAction) {
        Planet targetPlanet = planetHandler.planets[diplomacyAction.TargetPlanet.Name];
        targetPlanet.HandleDiplomacyAction(diplomacyAction);
    }

    private void HandleEspionageAction(EspionageAction espionageAction) {
        throw new NotImplementedException("Espionage action is not implemented to be handled yet.");
    }

    private void HandleTradeAction(TradeAction tradeAction) {
        if (tradeAction.TargetLeader == player.Leader) { // Should be reference equality, this is intentional.
            player.AddOutstandingTrade(tradeAction);
            return;
        }
        opponentHandler.ProcessOpponentOnlyTradeAction(tradeAction);
    }

    public string DEBUG_GetLeaderInfoString(string leaderName) {
        Leader leader = opponentHandler.opponents[leaderName].Leader;
        string leaderDebugInfo = "Leader Name: " + leader.Name;
        leaderDebugInfo += "\nMoney Stockpile: " + leader.AffluenceStockpile + ", Money Yield: " + leader.AffluenceYield;
        leaderDebugInfo += "\nPolitics Stockpile: " + leader.PoliticsStockpile + ", Politics Yield: " + leader.PoliticsYield;
        leaderDebugInfo += "\nIntellect Stockpile: " + leader.IntelligenceStockpile + ", Intellect Yield: " + leader.IntelligenceYield;
        leaderDebugInfo += "\nControlled Planets: ";
        foreach (Planet controlledPlanet in leader.GetControlledPlanets()) {
            leaderDebugInfo += controlledPlanet.Name + " (Influence: " + leader.GetPlanetInfluence(controlledPlanet.Name).InfluenceValue + "), ";
        }
        return leaderDebugInfo;
    }

    public Leader GetOpponentLeader(string leaderName) { 
        return opponentHandler.opponents[leaderName].Leader;    
    }

    public Leader GetPlayerLeader() {
        return player.Leader;
    }

    public int GetPlayerActionsLeft() {
        return player.PlayerTurnActionsLeft;
    }
}
