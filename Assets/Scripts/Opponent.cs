using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Opponent {
    #region Game Constants

    #endregion
    public static int ActionsPerTurn { get; } = 1;

    private OpponentDecisionProfile decisionProfile;

    public Leader Leader { get; }

    public Opponent(Leader leader, float hoarder, float loner, float affluenceBias, float politicsBias, float intelligenceBias) {
        decisionProfile = new(leader, hoarder, loner, affluenceBias, politicsBias, intelligenceBias); // TODO: Make a builder class for this!
        Leader = leader;
    }

    #region Decision-making
    public GameAction MakeDecision() {
        GameAction action = decisionProfile.ChooseNextAction(4);
        return action;
    }
    public void UpdateYieldPriorities() {
        decisionProfile.UpdateYieldPriorities();
    }

    public void UpdatePlanetPriorities() {
        decisionProfile.UpdatePlanetPriorities();
    }

    public void UpdateLeaderPriorities() {
        decisionProfile.UpdateLeaderPriorities();
    }
    #endregion

    /// <summary>
    /// By convention, the Leader who receives the trade deal is responsible for updating their relationship based
    /// on its outcome.
    /// </summary>
    /// <param name="trade"></param>
    /// <returns></returns>
    public bool ProcessIncomingTrade(TradeAction trade) {
        if (trade.TradeWeight < 0) {
            trade.Accepted = false;
            Leader.RefuseIncomingTrade(trade);
            return false;
        }
        // Logic for trade being accepted
        trade.Accepted = true;
        Leader.AcceptIncomingTrade(trade);
        return true;
    }

    public void ProcessOutgoingTradeOutcome(TradeAction trade) {
        if (!trade.Accepted) return;
        Leader.ProcessOutgoingTradeOutcome(trade);
    }

    public void ProcessLeaderElimination(string leaderName) {
        decisionProfile.RemoveRelationship(leaderName);
    }

    // TODO: REMOVE
    /*public void AddNewInfluence(Influence influence) {
        Leader.AddNewInfluence(influence);
        decisionProfile.AddNewPlanet(influence);
    }*/

    public class OpponentDecisionProfile {
        private const int ComfortableAffluenceSurplus = 20;
        private const int ComfortablePoliticsSurplus = 25;
        private const int ComfortableIntelligenceSurplus = 25;
        private const int ComfortableAffluenceYield = 2;
        private const int ComfortablePoliticsYield = 2;
        private const int ComfortableIntelligenceYield = 2;

        private const int ActionsToChooseFrom = 2;
        private const int PlanetsToChooseFrom = 6;
        private const int TradeLeadersToChooseFrom = 3;
        private const int TradeCurrenciesToChooseFrom = 1;
        private const float TradeCurrencyRequestAggression = 0.9f;
        private const float TradeCurrencyOfferAggression = 0.3f; // The minimum percentage of the leader's trade-offered currency stockpile that they will offer.
        private const int MinRequestAmountIfComfortable = 3; // The minimum amount of currency the AI requests if they have a comfortable amount of their most-desired currency.
        private const int MaxRequestAmountIfComfortable = 8; // The maximum amount of currency the AI requests if they have a comfortable amount of their most-desired currency.

        public const float MinimumEquivalentDealRelationshipValue = 1.0f;
        public const int RelationshipTradePenalty = -1;

        private Leader leader;
        private System.Random random;
        // Personality-driven static influences
        private float hoarder; // 0 = values high yields; 1 = values high stockpiles
        private float loner; // 0 = values solo actions; 1 = values leader interactions
        // Static decision influencers
        private float affluenceBias;
        private float politicsBias;
        private float intelligenceBias;
        // Dynamic decision influencers
        private float affluencePriority;
        private float politicsPriority;
        private float intelligencePriority;
        private List<Influence> planetPriorities;
        private List<Relationship> leaderPriorities;

        public float AffluenceRawBias => affluenceBias;
        public float PoliticsRawBias => politicsBias;
        public float IntellectRawBias => intelligenceBias;


        public float AffluencePriority => affluencePriority;
        public float PoliticsPriority => politicsPriority;
        public float IntelligencePriority => intelligencePriority;
        public List<(CurrencyType, float)> CurrencyPriorities { get; private set; }
        public List<Influence> PlanetPriorities => planetPriorities;

        public OpponentDecisionProfile(Leader leader, float hoarder, float loner, float affluenceBias, float politicsBias, float intelligenceBias) {
            this.random = new();
            this.leader = leader;
            this.hoarder = hoarder;
            this.loner = loner;
            this.affluenceBias = affluenceBias;
            this.politicsBias = politicsBias;
            this.intelligenceBias = intelligenceBias;

            planetPriorities = new();
            leaderPriorities = new();
            CurrencyPriorities = new() { (CurrencyType.Affluence, 1.0f), (CurrencyType.Politics, 1.0f), (CurrencyType.Intellect, 1.0f) };
        }

        public void RemoveRelationship(string leaderName) {
            for (int i = 0; i < leaderPriorities.Count; i++) { 
                Relationship relationship = leaderPriorities[i];
                if (relationship.GetOtherLeader(leader.Name).Name == leaderName) {
                    leaderPriorities.RemoveAt(i);
                    return;
                }
            }
        }

        public void AddNewPlanet(Influence influence) {
            planetPriorities.Add(influence);
        }

        public GameAction ChooseNextAction(int actionsToConsider) {
            List<GameAction> possibleActions = GetLikelyActions(actionsToConsider);
            return possibleActions[random.Next(possibleActions.Count)];
        }

        public List<GameAction> GetLikelyActions(int actionsToGet) {
            List<GameAction> likelyActions = new();

            (ActionTypes, float) tradeAction = (ActionTypes.TradeAction, ComputeTradeActionWeight());
            (ActionTypes, float) diplomacyAction = (ActionTypes.DiplomacyAction, ComputeDiplomacyActionWeight());
            (ActionTypes, float) espionageAction = (ActionTypes.TradeAction, ComputeTradeActionWeight()); // TODO: Implement

            List<(ActionTypes, float)> generalActionWeights = new() { espionageAction, diplomacyAction, tradeAction };
            generalActionWeights.Sort((action1, action2) => action1.Item2.CompareTo(action2.Item2));
            int actionsPerCategory = actionsToGet / ActionsToChooseFrom;
            ActionTypes highestPriorityGeneralAction = generalActionWeights[0].Item1;

            for (int i = 0; i < ActionsToChooseFrom; i++) {
                ActionTypes currencyActionType = generalActionWeights[i].Item1;
                PopulateDecisionList(likelyActions, currencyActionType, actionsPerCategory);
            }

            return likelyActions;
        }
        // UNITY PLEASE update your .NET version compatability! Let me live my half-baked polymorphic dreams!! :-(
        public float ComputeTradeActionWeight() {
            if (leader.AffluenceStockpile < TradeAction.AffluenceCost || leader.PoliticsStockpile < TradeAction.PoliticsCost || leader.IntelligenceStockpile < TradeAction.IntellectCost) {
                return -1;
            }

            return Mathf.Clamp(1 - AffluenceRawBias - AffluencePriority, 0, 1);
        }

        public float ComputeDiplomacyActionWeight() {
            if (leader.AffluenceStockpile < DiplomacyAction.AffluenceCost || leader.PoliticsStockpile < DiplomacyAction.PoliticsCost || leader.IntelligenceStockpile < DiplomacyAction.IntellectCost) {
                return -1;
            }

            return Mathf.Clamp(1 - PoliticsRawBias - PoliticsPriority, 0, 1);
        }

        public float ComputeEspionageActionWeight() {
            if (leader.AffluenceStockpile < DiplomacyAction.AffluenceCost || leader.PoliticsStockpile < DiplomacyAction.PoliticsCost || leader.IntelligenceStockpile < DiplomacyAction.IntellectCost) {
                return -1;
            }

            return Mathf.Clamp(1 - IntellectRawBias - IntelligencePriority, 0, 1);
        }

        public void PopulateDecisionList(List<GameAction> gameActionList, ActionTypes gameActionType, int numOfActions) {
            switch (gameActionType) {
                case ActionTypes.EspionageAction:
                    goto case ActionTypes.DiplomacyAction;
                case ActionTypes.DiplomacyAction:
                    for (int i = 0; i < numOfActions; i++) {
                        Array currencies = Enum.GetValues(typeof(CurrencyType));
                        int randomCurrencyIndex = random.Next(currencies.Length);
                        CurrencyType currencyToIncrease = (CurrencyType) currencies.GetValue(randomCurrencyIndex); // TODO: THIS IS STUPID MAKE THIS SMARTER!!
                        int nextCurrencyIndex = (randomCurrencyIndex + 1) % currencies.Length;
                        CurrencyType currencyToDecrease = (CurrencyType) currencies.GetValue(nextCurrencyIndex);
                        Planet targetPlanet = planetPriorities[i + random.Next(PlanetsToChooseFrom)].Planet;
                        gameActionList.Add(new DiplomacyAction(0, leader, targetPlanet, currencyToIncrease, currencyToDecrease)); // TODO: No access to turn occurred
                    }
                    break;
                case ActionTypes.TradeAction:
                    ComputeLikelyTradeActions(gameActionList, numOfActions);
                    break;
            }
        }

        public void ComputeLikelyTradeActions(List<GameAction> gameActionList, int numOfTradeActions) {
            for (int i = 0; i < numOfTradeActions; i++) {
                Leader targetLeader = leaderPriorities[i].GetOtherLeader(leader.Name); // TODO: Key way duplicate trades are prevented right now (10/13). Maybe do something smarter.
                CurrencyType requestedCurrency = CurrencyPriorities[0].Item1;
                int requestAmount = ComputeNeededRequestCurrencyAmount(requestedCurrency);
                List<CurrencyType> offerableCurrencies = new() { CurrencyPriorities[1].Item1, CurrencyPriorities[2].Item1 };
                List<(CurrencyType, int)> offerableCurrencyAmounts = ComputeOfferableCurrencyAmounts(offerableCurrencies, requestAmount, targetLeader);
                List<(CurrencyType, int)> offeredAmounts = MaximizeOffer(offerableCurrencyAmounts, GetOfferedCurrencyDistributions(offerableCurrencies), requestedCurrency, ref requestAmount, targetLeader);
                gameActionList.Add(BuildTradeActionFromValues(offeredAmounts, requestedCurrency, requestAmount, targetLeader));
                /*Leader targetLeader = leaderPriorities[i].GetOtherLeader(leader.Name); // TODO: Key way duplicate trades are prevented right now (10/13). Maybe do something smarter.
                CurrencyType offeredCurrency = FindTradableCurrency();
                if (offeredCurrency == CurrencyPriorities[0].Item1) return; // If the only currency the leader has to trade is the currency they want to request, trade cannot be performed; return early.
                int offeredCurrencyAmount = ComputeTradeOfferedCurrencyAmount(offeredCurrency, targetLeader); // Starts out at the absolute maximum value of currency they have to offer.
                CurrencyType requestedCurrency = CurrencyPriorities[random.Next(TradeCurrenciesToChooseFrom)].Item1;
                int requestedCurrencyAmount = ComputeTradeRequestedCurrencyAmount(requestedCurrency, offeredCurrency, ref offeredCurrencyAmount, targetLeader);

                int requestedAffluence, requestedPolitics, requestedIntellect, offeredAffluence, offeredPolitics, offeredIntellect;
                requestedAffluence = requestedPolitics = requestedIntellect = offeredAffluence = offeredPolitics = offeredIntellect = 0;
                switch (offeredCurrency) {
                    case CurrencyType.Affluence:
                        offeredAffluence = offeredCurrencyAmount;
                        break;
                    case CurrencyType.Politics:
                        offeredPolitics = offeredCurrencyAmount;
                        break;
                    case CurrencyType.Intellect:
                        offeredIntellect = offeredCurrencyAmount;
                        break;
                }
                switch (requestedCurrency) {
                    case CurrencyType.Affluence:
                        requestedAffluence = requestedCurrencyAmount;
                        break;
                    case CurrencyType.Politics:
                        requestedPolitics = requestedCurrencyAmount;
                        break;
                    case CurrencyType.Intellect:
                        requestedIntellect = requestedCurrencyAmount;
                        break;
                }
                TradeAction tradeAction = new(0, leader, targetLeader, offeredAffluence, offeredPolitics, offeredIntellect, requestedAffluence, requestedPolitics, requestedIntellect);
                gameActionList.Add(tradeAction);*/
            }


        }

        private TradeAction BuildTradeActionFromValues(List<(CurrencyType, int)> offeredAmounts, CurrencyType requestedCurrencyType, int requestedCurrencyAmount, Leader targetLeader) {
            int requestedAffluence, requestedPolitics, requestedIntellect, offeredAffluence, offeredPolitics, offeredIntellect;
            requestedAffluence = requestedPolitics = requestedIntellect = offeredAffluence = offeredPolitics = offeredIntellect = 0;
            foreach ((CurrencyType, int) offeredAmount in offeredAmounts) {
                switch (offeredAmount.Item1) {
                    case CurrencyType.Affluence:
                        offeredAffluence = offeredAmount.Item2;
                        break;
                    case CurrencyType.Politics:
                        offeredPolitics = offeredAmount.Item2;
                        break;
                    case CurrencyType.Intellect:
                        offeredIntellect = offeredAmount.Item2;
                        break;
                }
            }
            switch (requestedCurrencyType) {
                case CurrencyType.Affluence:
                    requestedAffluence = requestedCurrencyAmount;
                    break;
                case CurrencyType.Politics:
                    requestedPolitics = requestedCurrencyAmount;
                    break;
                case CurrencyType.Intellect:
                    requestedIntellect = requestedCurrencyAmount;
                    break;
            }
            return new(0, leader, targetLeader, offeredAffluence, offeredPolitics, offeredIntellect, requestedAffluence, requestedPolitics, requestedIntellect);
        }

        private List<(CurrencyType, int)> MaximizeOffer(List<(CurrencyType, int)> offerableAmounts, Dictionary<CurrencyType, float> currencyDistributions, CurrencyType requestedCurrency, ref int requestAmount, Leader targetLeader) {
            int targetLeaderCurrencyStockpile = targetLeader.GetStockpileFromCurrencyType(requestedCurrency);
            // Only request as much as the target leader has stockpiled.
            requestAmount = Math.Min(requestAmount, targetLeaderCurrencyStockpile);
            int knownRequestWeight = requestAmount;
            if (leader.GetLeaderPreferenceVisibility(targetLeader, requestedCurrency)) {
                knownRequestWeight += targetLeader.GetCurrencyPreferenceFromType(requestedCurrency);
            }
            int knownOfferWeight = 0;
            if (targetLeader.GetRelationshipValue(leader.Name) < MinimumEquivalentDealRelationshipValue) {
                knownOfferWeight += RelationshipTradePenalty;
            }
            List<(CurrencyType, int)> offeredAmounts = new();
            for (int i = 0; i < offerableAmounts.Count && knownOfferWeight < requestAmount; i++) {
                (CurrencyType, int) offerableAmount = offerableAmounts[i];
                int knownModifier = 0;
                if (leader.GetLeaderPreferenceVisibility(targetLeader, offerableAmount.Item1)) {
                    knownModifier = targetLeader.GetCurrencyPreferenceFromType(offerableAmount.Item1);
                }
                // If the currency pref value is less than 0 and the amount that we're willing to offer has a resultant effective weight of 0 or less,
                // offering this currency in the trade provides 0/negative value; remove this currency from the offerable amounts.
                if (knownModifier < 0 && offerableAmount.Item2 + knownModifier <= 0) {
                    continue;
                }
                float currencyDistribution = currencyDistributions[offerableAmount.Item1];
                int requestWeightPortion = (int) (currencyDistribution * knownRequestWeight);
                int actualOfferAmount = Math.Min(offerableAmount.Item2, requestWeightPortion);
                knownOfferWeight = knownOfferWeight + actualOfferAmount + knownModifier;
                offeredAmounts.Add((offerableAmount.Item1, actualOfferAmount));
            }
            requestAmount = Math.Min(requestAmount, knownOfferWeight - targetLeader.GetCurrencyPreferenceFromType(requestedCurrency));
            return offeredAmounts;
        }

        private CurrencyType FindTradableCurrency() {
            CurrencyType offeredCurrency = CurrencyPriorities[0].Item1;
            bool foundTradableCurrency = false;
            for (int j = CurrencyPriorities.Count - 1; j >= 0; j--) {
                offeredCurrency = CurrencyPriorities[j].Item1;
                switch (offeredCurrency) {
                    case CurrencyType.Affluence:
                        if (leader.AffluenceStockpile >= 1) {
                            foundTradableCurrency = true;
                        }
                        break;
                    case CurrencyType.Politics:
                        if (leader.PoliticsStockpile >= 1) {
                            foundTradableCurrency = true;
                        }
                        break;
                    case CurrencyType.Intellect:
                        if (leader.IntelligenceStockpile >= 1) {
                            foundTradableCurrency = true;
                        }
                        break;
                }
                if (foundTradableCurrency) break;
            }
            return offeredCurrency;

        }

        private int ComputeNeededRequestCurrencyAmount(CurrencyType requestedCurrency) {
            int neededAmount;
            switch (requestedCurrency) {
                case CurrencyType.Affluence:
                    neededAmount = ComfortableAffluenceSurplus - leader.AffluenceStockpile;
                    break;
                case CurrencyType.Politics:
                    neededAmount = ComfortablePoliticsSurplus - leader.PoliticsStockpile;
                    break;
                case CurrencyType.Intellect:
                    neededAmount = ComfortablePoliticsSurplus - leader.IntelligenceStockpile;
                    break;
                default:
                    Debug.LogError("OpponentDecisionProfile.ComputeNeededRequestCurrencyAmount: A target requested currency had an invalid CurrencyType enum type.");
                    neededAmount = 0;
                    break;
            }
            if (neededAmount < 0) {
                neededAmount = random.Next(MinRequestAmountIfComfortable, MaxRequestAmountIfComfortable);
            }
            return neededAmount;
        }

        private int ComputeTradeRequestedCurrencyAmount(CurrencyType requestedCurrency, CurrencyType offeredCurrency, ref int offeredCurrencyAmount, Leader targetLeader) {
            int offerModifiers = 0;
            // Accounts for targetLeader relationship value.
            if (targetLeader.GetRelationshipValue(leader.Name) < MinimumEquivalentDealRelationshipValue) {
                offerModifiers += RelationshipTradePenalty;
            }
            switch (offeredCurrency) {
                case CurrencyType.Affluence:
                    offerModifiers += targetLeader.AffluencePreference;
                    break;
                case CurrencyType.Politics:
                    offerModifiers += targetLeader.PoliticsPreference;
                    break;
                case CurrencyType.Intellect:
                    offerModifiers += targetLeader.IntellectPreference;
                    break;
            }

            int maxWantedAmount;
            int maxRequestableAmount;
            int targetRequestedStockpile;
            switch (requestedCurrency) {
                case CurrencyType.Affluence:
                    offerModifiers -= targetLeader.AffluencePreference;
                    targetRequestedStockpile = targetLeader.AffluenceStockpile;
                    maxRequestableAmount = Math.Min(offeredCurrencyAmount + offerModifiers, targetRequestedStockpile);
                    if (maxRequestableAmount < 1) {
                        maxWantedAmount = 1;
                    } else {
                        maxWantedAmount = Math.Clamp(ComfortableAffluenceSurplus - leader.AffluenceStockpile, 1, maxRequestableAmount);
                    }
                    break;
                case CurrencyType.Politics:
                    offerModifiers -= targetLeader.PoliticsPreference;
                    targetRequestedStockpile = targetLeader.PoliticsStockpile;
                    maxRequestableAmount = Math.Min(offeredCurrencyAmount + offerModifiers, targetRequestedStockpile);
                    if (maxRequestableAmount < 1) {
                        maxWantedAmount = 1;
                    } else {
                        maxWantedAmount = Math.Clamp(ComfortablePoliticsSurplus - leader.PoliticsStockpile, 1, maxRequestableAmount);
                    }
                    break;
                case CurrencyType.Intellect:
                    offerModifiers -= targetLeader.IntellectPreference;
                    targetRequestedStockpile = targetLeader.IntelligenceStockpile;
                    maxRequestableAmount = Math.Min(offeredCurrencyAmount + offerModifiers, targetRequestedStockpile);
                    if (maxRequestableAmount < 1) {
                        maxWantedAmount = 1;
                    } else {
                        maxWantedAmount = Math.Clamp(ComfortableIntelligenceSurplus - leader.IntelligenceStockpile, 1, maxRequestableAmount);
                    }
                    break;
                default:
                    Debug.LogError("OpponentDecisionProfile.ComputeTradeRequestedCurrencyAmount: A target requested currency had an invalid CurrencyType enum type.");
                    maxWantedAmount = 0;
                    break;
            }
            int minWantedAmount = Math.Clamp((int) (maxWantedAmount * TradeCurrencyRequestAggression), 1, maxWantedAmount);
            int amountToRequest = random.Next(minWantedAmount, maxWantedAmount);
            offeredCurrencyAmount = Math.Clamp(amountToRequest - offerModifiers, 1, 1000); // TODO: 1000 temp here.
            return amountToRequest;
        }

        private List<(CurrencyType, int)> ComputeOfferableCurrencyAmounts(List<CurrencyType> offerableCurrencies, int wantedRequestAmount, Leader targetLeader) {
            // TODO: The following commented out logic is for AI trades that offer more than one currency.   
            List<(CurrencyType, int)> offeredCurrencyAmounts = new();
            foreach (CurrencyType currencyType in offerableCurrencies) {
                int offerableCurencyAmount = ComputeOfferableCurrencyAmount(currencyType);
                if (offerableCurencyAmount <= 0) {
                    continue;
                }
                offeredCurrencyAmounts.Add((currencyType, offerableCurencyAmount));
            }
            /*int maxOfferedCurrencyAmount;
            switch (offeredCurrency) {
                case CurrencyType.Affluence:
                    maxOfferedCurrencyAmount = leader.AffluenceStockpile - TradeAction.AffluenceCost;
                    break;
                case CurrencyType.Politics:
                    maxOfferedCurrencyAmount = leader.PoliticsStockpile - TradeAction.PoliticsCost;
                    break;
                case CurrencyType.Intellect:
                    maxOfferedCurrencyAmount = leader.IntelligenceStockpile - TradeAction.IntellectCost;
                    break;
                default:
                    Debug.LogError("OpponentDecisionProfile.ComputeTradeMaxOfferedCurrencyAmount: A target requested currency had an invalid CurrencyType enum type.");
                    maxOfferedCurrencyAmount = 0;
                    break;
            }*/
            // int minOfferedCurrencyAmount = (int) (maxOfferedCurrencyAmount * TradeCurrencyOfferAggression);
            // return random.Next(minOfferedCurrencyAmount, maxOfferedCurrencyAmount);
            return offeredCurrencyAmounts;
        }

        private int ComputeOfferableCurrencyAmount(CurrencyType currencyType) {
            int currencyStockpile = leader.GetStockpileFromCurrencyType(currencyType);
            int currencyYield = leader.GetYieldFromCurrencyType(currencyType);
            if (currencyYield < 0) { 
                return (int) (0.25 *  currencyStockpile);
            }
            int comfortableExcess = currencyStockpile - GetComfortableCurrencyAmountFromType(currencyType);
            int offerableAmount = Math.Min(currencyStockpile, currencyYield * (int) (TurnHandler.TurnLimit * 0.1));
            if (comfortableExcess > 0) {
                return offerableAmount + comfortableExcess;
            }
            return offerableAmount;
        }

        private static int GetComfortableCurrencyAmountFromType(CurrencyType currencyType) {
            switch (currencyType) {
                case CurrencyType.Affluence:
                    return ComfortableAffluenceSurplus;
                case CurrencyType.Politics:
                    return ComfortablePoliticsSurplus;
                case CurrencyType.Intellect:
                    return ComfortableIntelligenceSurplus;
            }
            Debug.LogError("OpponentDecisionProfile.GetComfortableCurrencyAmountFromType: An invalid CurrencyType enum value was provided! Default returning 0.");
            return 0;
        }

        private Dictionary<CurrencyType, float> GetOfferedCurrencyDistributions(List<CurrencyType> offeredCurrencies) {
            Dictionary<CurrencyType, float> offeredCurrenciesDistribution = new();
            float totalPriority = 0f;
            foreach (CurrencyType currencyType in offeredCurrencies) {
                switch (currencyType) {
                    case CurrencyType.Affluence:
                        totalPriority += affluencePriority;
                        break;
                    case CurrencyType.Politics:
                        totalPriority += politicsPriority;
                        break;
                    case CurrencyType.Intellect:
                        totalPriority += intelligencePriority;
                        break;
                }
            }
            foreach (CurrencyType currencyType in offeredCurrencies) {
                switch (currencyType) {
                    case CurrencyType.Affluence:
                        offeredCurrenciesDistribution.Add(CurrencyType.Affluence, 1 - (affluencePriority / totalPriority));
                        break;
                    case CurrencyType.Politics:
                        offeredCurrenciesDistribution.Add(CurrencyType.Politics, 1 - (politicsPriority / totalPriority));
                        break;
                    case CurrencyType.Intellect:
                        offeredCurrenciesDistribution.Add(CurrencyType.Intellect, 1 - (intelligencePriority / totalPriority));
                        break;
                }
            }
            return offeredCurrenciesDistribution;
        }

        public void UpdateYieldPriorities() {
            UpdateAffluencePriority();
            UpdatePoliticsPriority();
            UpdateIntelligencePriority();
        }


        public void UpdateLeaderPriorities() {
            leaderPriorities = leader.GetAscendingSortedLeaderRelationships();
        }

        public void UpdatePlanetPriorities() {
            planetPriorities = leader.GetAscendingSortedPlanetInfluences();
        }

        private void UpdateAffluencePriority() {
            float surplusFactor = 1 - (Mathf.Clamp(leader.AffluenceStockpile, 0, 1_000) / (ComfortableAffluenceSurplus * 2)); // Calculate actual theoretical maximums
            float yieldFactor = 1 - (Mathf.Clamp(leader.AffluenceYield, 0, 1_000) / (ComfortableAffluenceYield * 2));
            float sumFactors = Mathf.Clamp(surplusFactor, 0, 1) + Mathf.Clamp(yieldFactor, 0, 1);
            affluencePriority = Mathf.Clamp(sumFactors, 0, 1);
        }

        private void UpdatePoliticsPriority() {
            float surplusFactor = 1 - (Mathf.Clamp(leader.PoliticsStockpile, 0, 1_000) / (ComfortablePoliticsSurplus * 2)); // Calculate actual theoretical maximums
            float yieldFactor = 1 - (Mathf.Clamp(leader.PoliticsYield, 0, 1_000) / (ComfortablePoliticsYield * 2));
            float sumFactors = Mathf.Clamp(surplusFactor, 0, 1) + Mathf.Clamp(yieldFactor, 0, 1);
            politicsPriority = Mathf.Clamp(sumFactors, 0, 1);
        }

        private void UpdateIntelligencePriority() {
            float surplusFactor = 1 - (Mathf.Clamp(leader.IntelligenceStockpile, 0, 1_000) / (ComfortableIntelligenceSurplus * 2)); // Calculate actual theoretical maximums
            float yieldFactor = 1 - (Mathf.Clamp(leader.IntelligenceYield, 0, 1_000) / (ComfortableIntelligenceYield * 2));
            float sumFactors = Mathf.Clamp(surplusFactor, 0, 1) + Mathf.Clamp(yieldFactor, 0, 1);
            intelligencePriority = Mathf.Clamp(sumFactors, 0, 1);
        }

    }
}

public static class OpponentDecisionProfileExtensions {
    public static bool TradeActionExists(this List<GameAction> actionList, string targetLeaderName) {
        foreach (GameAction gameAction in actionList) { 
            if (gameAction.GetType() != typeof(TradeAction)) {
                TradeAction tradeAction = (TradeAction) gameAction;
                if (tradeAction.TargetLeader.Name == targetLeaderName) return true;
            }
        }
        return false;
    }
}
