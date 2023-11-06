using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetHandler {
    private System.Random random;
    public Dictionary<string, Planet> planets;
    public PlanetHandler() {
        random = new();
        planets = new();
    }

    public void AddPlanet(Planet planet) { 
        planets.Add(planet.Name, planet);
    }

    public void BuildRandomPlanets(int planetsToCreate) {
        for (int i = 1; i <= planetsToCreate; i++) {
            Planet planet = new("Planet " + i, random.Next(-2, 2), random.Next(-2, 2), random.Next(-2, 2));
            planets.Add(planet.Name, planet);
        }
    }

    public void HandleLeaderElimination(string leaderName) {
        foreach (Planet planet in planets.Values) {
            planet.RemoveLeaderInfluence(leaderName);
        }
    }
}
