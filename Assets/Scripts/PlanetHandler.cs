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

    public void BuildRandomPlanets(int planetsToCreate) {
        for (int i = 1; i <= planetsToCreate; i++) {
            Planet planet = new("Planet " + i, (float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble(), random.Next(-2, 2), random.Next(-2, 2), random.Next(-2, 2));
            planets.Add(planet.Name, planet);
        }
    }
}
