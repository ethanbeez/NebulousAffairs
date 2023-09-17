using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class GameHandler {
    LeaderHandler leaderHandler;
    PlanetHandler planetHandler;
    public GameHandler() {
        planetHandler = new();
        leaderHandler = new();

        planetHandler.BuildRandomPlanets(12);
        leaderHandler.BuildRandomLeaders(6);
        BuildConnections();
    }

    public void BuildConnections() {
        BuildInfluenceProfiles();
        BuildPlanetLeaders();
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

    public void BuildPlanetLeaders() {
        List<Planet> planetList = planetHandler.planets.Values.ToList();
        int planetIndex = 0;
        foreach (Leader leader in leaderHandler.leaders.Values) {
            while (leader.PlanetControlCount < 2) {
                leader.SetPlanetInfluence(planetList[planetIndex].Name, 0.6f);
                leader.GainPlanetControl(planetList[planetIndex++]);
            }
        }
    }

    public void UpdatePlanetLeader(Planet planet, Leader leader) {
        leader.GainPlanetControl(planet);
    }

    public void ExecuteBotTurns() {
        foreach (Leader leader in leaderHandler.leaders.Values) {
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

    private void HandleDiplomacyAction(DiplomacyAction diplomacyAction) {
        
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
