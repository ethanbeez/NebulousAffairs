#nullable enable

using System.Collections;
using System.Collections.Generic;
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
        // Storing influences
        /*foreach (Influence influence in influences) {
            this.influences.Add(influence.LeaderName, influence);
        }*/
    }
    #endregion

    #region Getters/Setters
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

    public void SetCurrentLeader(Leader leader) {
        currentLeader = leader;
    }
    #endregion

    #region Actions
    public void HandleDiplomacyAction(DiplomacyAction diplomacyAction) {
        CurrencyType currencyToIncrease = diplomacyAction.CurrencyToIncrease;
        CurrencyType currencyToDecrease = diplomacyAction.CurrencyToDecrease;
    }
    #endregion
}
