using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameHandler {
    LeaderHandler leaderHandler;
    PlanetHandler planetHandler;
    public GameHandler() {
        planetHandler = new();
        leaderHandler = new();

        planetHandler.BuildRandomPlanets(12);
        leaderHandler.BuildRandomLeaders(6);
        BuildConnections();
    }

    public void BuildConnections() {
        List<Planet> planetList = planetHandler.planets.Values.ToList();
        int planetIndex = 0;
        foreach (Leader leader in leaderHandler.leaders.Values) {
            while (leader.PlanetControlCount < 2) { 
            
            }            
        }
    }
}
