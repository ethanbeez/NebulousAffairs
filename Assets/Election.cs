using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Election {
    private int electionTurn;
    private int electionYear;
    private Dictionary<string, Leader> gainedPlanetLeaders;
    private Dictionary<string, Leader> lostPlanetLeaders;
    private Dictionary<string, Planet> newLeaderPlanets;
}
