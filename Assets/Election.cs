using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Election {
    private int electionTurn;
    private int electionYear;
    private Dictionary<string, Planet> gainedPlanetLeaders;
    private Dictionary<string, Planet> lostPlanetLeaders;
    // private Dictionary<string, List<(Leader, float)>> planetVoteRatios; // TODO: Probably don't need to store?

    public Dictionary<string, Planet> GainedPlanetLeaders => gainedPlanetLeaders;
    public Dictionary<string, Planet> LostPlanetLeaders => lostPlanetLeaders;

    public Election(int electionTurn, int electionYear) {
        // this.planetVoteRatios = new();
        gainedPlanetLeaders = new();
        lostPlanetLeaders = new();
        this.electionTurn = electionTurn;
        this.electionYear = electionYear;
    }

    public void DeterminePlanetElectionOutcome(Planet planet) {
        float largestInfluenceValue = -1;
        string newEmperorName = "";
        foreach (Influence influence in planet.GetAllInfluences()) {
            if (influence.InfluenceValue > largestInfluenceValue) {
                largestInfluenceValue = influence.InfluenceValue;
                newEmperorName = influence.LeaderName;
            }
        }
        if (!string.Equals(newEmperorName, planet.CurrentLeader.Name)) {
            gainedPlanetLeaders.Add(newEmperorName, planet);
            lostPlanetLeaders.Add(planet.CurrentLeader.Name, planet);
        }
    }

    // TODO: Maybe make instance later
    public static Dictionary<string, List<(Leader, float)>> GetPlanetInfluenceRatios(List<Planet> planets) {
        Dictionary<string, List<(Leader, float)>> planetVoteRatios = new();
        foreach (Planet planet in planets) {
            List<(Leader, float)> voteRatios = new();
            // TODO: Not exactly optimal code here.
            List<Influence> influences = planet.GetAllInfluences();
            float influenceSum = 0;
            foreach (Influence influence in influences) {
                influenceSum += influence.InfluenceValue;
            }
            foreach (Influence influence in influences) {
                voteRatios.Add((influence.Leader, influence.InfluenceValue / influenceSum));
            }
            voteRatios.Sort((ratio1, ratio2) => ratio2.Item2.CompareTo(ratio1.Item2));
            planetVoteRatios.Add(planet.Name, voteRatios);
        }
        return planetVoteRatios;
    }
}
