#nullable enable

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Planet {
    private string name;
    #region Fields
    private int affluenceYield;
    private int politicsYield;
    private int intelligenceYield;
    private Leader? currentLeader;

    // Personality
    private float affluencePriority;
    private float politicsPriority;
    private float intelligencePriority;
    private Dictionary<string, Influence> influences;
    #endregion

    #region Properties
    public string Name => name;
    public int AffluenceYield => affluenceYield;
    public int PoliticsYield => politicsYield;
    public int IntelligenceYield => intelligenceYield;
    // Personality
    public float AffluencePriority => affluencePriority;
    public float PoliticsPriority => politicsPriority;
    public float IntelligencePriority => intelligencePriority;
    public Leader? CurrentLeader => currentLeader;
    #endregion

    #region Constructors
    public Planet(string name, float affluencePriority, float politicsPriority, float intelligencePriority, int affluenceYield = 1, int politicsYield = 1, int intelligenceYield = 1) {
        this.name = name;
        // Priorities
        this.affluencePriority = affluencePriority;
        this.politicsPriority = politicsPriority;
        this.intelligencePriority = intelligencePriority;
        // Yields
        this.affluenceYield = affluenceYield;
        this.politicsYield = politicsYield;
        this.intelligenceYield = intelligenceYield;
        influences = new();
    }
    #endregion

    #region Getters/Setters
    public void SetCurrentLeader(Leader leader) {
        this.currentLeader = leader;
    }

    public List<Influence> GetAllInfluences() {
        return influences.Values.ToList();
    }

    public Influence GetLeaderInfluence(string leaderName) {
        return influences[leaderName];
    }

    public void AddNewInfluence(Influence influence) {
        influences.Add(influence.LeaderName, influence);
    }

    public void SetAffluenceYield(int affluenceYield) { 
        this.affluenceYield = affluenceYield;
    }

    public void SetPoliticsYield(int politicsYield) {
        this.politicsYield = politicsYield;
    }

    public void SetIntelligenceYield(int intelligenceYield) {
        this.intelligenceYield = intelligenceYield;
    }
    #endregion

    #region Actions
    public void HandleDiplomacyAction(DiplomacyAction diplomacyAction) {
        // TODO: All of this logic should probably be a class in Planet.
        float currencyIncreasePriority = 0;
        string increasedCurrency = ""; // TODO: Remove these, mainly just for debugging.
        switch (diplomacyAction.CurrencyToIncrease) {
            case CurrencyType.Affluence:
                currencyIncreasePriority = affluencePriority;
                increasedCurrency = "Gold";
                break;
            case CurrencyType.Politics:
                currencyIncreasePriority = politicsPriority;
                increasedCurrency = "Politics";
                break;
            case CurrencyType.Intellect:
                currencyIncreasePriority = intelligencePriority;
                increasedCurrency = "Intellect";
                break;
        }
        float currencyDecreasePriority = 0;
        string decreasedCurrency = "";
        switch (diplomacyAction.CurrencyToDecrease) {
            case CurrencyType.Affluence:
                currencyDecreasePriority = affluencePriority;
                decreasedCurrency = "Gold";
                break;
            case CurrencyType.Politics:
                currencyDecreasePriority = politicsPriority;
                decreasedCurrency = "Politics";
                break;
            case CurrencyType.Intellect:
                currencyDecreasePriority = intelligencePriority;
                decreasedCurrency = "Intellect";
                break;
        }

        float potentialIncreasePoints = 0.2f; // TODO: Don't hardcode these!
        float potentialDecreasePoints = -0.05f;

        float actualIncreasePoints = potentialIncreasePoints * currencyIncreasePriority;
        float actualDecreasePoints = potentialDecreasePoints * currencyDecreasePriority;
        float totalInfluenceModifier = actualIncreasePoints + actualDecreasePoints;
        Influence leaderInfluence = influences[diplomacyAction.OriginLeader.Name];
        leaderInfluence.UpdateInfluence(totalInfluenceModifier);
        string gainOrLoss = (totalInfluenceModifier > 0) ? "gain" : "loss";
        Debug.Log(leaderInfluence.LeaderName + " performed diplomacy with " + leaderInfluence.PlanetName + ", resulting in +1 " + increasedCurrency + "/turn, and -1 " + decreasedCurrency + "/turn. " +
            "This resulted in a " + gainOrLoss + " of " + totalInfluenceModifier + " Influence points! (new total: " + leaderInfluence.InfluenceValue + ")");
    }
    #endregion
}
