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
    private Dictionary<string, LeaderVisibility> visibilities; // (Target Leader Name) -> (Leader Visibility)
    private Dictionary<string, Planet> controlledPlanets;
    private int planetControlCount;

    private LeaderResources leaderResources;
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
    public Leader(string name, int affluencePreference, int politicsPreference, int intellectPreference,
        int startingAffluence = 10, int startingPolitics = 10, int startingIntelligence = 10,
        int affluenceYield = 0, int politicsYield = 0, int intelligenceYield = 0) {
        AffluenceYield = affluenceYield;
        PoliticsYield = politicsYield;
        IntelligenceYield = intelligenceYield;

        planetControlCount = 0; // To be 'corrected' to 2 when building connections in GameHandler
        Name = name;
        AffluenceStockpile = startingAffluence;
        PoliticsStockpile = startingPolitics;
        IntelligenceStockpile = startingIntelligence;

        AffluencePreference = affluencePreference;
        PoliticsPreference = politicsPreference;
        IntellectPreference = intellectPreference;

        influences = new();
        controlledPlanets = new();
        relationships = new();
        visibilities = new();
        leaderResources = new(this);
    }

    public string GetLeaderImagePath(LeaderResources.Perspectives perspective, LeaderResources.Expressions expression) {
        return leaderResources.GetLeaderImagePath(perspective, expression);
    }

    public void AddNewLeaderVisiblity(LeaderVisibility visibility) {
        visibilities.Add(visibility.TargetLeader.Name, visibility);
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
    public bool GetLeaderPreferenceVisibility(Leader targetLeader, CurrencyType currencyType) {
        return visibilities[targetLeader.Name].GetPreferenceVisibilityFromType(currencyType);
    }

    public int GetCurrencyPreferenceFromType(CurrencyType currencyType) {
        switch (currencyType) {
            case CurrencyType.Affluence:
                return AffluencePreference;
            case CurrencyType.Politics:
                return PoliticsPreference;
            case CurrencyType.Intellect:
                return IntellectPreference;
        }
        Debug.LogError("Leader.GetCurrencyPreferenceFromType: An invalid CurrencyType enum value was provided! Default returning 0.");
        return 0;
    }

    public int GetStockpileFromCurrencyType(CurrencyType currencyType) {
        switch (currencyType) {
            case CurrencyType.Affluence:
                return AffluenceStockpile;
            case CurrencyType.Politics:
                return PoliticsStockpile;
            case CurrencyType.Intellect:
                return IntelligenceStockpile;
        }
        Debug.LogError("Leader.GetStockpileFromCurrency: An invalid CurrencyType enum value was provided! Default returning 0.");
        return 0;
    }

    public int GetYieldFromCurrencyType(CurrencyType currencyType) {
        switch (currencyType) {
            case CurrencyType.Affluence:
                return AffluenceYield;
            case CurrencyType.Politics:
                return PoliticsYield;
            case CurrencyType.Intellect:
                return IntelligenceYield;
        }
        Debug.LogError("Leader.GetYieldFromCurrencyType: An invalid CurrencyType enum value was provided! Default returning 0.");
        return 0;
    }

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

    public void ProcessLeaderElimination(string leaderName) {
        relationships.Remove(leaderName);
        visibilities.Remove(leaderName);
    }
    #endregion

    #region Game Actions
    public void AccrueYields() {
        AffluenceStockpile += AffluenceYield;
        IntelligenceStockpile += IntelligenceYield;
        PoliticsStockpile += PoliticsYield;
    }

    public void IncurElectionTurnCosts() {
        foreach (string planetName in controlledPlanets.Keys) {
            TryIncurPlanetUpkeep(CurrencyType.Affluence, planetControlCount, planetName);
            TryIncurPlanetUpkeep(CurrencyType.Politics, planetControlCount, planetName);
            TryIncurPlanetUpkeep(CurrencyType.Intellect, planetControlCount, planetName);
        }
    }

    public void IncurGameActionCosts(GameAction action) {
        switch (action) {
            case DiplomacyAction diplomacy:
                IncurTradeCosts();
                break;
            case EspionageAction espionage:
                IncurDiplomacyCosts();
                break;
            case TradeAction trade:
                IncurEspionageCosts();
                break;
        }
    }

    public void IncurTradeCosts() {
        AffluenceStockpile -= TradeAction.AffluenceCost;
        IntelligenceStockpile -= TradeAction.IntellectCost;
        PoliticsStockpile -= TradeAction.PoliticsCost;
    }

    public void IncurDiplomacyCosts() {
        AffluenceStockpile -= DiplomacyAction.AffluenceCost;
        IntelligenceStockpile -= DiplomacyAction.IntellectCost;
        PoliticsStockpile -= DiplomacyAction.PoliticsCost;
    }
    public void IncurEspionageCosts() {
        AffluenceStockpile -= EspionageAction.AffluenceCost;
        IntelligenceStockpile -= EspionageAction.IntellectCost;
        PoliticsStockpile -= EspionageAction.PoliticsCost;
    }

    public bool CanAffordTrade() { 
        return AffluenceStockpile >= TradeAction.AffluenceCost && IntelligenceStockpile >= TradeAction.IntellectCost && PoliticsStockpile >= TradeAction.PoliticsCost;
    }

    public bool CanAffordEspionage() {
        return AffluenceStockpile >= EspionageAction.AffluenceCost && IntelligenceStockpile >= EspionageAction.IntellectCost && PoliticsStockpile >= EspionageAction.PoliticsCost;
    }

    public bool CanAffordDiplomacy() {
        return AffluenceStockpile >= DiplomacyAction.AffluenceCost && IntelligenceStockpile >= DiplomacyAction.IntellectCost && PoliticsStockpile >= DiplomacyAction.PoliticsCost;
    }

    private void TryIncurPlanetUpkeep(CurrencyType currencyType, int costPerCurrency, string planetName) {
        switch (currencyType) {
            case CurrencyType.Affluence:
                if (AffluenceStockpile - costPerCurrency < 0) {
                    influences[planetName].SetInfluence(influences[planetName].InfluenceValue / 2);
                } else {
                    AffluenceStockpile -= costPerCurrency;
                }
                break;
            case CurrencyType.Politics:
                if (PoliticsStockpile - costPerCurrency < 0) {
                    influences[planetName].SetInfluence(influences[planetName].InfluenceValue / 2);
                } else {
                    PoliticsStockpile -= costPerCurrency;
                }
                break;
            case CurrencyType.Intellect:
                if (IntelligenceStockpile - costPerCurrency < 0) {
                    influences[planetName].SetInfluence(influences[planetName].InfluenceValue / 2);
                } else {
                    IntelligenceStockpile -= costPerCurrency;
                }
                break;
        }
    }

    /*public void ProcessIncomingTradeAction(TradeAction tradeAction) {
        AffluenceStockpile += tradeAction.OfferedAffluence;
        IntelligenceStockpile += tradeAction.OfferedIntellect;
        PoliticsStockpile += tradeAction.OfferedPolitics;


        AffluenceStockpile -= tradeAction.RequestedAffluence;
        IntelligenceStockpile -= tradeAction.RequestedIntellect;
        PoliticsStockpile -= tradeAction.RequestedPolitics;
    }*/

    public void ProcessOutgoingTradeOutcome(TradeAction tradeAction) {
        AffluenceStockpile -= tradeAction.OfferedAffluence;
        IntelligenceStockpile -= tradeAction.OfferedIntellect;
        PoliticsStockpile -= tradeAction.OfferedPolitics;

        AffluenceStockpile += tradeAction.RequestedAffluence;
        IntelligenceStockpile += tradeAction.RequestedIntellect;
        PoliticsStockpile += tradeAction.RequestedPolitics;
    }

    public void AcceptIncomingTrade(TradeAction tradeAction) {
        AffluenceStockpile += tradeAction.OfferedAffluence - tradeAction.RequestedAffluence;
        IntelligenceStockpile += tradeAction.OfferedIntellect - tradeAction.RequestedIntellect;
        PoliticsStockpile += tradeAction.OfferedPolitics - tradeAction.RequestedPolitics;
        relationships[tradeAction.OriginLeader.Name].ProcessTradeOutcome(tradeAction.TradeWeight);
    }

    public void RefuseIncomingTrade(TradeAction tradeAction) {
        relationships[tradeAction.OriginLeader.Name].ProcessTradeOutcome(tradeAction.TradeWeight);
    }

    public void UpdateLeaderPreferenceVisibility(string targetLeaderName, CurrencyType targetPreferenceType, bool visible) {
        visibilities[targetLeaderName].SetPreferenceVisibilityFromType(targetPreferenceType, visible);
    }

    /// <summary>
    /// This method does nothing if this leader was already the leader of the planet.
    /// </summary>
    /// <param name="planet"></param>
    public Leader GainPlanetControl(Planet planet) {
        bool wasLeader = influences[planet.Name].SetIsLeader(true);
        Leader previousLeader = influences[planet.Name].Planet.CurrentLeader!;
        influences[planet.Name].Planet.SetCurrentLeader(this);
        if (!wasLeader) {
            controlledPlanets.Add(planet.Name, planet);
            planetControlCount++;
            AffluenceYield += planet.AffluenceYield;
            PoliticsYield += planet.PoliticsYield;
            IntelligenceYield += planet.IntellectYield;
        }
        return previousLeader;
    }

    public void LosePlanetControl(Planet planet) {
        bool wasLeader = influences[planet.Name].SetIsLeader(false);
        if (wasLeader) {
            controlledPlanets.Remove(planet.Name, out _);
            planetControlCount--;
            AffluenceYield -= planet.AffluenceYield;
            PoliticsYield -= planet.PoliticsYield;
            IntelligenceYield -= planet.IntellectYield;
        }
    }
    #endregion

    public LeaderDialogue.DialogueNode GetEventDialogueResponse(Leader originLeader, Leader targetLeader, Leader interactionLeader, DialogueContextType dialogueContext) {
        return leaderResources.GetEventDialogueResponse(originLeader, targetLeader, interactionLeader, dialogueContext);
    }
}
public enum DialogueContextType {
    Default = 0,
    SendTrade = 1,
    SentTradeAccepted = 2,
    SendTradeRejected = 3,
    ReceiveTrade = 4,
    AcceptReceivedTrade = 5,
    RefuseReceivedTrade = 6,
    StolenPlanet = 7,
    LeaderEliminated = 8,
    PersonalQuestion = 9
}

public class LeaderResources {
    public enum Expressions { 
        Default = 0,
        Neutral = 1,
        Happy = 2,
        Sad = 3,
        Angry = 4,
        Curious = 5
    }

    public enum Perspectives { 
        Default = 0,
        Portrait = 1,
        Full = 2
    }

    private LeaderDialogue dialogue;

    private Dictionary<Perspectives, Dictionary<Expressions, string>> imagePaths;

    public LeaderResources(Leader leader) {
        dialogue = FileManager.GetLeaderDialogue(leader);
        imagePaths = FileManager.GetLeaderImagePaths(leader.Name);
        Debug.Log("hlelo");
    }

    public List<string> GetValidDialogueQuestions(Leader originLeader) {
        return dialogue.GetValidQuestions(originLeader);
    }

    public LeaderDialogue.DialogueNode GetEventDialogueResponse(Leader originLeader, Leader targetLeader, Leader interactionLeader, DialogueContextType dialogueContext) {
        return dialogue.GetEventDialogueResponse(originLeader, targetLeader, interactionLeader, dialogueContext);
    }

    public string GetLeaderImagePath(Perspectives perspective, Expressions expression) {
        return imagePaths[perspective][expression];
    }
}

public class LeaderDialogue {

    private Leader leader;
    private Dictionary<string, DialogueNode> nodes;
    private Dictionary<DialogueContextType, ContextNode> contextNodes;
    public LeaderDialogue(Leader leader) {
        contextNodes = new();
        nodes = new();
        BuildContextNodes();
        this.leader = leader;
    }

    public DialogueNode GetEventDialogueResponse(Leader originLeader, Leader targetLeader, Leader interactionLeader, DialogueContextType dialogueContext) {
        ContextNode contextNode = contextNodes[dialogueContext];
        return contextNode.GetValidDialogueNode(originLeader, targetLeader, interactionLeader);
    }

    private void BuildContextNodes() {
        foreach (DialogueContextType contextType in (DialogueContextType[]) Enum.GetValues(typeof(DialogueContextType))) {
            contextNodes.Add(contextType, new(contextType));
        }
    }

    public List<string> GetValidQuestions(Leader originLeader) {
        List<string> validQuestions = new();
        ContextNode personalQuestionsContext = contextNodes[DialogueContextType.PersonalQuestion];
        foreach (DialogueEdge edge in personalQuestionsContext.adjacencyList) {
            if (edge.CheckConditions(originLeader, leader, leader)) {
                validQuestions.Add(edge.Destination.Dialogue);
            }
        }
        return validQuestions;
    }

    public void AddDialogueEdge(string sourceDialogue, string destinationDialogue, DialogueCondition condition) {
        DialogueNode sourceNode;
        if (nodes.ContainsKey(sourceDialogue)) {
            sourceNode = nodes[sourceDialogue];
        } else { 
            sourceNode = new DialogueNode(sourceDialogue);
            nodes[sourceDialogue] = sourceNode;
        }
        DialogueNode destinationNode;
        if (nodes.ContainsKey(destinationDialogue)) {
            destinationNode = nodes[destinationDialogue];
        } else {
            destinationNode = new DialogueNode(destinationDialogue);
            nodes[destinationDialogue] = destinationNode;
        }
        sourceNode.AddEdge(new(destinationNode, condition));
    }

    public void AddContextEdge(DialogueContextType contextType, string dialogue, DialogueCondition condition) {
        ContextNode contextNode = contextNodes[contextType];
        DialogueNode dialogueNode = new(dialogue);
        contextNode.AddEdge(new(dialogueNode, condition));
        nodes.Add(dialogue, dialogueNode);
    }

    public class ContextNode {
        public List<DialogueEdge> adjacencyList;
        public DialogueContextType ContextType;
        public ContextNode(DialogueContextType contextType) {
            ContextType = contextType;
            adjacencyList = new();
        }

        public void AddEdge(DialogueEdge dialogueEdge) {
            adjacencyList.Add(dialogueEdge);
        }

        public DialogueNode GetValidDialogueNode(Leader originLeader, Leader targetLeader, Leader interactionLeader) {
            foreach (DialogueEdge edge in adjacencyList) {
                if (edge.CheckConditions(originLeader, targetLeader, interactionLeader)) {
                    return edge.Destination;
                }
            }
            return null;
        }
    }

    public class DialogueNode {
        public string Dialogue;
        // public bool OriginLeaderInitiated;
        public List<DialogueEdge> adjacencyList;

        public DialogueNode(string dialogue) {
            Dialogue = dialogue;
            // OriginLeaderInitiated = originLeaderInitiated;
            adjacencyList = new();
        }

        public void AddEdge(DialogueEdge dialogueEdge) {
            adjacencyList.Add(dialogueEdge);
        }
    }

    public enum ConditionType { 
        Default = 0,
        RelationshipLevel = 1,
        TargetLeader = 2
    }

    public class DialogueEdge {
        public DialogueCondition Condition;
        public DialogueNode Destination;

        public DialogueEdge(DialogueNode destination, DialogueCondition condition) {
            Destination = destination;
            Condition = condition;
        }

        public bool CheckConditions(Leader originLeader, Leader targetLeader, Leader interactionLeader) {
            return Condition.CheckCondition(originLeader, targetLeader, interactionLeader);
        }
    }

    public class DialogueCondition {
        private float MinimumRelationshipRequired;
        private float MaximumRelationshipRequired;
        private string InteractionLeaderName;

        public DialogueCondition(float minimumRelationshipRequired, float maximumRelationshipRequired, string interactionLeaderName) { 
            MinimumRelationshipRequired = minimumRelationshipRequired;
            MaximumRelationshipRequired = maximumRelationshipRequired;
            InteractionLeaderName = interactionLeaderName;
        }

        public bool CheckCondition(Leader originLeader, Leader targetLeader, Leader interactionLeader) {
            if (interactionLeader.Name != InteractionLeaderName) return false;
            float relationshipValue = originLeader.GetRelationshipValue(targetLeader.Name);
            if (relationshipValue < MinimumRelationshipRequired || relationshipValue > MaximumRelationshipRequired) return false;
            return true;
        }
    }

    /*public class RelationshipCondition : Condition {
        private float MinimumRelationshipRequired;
        private float MaximumRelationshipRequired;

        public RelationshipCondition(float minimumRelationshipRequired, float maximumRelationshipRequired) {
            MinimumRelationshipRequired = minimumRelationshipRequired;
            MaximumRelationshipRequired = maximumRelationshipRequired;
        }

        public override bool CheckCondition(Leader originLeader, Leader targetLeader) {
            float relationshipValue = originLeader.GetRelationshipValue(targetLeader.Name);
            return relationshipValue >= MinimumRelationshipRequired && relationshipValue <= MaximumRelationshipRequired;
        }
    }

    public class TargetLeaderCondition : Condition {
        private string TargetLeaderName;

        public TargetLeaderCondition(string targetLeaderName) {
            TargetLeaderName = targetLeaderName;
        }

        public override bool CheckCondition(Leader originLeader, Leader targetLeader) {
            return targetLeader.Name == TargetLeaderName;
        }
    }*/
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
    private const bool PoliticsPrefVisibleDefault = true;
    private const bool AffluencePrefVisibleDefault = true;
    private const bool IntellectPrefVisibleDefault = true;
    #region Properties
    public Leader OriginLeader { get; }
    public Leader TargetLeader { get; }

    public bool TargetLeaderPoliticsPrefVisible { get; set; } = PoliticsPrefVisibleDefault;
    public bool TargetLeaderAffluencePrefVisible { get; set; } = AffluencePrefVisibleDefault;
    public bool TargetLeaderIntellectPrefVisible { get; set; } = IntellectPrefVisibleDefault;
    #endregion

    #region Constructors
    public LeaderVisibility(Leader originLeader, Leader targetLeader) { 
        OriginLeader = originLeader;
        TargetLeader = targetLeader;
    }
    #endregion

    #region Getters
    public bool GetPreferenceVisibilityFromType(CurrencyType currencyType) {
        switch (currencyType) {
            case CurrencyType.Affluence:
                return TargetLeaderAffluencePrefVisible;
            case CurrencyType.Politics:
                return TargetLeaderPoliticsPrefVisible;
            case CurrencyType.Intellect:
                return TargetLeaderIntellectPrefVisible;
        }
        Debug.LogError("LeaderVisibility.GetPreferenceVisibilityFromType: An invalid CurrencyType enum value was provided! Default returning false.");
        return false;
    }

    public void SetPreferenceVisibilityFromType(CurrencyType currencyType, bool visible) {
        switch (currencyType) {
            case CurrencyType.Affluence:
                TargetLeaderAffluencePrefVisible = visible;
                break;
            case CurrencyType.Politics:
                TargetLeaderPoliticsPrefVisible = visible;
                break;
            case CurrencyType.Intellect:
                TargetLeaderIntellectPrefVisible = visible;
                break;
        }
    }
    #endregion
}