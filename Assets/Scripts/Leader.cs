#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using static Influence;
using static TurnHandler;

public class Leader {
    #region Game Constants
    #endregion Game Constants
    #region Fields
    // Relationships and influence.
    private Dictionary<string, Relationship> relationships; // (Leader Name) -> (Leader Relationship)
    private Dictionary<string, Influence> influences; // (Planet Name) -> (Planet Relationship)
    private Dictionary<string, Planet> controlledPlanets;
    private int planetControlCount;

    #endregion

    #region Properties
    public string Name { get; }
    // Currency stockpiles, representing the current unspent currency value.
    public int AffluenceStockpile { get; set; }
    public int PoliticsStockpile { get; set; }
    public int IntelligenceStockpile { get; set; }
    // Currency yields, represented as a per-turn integer.
    public int AffluenceYield { get; set; }
    public int PoliticsYield { get; set; }
    public int IntelligenceYield { get; set; }

    public int AffluencePreference { get; }
    public int PoliticsPreference { get; }
    public int IntellectPreference { get; }
    public int PlanetControlCount => planetControlCount;
    #endregion

    #region Events
    public delegate void TurnChangeHandler(GameTurns gameTurns);
    public event TurnChangeHandler? TurnChanged;
    #endregion

    #region Constructors & Builders
    public Leader(string name, int startingAffluence = 0, int startingPolitics = 0, int startingIntelligence = 0,
        int affluenceYield = 1, int politicsYield = 1, int intelligenceYield = 1) {
        AffluenceYield = affluenceYield;
        PoliticsYield = politicsYield;
        IntelligenceYield = intelligenceYield;

        planetControlCount = 0; // To be 'corrected' to 2 when building connections in GameHandler
        Name = name;
        AffluenceStockpile = startingAffluence;
        PoliticsStockpile = startingPolitics;
        IntelligenceStockpile = startingIntelligence;

        influences = new();
        controlledPlanets = new();
        relationships = new();
    }

    public void AddNewInfluence(Influence influence) {
        influences.Add(influence.PlanetName, influence);
    }

    public void AddNewRelationship(Relationship relationship) {
        // Gets the name of the other Leader (i.e., the leader that is not 'this' Leader).
        string otherLeaderName = (relationship.Leader1 == this) ? relationship.Leader2.Name : relationship.Leader1.Name;
        relationships.Add(otherLeaderName, relationship);
    }
    #endregion

    #region Getters/Setters
    public float GetRelationshipValue(string leaderName) {
        return relationships[leaderName].RelationshipValue;
    }

    public List<Relationship> GetAscendingSortedLeaderRelationships() {
        List<Relationship> sortedLeaderRelationships = new();
        foreach (Relationship relationship in relationships.Values) {
            sortedLeaderRelationships.Add(relationship);
        }
        sortedLeaderRelationships.Sort((relationship1, relationship2) => relationship1.RelationshipValue.CompareTo(relationship2.RelationshipValue));
        return sortedLeaderRelationships;
    }

    public List<Influence> GetAscendingSortedPlanetInfluences() {
        List<Influence> sortedPlanetInfluences = new();
        foreach (Influence influence in influences.Values) {
            sortedPlanetInfluences.Add(influence);
        }
        sortedPlanetInfluences.Sort((influence1, influence2) => influence1.InfluenceValue.CompareTo(influence2.InfluenceValue));
        return sortedPlanetInfluences;
    }

    public List<Planet> GetControlledPlanets() {
        List<Planet> controlledPlanetsList = new();
        foreach (Planet controlledPlanet in controlledPlanets.Values) {
            controlledPlanetsList.Add(controlledPlanet);
        }
        return controlledPlanetsList;
    }

    public void SetPlanetInfluence(string planetName, float leaderInfluence) {
        if (!influences.ContainsKey(planetName)) {
            Debug.LogError("Leader.SetPlanetInfluence tried to access a planet by name that did not exist in the influences Dictionary.");
        }
        influences[planetName].SetInfluence(leaderInfluence);
    }

    public Influence GetPlanetInfluence(string planetName) {
        return influences[planetName];
    }

    public List<Influence> GetAllInfluences() {
        return influences.Values.ToList();
    }
    #endregion

    #region Game Actions
    public void ProcessIncomingTradeAction(TradeAction tradeAction) {
        AffluenceStockpile += tradeAction.OfferedAffluence;
        IntelligenceStockpile += tradeAction.OfferedIntellect;
        PoliticsStockpile += tradeAction.OfferedPolitics;


        AffluenceStockpile -= tradeAction.RequestedAffluence;
        IntelligenceStockpile -= tradeAction.RequestedIntellect;
        PoliticsStockpile -= tradeAction.RequestedPolitics;
    }

    public void ProcessOutgoingTradeOutcome(TradeAction tradeAction) {
        AffluenceStockpile -= tradeAction.OfferedAffluence;
        IntelligenceStockpile -= tradeAction.OfferedIntellect;
        PoliticsStockpile -= tradeAction.OfferedPolitics;

        AffluenceStockpile += tradeAction.RequestedAffluence;
        IntelligenceStockpile += tradeAction.RequestedIntellect;
        PoliticsStockpile += tradeAction.RequestedPolitics;
    }

    public void AcceptIncomingTrade(TradeAction tradeAction) {
        AffluenceStockpile += tradeAction.OfferedAffluence;
        IntelligenceStockpile += tradeAction.OfferedIntellect;
        PoliticsStockpile += tradeAction.OfferedPolitics;
        relationships[tradeAction.OriginLeader.Name].ProcessTradeOutcome(tradeAction.TradeWeight);
    }

    public void RefuseIncomingTrade(TradeAction tradeAction) {
        relationships[tradeAction.OriginLeader.Name].ProcessTradeOutcome(tradeAction.TradeWeight);
    }

    /// <summary>
    /// This method does nothing if this leader was already the leader of the planet.
    /// </summary>
    /// <param name="planet"></param>
    public void GainPlanetControl(Planet planet) {
        bool wasLeader = influences[planet.Name].SetIsLeader(true);
        influences[planet.Name].Planet.SetCurrentLeader(this);
        if (!wasLeader) {
            controlledPlanets.Add(planet.Name, planet);
            planetControlCount++;
        }
    }

    public void LosePlanetControl(Planet planet) {
        bool wasLeader = influences[planet.Name].SetIsLeader(false);
        if (wasLeader) {
            controlledPlanets.Remove(planet.Name, out _);
            planetControlCount--;
        }
    }
    #endregion
}

public class Influence {
    #region Fields
    private Leader leader;
    private Planet planet;
    private float influenceValue;
    private bool isLeader;
    #endregion

    #region Properties
    public string LeaderName => leader.Name;
    public string PlanetName => planet.Name;
    public Planet Planet => planet;
    public Leader Leader => leader;
    public float InfluenceValue => influenceValue;
    public bool IsLeader => isLeader;
    #endregion

    #region Constructors
    public Influence(Leader leader, Planet planet, float influenceValue = 0, bool isLeader = false) {
        this.leader = leader;
        this.planet = planet;
        this.influenceValue = influenceValue;
        this.isLeader = isLeader;
    }
    #endregion

    public void SetInfluence(float influenceValue) {
        this.influenceValue = influenceValue;
    }

    public void UpdateInfluence(float influenceModifier) {
        float newInfluenceValue = Mathf.Clamp(influenceValue + influenceModifier, 0, 1);
        influenceValue = newInfluenceValue;
    }
    /// <summary>
    /// Returns true if this leader was already the leader of the planet.
    /// </summary>
    public bool SetIsLeader(bool isLeader) {
        bool previousIsLeader = this.isLeader;
        this.isLeader = isLeader;
        return previousIsLeader;
    }
}

public class Relationship {
    #region Game Constants
    private const float StartingRelationshipValue = 0.5f;
    private const float IncomingTradeWeightModifier = 0.05f;
    #endregion

    #region Properties
    // public string TargetLeaderName => targetLeader.Name;
    public Leader Leader1 { get; }
    public Leader Leader2 { get; }
    public float RelationshipValue { get; set; }
    #endregion

    #region Constructors
    public Relationship(Leader leader1, Leader leader2) {
        Leader1 = leader1;
        Leader2 = leader2;
        RelationshipValue = StartingRelationshipValue;
    }
    #endregion

    public void ProcessTradeOutcome(int incomingTradeWeight) {
        RelationshipValue += IncomingTradeWeightModifier * incomingTradeWeight;
    }

    public Leader GetOtherLeader(string leaderName) {
        return (leaderName == Leader1.Name) ? Leader2 : Leader1;
    }
}

public class LeaderVisibility {
    #region Properties
    public Leader OriginLeader { get; }
    public Leader TargetLeader { get; }

    public bool TargetLeaderPoliticsVisible { get; }
    public bool TargetLeaderAffluenceVisible { get; }
    public bool TargetLeaderIntellectVisible { get; }
    #endregion

    #region Constructors
    public LeaderVisibility(Leader originLeader, Leader targetLeader) { 
        OriginLeader = originLeader;
        TargetLeader = targetLeader;
    }
    #endregion
}