using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameDataManager {
    private static Dictionary<string, Dictionary<CurrencyType, int>> leaderPreferences;
    public static Dictionary<string, Dictionary<CurrencyType, int>> LeaderPreferences { 
        get { 
            if (leaderPreferences == null) {
                Dictionary<string, Dictionary<CurrencyType, int>> leaderPreferences = FileManager.GetLeaderPreferences();
                LeaderPreferences = leaderPreferences;
                return leaderPreferences;
            }
            return leaderPreferences;
        }
        private set {
            leaderPreferences = value;
        } 
    }
    private static Dictionary<string, Dictionary<CurrencyType, int>> planetPreferences;
    public static Dictionary<string, Dictionary<CurrencyType, int>> PlanetPreferences {
        get {
            if (planetPreferences == null) {
                Dictionary<string, Dictionary<CurrencyType, int>> planetPreferences = FileManager.GetPlanetPreferences();
                PlanetPreferences = planetPreferences;
                return planetPreferences;
            }
            return planetPreferences;
        }
        private set {
            planetPreferences = value;
        }
    }
    private static Dictionary<string, string> initialPlanetLeaders;
    public static Dictionary<string, string> InitialPlanetLeaders {
        get {
            if (initialPlanetLeaders == null) {
                Dictionary<string, string> initialPlanetLeaders = FileManager.GetInitialPlanetLeaders();
                InitialPlanetLeaders = initialPlanetLeaders;
                return initialPlanetLeaders;
            }
            return initialPlanetLeaders;
        }
        private set {
            initialPlanetLeaders = value;
        }
    }
}
