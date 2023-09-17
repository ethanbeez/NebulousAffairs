using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionTypes {
    DiplomacyAction = 0,
    EspionageAction = 1,
    TradeAction = 2
}

public enum CurrencyType { 
    Affluence = 0,
    Diplomacy = 1,
    Intellect = 2
}

public abstract class GameAction {
    private readonly int turnOccurred;
    private readonly Leader originLeader;
    public int TurnOccurred => turnOccurred;
    public Leader OriginLeader => originLeader;

    protected GameAction(int turnOccurred, Leader originLeader) { 
        this.turnOccurred = turnOccurred;
        this.originLeader = originLeader;
    }
}

public class TradeAction : GameAction {
    private const int AffluenceCost = 2;
    private const int PoliticsCost = 0;
    private const int IntellectCost = 0;
    private readonly Leader targetLeader;

    public Leader TargetLeader => targetLeader;

    public TradeAction(int turnOccurred, Leader originLeader, Leader targetLeader) : base(turnOccurred, originLeader) { 
        this.targetLeader = targetLeader;
    }

    // UNITY PLEASE update your .NET version compatability! Let me live my half-baked polymorphic dreams!! :-(
    public static float ComputeActionDecisionWeight(Leader leader, float additionalAdjustments = 0) {
        if (leader.AffluenceStockpile < AffluenceCost || leader.PoliticsStockpile < PoliticsCost || leader.IntelligenceStockpile < IntellectCost) {
            return -1;
        }

        return Mathf.Clamp(1 - leader.DecisionProfile.AffluenceBias - leader.DecisionProfile.AffluencePriority + additionalAdjustments, 0, 1);
    }

    public static float ComputeSpecificDecisionWeight(Leader originLeader, Leader targetLeader) {
        throw new System.NotImplementedException();
    }
}

public class DiplomacyAction : GameAction {
    private const int AffluenceCost = 0;
    private const int PoliticsCost = 5;
    private const int IntellectCost = 0;
    private readonly Planet targetPlanet;
    private readonly CurrencyType currencyToIncrease;
    private readonly CurrencyType currencyToDecrease;
    public CurrencyType CurrencyToIncrease => currencyToIncrease;
    public CurrencyType CurrencyToDecrease => currencyToDecrease;
    public Planet TargetPlanet => targetPlanet;
    public DiplomacyAction(int turnOccurred, Leader originLeader, Planet targetPlanet, CurrencyType currencyToIncrease, CurrencyType currencyToDecrease) 
        : base(turnOccurred, originLeader) {
        this.currencyToIncrease = currencyToIncrease;
        this.currencyToDecrease = currencyToDecrease;
        this.targetPlanet = targetPlanet;
    }

    public static float ComputeActionDecisionWeight(Leader leader, float additionalAdjustments = 0) {
        if (leader.AffluenceStockpile < AffluenceCost || leader.PoliticsStockpile < PoliticsCost || leader.IntelligenceStockpile < IntellectCost) {
            return -1;
        }

        return Mathf.Clamp(1 - leader.DecisionProfile.PoliticsBias - leader.DecisionProfile.PoliticsPriority + additionalAdjustments, 0, 1);
    }

    public static float ComputeSpecificDecisionWeight() {
        throw new System.NotImplementedException();
    }
}

public class EspionageAction : GameAction {
    private const int AffluenceCost = 0;
    private const int PoliticsCost = 0;
    private const int IntellectCost = 5;
    private readonly Leader targetLeader;
    public EspionageAction(int turnOccurred, Leader originLeader, Leader targetLeader) : base(turnOccurred, originLeader) {
        this.targetLeader = targetLeader;
    }

    public static float ComputeActionDecisionWeight(Leader leader, float additionalAdjustments = 0) {
        if (leader.AffluenceStockpile < AffluenceCost || leader.PoliticsStockpile < PoliticsCost || leader.IntelligenceStockpile < IntellectCost) {
            return -1;
        }

        return Mathf.Clamp(1 - leader.DecisionProfile.IntelligenceBias - leader.DecisionProfile.IntelligencePriority + additionalAdjustments, 0, 1);
    }

    public static float ComputeSpecificDecisionWeight() {

        throw new System.NotImplementedException();
    }
}