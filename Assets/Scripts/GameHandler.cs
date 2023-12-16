using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
    NotificationHandler notificationHandler;
    public GameHistory gameHistory;

    public GameHandler(int leadersPerPlanet) {
        notificationHandler = new();
        opponentHandler = new();
        player = new();
        BuildLeadersFromGameData();
        planetHandler = new();
        BuildPlanetsFromGameData();
        BuildConnections(leadersPerPlanet);
        gameHistory = new();
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
            LeaderVisibility playerToOpponentVisibility = new(player.Leader, opponentLeader);
            player.Leader.AddNewLeaderVisiblity(playerToOpponentVisibility);
            LeaderVisibility opponentToPlayerVisibility = new(opponentLeader, player.Leader);
            opponentLeader.AddNewLeaderVisiblity(opponentToPlayerVisibility);
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
            opponentLeader.AddNewRelationship(relationship);
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
    public bool ProcessPlayerInitiatedTrade(TradeAction tradeAction) {
        // TODO: Orion, here's how to build the TradeAction passed to the method in this code:
        // - don't worry about turnOccurred constructor param; just pass 0
        // - you'll need player leader and target leader references for next params
        // - pass the offer/request values in the order that they appear for the constructor
        bool output = opponentHandler.ProcessPlayerInitiatedTradeAction(tradeAction);
        player.Leader.IncurTradeCosts();
        player.ProcessOutgoingTradeOutcome(tradeAction);
        player.PlayerTurnActionsLeft--;
        return output;
    }

    public void IntitiatePlayerTrade() {
        player.PlayerTurnActionsLeft--;
        player.Leader.IncurTradeCosts();
    }

    public void ProcessPlayerEspionage(string targetLeaderName, CurrencyType targetCurrency) {
        player.PlayerTurnActionsLeft--;
        player.Leader.IncurEspionageCosts();
        player.Leader.UpdateLeaderPreferenceVisibility(targetLeaderName, targetCurrency, false);
    }

    public void ProcessPlayerDiplomacy(string targetPlanetName, CurrencyType currencyToIncrease, CurrencyType currencyToDecrease) {
        player.PlayerTurnActionsLeft--;
        player.Leader.IncurDiplomacyCosts();
        Planet targetPlanet = planetHandler.planets[targetPlanetName];
        DiplomacyAction diplomacy = new(0, player.Leader, targetPlanet, currencyToIncrease, currencyToDecrease);
        
        targetPlanet.HandleDiplomacyAction(diplomacy);
    }

    public bool CheckPlayerCanAffordTrade() {
        return player.Leader.CanAffordTrade();
    }

    public bool CheckPlayerCanAffordEspionage() {
        return player.Leader.CanAffordEspionage();
    }

    public bool CheckPlayerCanAffordDiplomacy() {
        return player.Leader.CanAffordDiplomacy();
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
        notificationHandler.ClearNotifications();
        player.ClearOutstandingTrades();
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
                        if (GameHistory.LogIncludesDiplomacyActions) gameHistory.LogGameEvent(new(diplomacy.ToString()));
                        break;
                    case EspionageAction espionage:
                        // HandleEspionageAction(espionage);
                        if (GameHistory.LogIncludesDiplomacyActions) gameHistory.LogGameEvent(new(espionage.ToString()));
                        break;
                    case TradeAction trade:
                        HandleTradeAction(trade);
                        if (GameHistory.LogIncludesDiplomacyActions) gameHistory.LogGameEvent(new(trade.ToString()));
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
                Leader previousLeader;
                if (gainedPlanets.Key == player.Leader.Name) {
                    previousLeader = player.Leader.GainPlanetControl(gainedPlanet);
                    foreach (Opponent opponent in opponentHandler.opponents.Values) {
                        if (opponent.Leader.Name == previousLeader.Name) continue;
                        if (!opponent.Leader.HasEventDialogueResponse(opponent.Leader, player.Leader, previousLeader, DialogueContextType.StolenPlanet)) continue;
                        string reaction = opponent.Leader.GetEventDialogueResponse(opponent.Leader, player.Leader, previousLeader, DialogueContextType.StolenPlanet).Dialogue;
                        notificationHandler.AddNotification(new(opponent.Leader, NotificationType.Message, 0, reaction));
                    }
                } else {
                    previousLeader = opponentHandler.opponents[gainedPlanets.Key].Leader.GainPlanetControl(gainedPlanet);
                }
                if (GameHistory.LogIncludesElectionOutcomes) {
                    List<(Leader, float)> influenceRatios = GetPlanetInfluenceRatios(gainedPlanet.Name); // TODO: This can be simplified.
                    string electedLeaderPercent = influenceRatios[0].Item2.ToString("P2", new NumberFormatInfo { PercentPositivePattern = 1, PercentNegativePattern = 1 });
                    gameHistory.LogGameEvent(new($"{gainedPlanets.Key} was elected ruler of {gainedPlanet.Name} with {electedLeaderPercent} of the vote, ousting {previousLeader.Name} in the process!"));
                }
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
                // Debug.Log(opponent.Leader.Name + " WAS ELIMINATED!");
                planetHandler.HandleLeaderElimination(opponent.Leader.Name);
                player.HandleLeaderElimination(opponent.Leader.Name);
                opponentHandler.HandleLeaderElimination(opponent.Leader.Name);
                eliminatedLeaderNames.Add(opponent.Leader.Name);
            }
        }
        // Additional loop necessary to handle concurrent modification.
        foreach (string eliminatedLeaderName in eliminatedLeaderNames) {
            opponentHandler.EliminateOpponent(eliminatedLeaderName);
            gameHistory.LogGameEvent(new($"{eliminatedLeaderName} was eliminated from the game!"));
        }
        if (player.Leader.PlanetControlCount == 0) {
            return true;
        }
        return false;
    }

    private void HandleDiplomacyAction(DiplomacyAction diplomacyAction) {
        diplomacyAction.originLeader.IncurDiplomacyCosts();
        Planet targetPlanet = planetHandler.planets[diplomacyAction.TargetPlanet.Name];
        targetPlanet.HandleDiplomacyAction(diplomacyAction);
    }
    // TODO: Implement for AI
    /*private void HandleEspionageAction(EspionageAction espionageAction) {
        throw new NotImplementedException("Espionage action is not implemented to be handled yet.");
    }*/

    private void HandleTradeAction(TradeAction tradeAction) {
        tradeAction.originLeader.IncurTradeCosts();
        if (tradeAction.TargetLeader == player.Leader) { // Should be reference equality, this is intentional.
            notificationHandler.AddNotification(new(tradeAction.originLeader, NotificationType.TradeOffer, 0, 
                tradeAction.originLeader.GetEventDialogueResponse(tradeAction.originLeader, tradeAction.TargetLeader, tradeAction.originLeader, DialogueContextType.SendTrade).Dialogue));
            player.AddOutstandingTrade(tradeAction);
            return;
        }
        opponentHandler.ProcessOpponentOnlyTradeAction(tradeAction);
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

    public IEnumerable<GameEvent> GetEventHistory() { 
        return gameHistory.GetEventHistory();
    }

    public List<Notification> GetNotifications()
    {
        return notificationHandler.GetCurrentNotifications();
    }

    public void CheckLeaderWon() {
        if (GetPlayerPlanetsControlled() >= 7) {
            gameHistory.LogGameEvent(new("You won with " + GetPlayerPlanetsControlled() + " planets!"));
            return;
        }
        foreach (Opponent opponent in opponentHandler.opponents.Values) {
            if (opponent.Leader.PlanetControlCount >= 7) {
                gameHistory.LogGameEvent(new(opponent.Leader.Name + " won with " + opponent.Leader.PlanetControlCount + " planets!"));
                return;
            }
        }
        gameHistory.LogGameEvent(new("You ended the game with " + GetPlayerPlanetsControlled() + " planets!"));
    }
}
