using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Opponent;

public enum ActionTypes {
    DiplomacyAction = 0,
    EspionageAction = 1,
    TradeAction = 2
}

public enum CurrencyType { 
    Affluence = 0,
    Politics = 1,
    Intellect = 2
}

public abstract class GameAction {
    
    private readonly int turnOccurred;
    public readonly Leader originLeader;
    public int TurnOccurred => turnOccurred;
    public Leader OriginLeader => originLeader;

    protected GameAction(int turnOccurred, Leader originLeader) { 
        this.turnOccurred = turnOccurred;
        this.originLeader = originLeader;
    }

    public abstract override string ToString();
}

public class TradeAction : GameAction {
    #region Game Constants
    public const int AffluenceCost = 2;
    public const int PoliticsCost = 0;
    public const int IntellectCost = 0;
    #endregion


    #region Properties
    public int OfferedAffluence { get; }
    public int OfferedPolitics { get; }
    public int OfferedIntellect { get; }

    public int RequestedAffluence { get; }
    public int RequestedPolitics { get; }
    public int RequestedIntellect { get; }
    public Leader TargetLeader { get; }
    public int TradeWeight { get; private set; } // By convention, a positive trade weight corresponds to a trade favoring the target leader.
    public bool Accepted { get; set; }
    #endregion



    public TradeAction(int turnOccurred, Leader originLeader, Leader targetLeader, int offeredAffluence, int offeredPolitics, int offeredIntellect,
        int requestedAffluence, int requestedPolitics, int requestedIntellect) : base(turnOccurred, originLeader) {
        OfferedAffluence = offeredAffluence;
        OfferedPolitics = offeredPolitics;
        OfferedIntellect = offeredIntellect;
        RequestedAffluence = requestedAffluence;
        RequestedPolitics = requestedPolitics;
        RequestedIntellect = requestedIntellect;
        TargetLeader = targetLeader;
        ComputeTradeWeights();
    }

    public void ComputeTradeWeights() {
        int relationshipModifier = (originLeader.GetRelationshipValue(TargetLeader.Name) >= OpponentDecisionProfile.MinimumEquivalentDealRelationshipValue) ? 0 : OpponentDecisionProfile.RelationshipTradePenalty;
        int effectiveOfferedAffluence = (OfferedAffluence > 0) ? OfferedAffluence + TargetLeader.AffluencePreference : 0;
        int effectiveOfferedPolitics = (OfferedPolitics > 0) ? OfferedPolitics + TargetLeader.PoliticsPreference : 0;
        int effectiveOfferedIntellect = (OfferedIntellect > 0) ? OfferedIntellect + TargetLeader.IntellectPreference : 0;

        int effectiveRequestedAffluence = (RequestedAffluence > 0) ? RequestedAffluence + TargetLeader.AffluencePreference : 0;
        int effectiveRequestedPolitics = (RequestedPolitics > 0) ? RequestedPolitics + TargetLeader.PoliticsPreference : 0;
        int effectiveRequestedIntellect = (RequestedIntellect > 0) ? RequestedIntellect + TargetLeader.IntellectPreference : 0;

        int effectiveOfferWeight = relationshipModifier + effectiveOfferedAffluence + effectiveOfferedPolitics + effectiveOfferedIntellect;
        int effectiveRequestWeight = effectiveRequestedAffluence + effectiveRequestedPolitics + effectiveRequestedIntellect;
        TradeWeight = effectiveOfferWeight - effectiveRequestWeight;
    }

    public override string ToString() {
        string accepted = Accepted ? "accepted" : "refused";
        return $"{originLeader.Name} sent {TargetLeader.Name} a trade offer, and {TargetLeader.Name} {accepted} the deal.";
    }
}

public class DiplomacyAction : GameAction {
    public const int AffluenceCost = 0;
    public const int PoliticsCost = 5;
    public const int IntellectCost = 0;
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

    public static float ComputeSpecificDecisionWeight() {
        throw new System.NotImplementedException();
    }

    public override string ToString() {
        return $"{originLeader.Name} campaigned on {targetPlanet.Name}, increasing their {CurrencyToIncrease} and decreasing their {CurrencyToDecrease}.";
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

    public static float ComputeSpecificDecisionWeight() {

        throw new System.NotImplementedException();
    }

    public override string ToString() {
        return $"{originLeader.Name} spied on {targetLeader.Name}.";
    }
}