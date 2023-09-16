using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderHandler {
    private System.Random random;
    public Dictionary<string, Leader> leaders;
    public LeaderHandler() {
        random = new();
        leaders = new();
    }

    public void BuildRandomLeaders(int leadersToCreate) {
        for (int i = 1; i <= leadersToCreate; i++) {
            Leader leader = new("Leader " + i, (float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble(), 
                                               (float) random.NextDouble(), (float) random.NextDouble(), 10, 10, 10);
            leaders.Add(leader.Name, leader);
        }
    }
}
