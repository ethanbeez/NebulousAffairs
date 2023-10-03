using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using UnityEngine;

public class GameHandler {
    LeaderHandler leaderHandler;
    PlanetHandler planetHandler;

    public GameHandler(int leadersPerPlanet) {
        planetHandler = new();
        leaderHandler = new();

        planetHandler.BuildRandomPlanets(12);
        leaderHandler.BuildRandomLeaders(6);
        BuildConnections(leadersPerPlanet);
    }

    public void BuildConnections(int leadersPerPlanet) {
        BuildInfluenceProfiles();
        BuildPlanetLeaders(leadersPerPlanet);
        BuildInitialPriorities();
    }

    public void BuildInfluenceProfiles() {
        foreach (Leader leader in leaderHandler.leaders.Values) {
            foreach (Planet planet in planetHandler.planets.Values) {
                Influence influence = new(leader, planet);
                leader.AddNewInfluence(influence);
                planet.AddNewInfluence(influence);
            }
        }
    }

    public void BuildPlanetLeaders(int leadersPerPlanet) {
        List<Planet> planetList = planetHandler.planets.Values.ToList();
        int planetIndex = 0;
        foreach (Leader leader in leaderHandler.leaders.Values) {
            while (planetIndex < planetList.Count && leader.PlanetControlCount < 2) {
                leader.SetPlanetInfluence(planetList[planetIndex].Name, 0.6f);
                planetList[planetIndex].SetCurrentLeader(leader);
                leader.GainPlanetControl(planetList[planetIndex++]);
            }
        }
    }

    public void BuildInitialPriorities() {
        foreach (Leader leader in leaderHandler.leaders.Values) {
            leader.UpdatePlanetPriorities();
        }
    }

    public void UpdatePlanetLeader(Planet planet, Leader leader) {
        leader.GainPlanetControl(planet);
    }

    public List<Planet> GetPlanets() {
        return planetHandler.planets.Values.ToList();
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

    public void ExecuteBotTurns(TurnHandler.GameTurns gameTurns) {
        foreach (Leader leader in leaderHandler.leaders.Values) {
            leader.UpdateYieldPriorities();
            if (gameTurns.ElectionTurn) leader.UpdatePlanetPriorities();
            for (int i = 0; i < 3; i++) {
                GameAction leaderAction = leader.MakeDecision();
                switch (leaderAction) {
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

    public void ProcessElection(Election electionData) { 
        foreach (Planet planet in planetHandler.planets.Values) {
            electionData.DeterminePlanetElectionOutcome(planet);
        }
        foreach (KeyValuePair<string, HashSet<Planet>> gainedPlanets in electionData.GainedPlanetLeaders) {
            foreach (Planet gainedPlanet in gainedPlanets.Value) {
                leaderHandler.leaders[gainedPlanets.Key].GainPlanetControl(gainedPlanet);
            }
        }
        foreach (KeyValuePair<string, HashSet<Planet>> lostPlanets in electionData.LostPlanetLeaders) {
            foreach (Planet lostPlanet in lostPlanets.Value) {
                leaderHandler.leaders[lostPlanets.Key].LosePlanetControl(lostPlanet);
            }
        }
        
        Dictionary<string, List<(Leader, float)>> ratios = Election.GetPlanetsInfluenceRatios(planetHandler.planets.Values.ToList());
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
        Debug.Log(sb.ToString());
    }

    private void HandleDiplomacyAction(DiplomacyAction diplomacyAction) {
        Planet targetPlanet = planetHandler.planets[diplomacyAction.TargetPlanet.Name];
        targetPlanet.HandleDiplomacyAction(diplomacyAction);
    }

    private void HandleEspionageAction(EspionageAction espionageAction) {
        throw new NotImplementedException("Espionage action is not implemented to be handled yet.");
    }

    private void HandleTradeAction(TradeAction tradeAction) {
        throw new NotImplementedException("Trade action is not implemented to be handled yet.");
    }

    public string DEBUG_GetLeaderInfoString(string leaderName) {
        Leader leader = leaderHandler.leaders[leaderName];
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
}
