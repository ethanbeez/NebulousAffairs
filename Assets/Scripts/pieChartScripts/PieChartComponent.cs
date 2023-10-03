using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PieChartComponent : MonoBehaviour
{
    public PieChart pieChart;

    void Start() {
        pieChart = new PieChart();
        GetComponent<UIDocument>().rootVisualElement.Add(pieChart);
    }
}
