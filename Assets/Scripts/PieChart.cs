using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieChart : MonoBehaviour
{
    [SerializeField] Image[] imagesPieChart;
    [SerializeField] string[] names;
    private List<(Leader, float)> influenceRatios;
    
    public void LoadPieChart(List<(Leader, float)> influenceRatios) {
        this.influenceRatios = influenceRatios;
        RenderPieChart();
    }


    private void RenderPieChart() {
        float totalValue = 0f;
        for(int i = 0; i < names.Length; i++)  {
            var leader = CheckLeaderNames(names[i]);
            if(leader.Item2 > 0.01) {
                totalValue += leader.Item2;
                Debug.Log(leader.Item1.Name + " " + leader.Item2);
                imagesPieChart[i].fillAmount = totalValue;
            }
            else {
                imagesPieChart[i].fillAmount = 0;
            }
        }       
    }

    private (Leader, float) CheckLeaderNames(string leaderName) {
        foreach( (Leader, float) leader in influenceRatios) {
            if(leaderName.Equals(leader.Item1.Name)) {
                return leader;
            }

        }
        return (new Leader(leaderName, 0, 0, 0, 0, 0, 0, 0, 0, 0), 0.0f);
        
    }
}
