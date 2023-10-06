using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using TMPro;
using System.Runtime.InteropServices;
using System;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour {

    GameManager gameManager;

    [Header("Main Screen Components")]
    [SerializeField] Canvas mainScreen;
    [SerializeField] TextMeshProUGUI turnDisplay;
    [SerializeField] TextMeshProUGUI playerPolitics;
    [SerializeField] TextMeshProUGUI playerWealth;
    [SerializeField] TextMeshProUGUI playerIntelligence;
    public Leader playerLeader;

    [Header("Planet Screen Components")]
    [SerializeField] Canvas planetScreen;
    [SerializeField] TextMeshProUGUI planetName;
    [SerializeField] TextMeshProUGUI planetInfo;
    [SerializeField] TextMeshProUGUI planetPoliticsPriority;
    [SerializeField] TextMeshProUGUI planetWealthPriority;
    [SerializeField] TextMeshProUGUI planetIntelligencePriority;
    [SerializeField] PieChart pieChart;
    PieChartComponent pcc;
    [SerializeField] UIDocument pieChartDoc;

    [Header("Leader Screen Components")]
    [SerializeField] Canvas leaderScreen;
    

    //Gets the GameHandler from the GameManager on wakeup
    void OnEnable() {
        gameManager = FindObjectOfType<GameManager>();
        if (GameObject.Find("PlanetScreen/PieChart").GetComponent<PieChartComponent>() == null) {
            Application.Quit();
        }
        pcc = GameObject.Find("PlanetScreen/PieChart").GetComponent<PieChartComponent>();
        // pieChart = GameObject.Find("PlanetScreen/PieChart").GetComponent<PieChartComponent>().pieChart;
        // pieChart = FindObjectOfType<PieChartComponent>().pieChart;
        /*if (pieChart == null) {
            Instantiate()
            GameObject.Find().
        }*/
        // actualPieChart = pieChart.pieChart;
        /*if (actualPieChart == null) {
            Application.Quit();
        }*/

        pieChartDoc.enabled = true;
        pieChartDoc.rootVisualElement.style.display = DisplayStyle.None;
    }

    //UHHHHHHHHHH
    void Update() {
        pieChart = pcc.pieChart;
        playerIntelligence.text = playerLeader.IntelligenceStockpile.ToString();
        playerWealth.text = playerLeader.AffluenceStockpile.ToString();
        playerPolitics.text = playerLeader.PoliticsStockpile.ToString();
    }

    //Renders the Main Scene
    public void RenderMainScene(float delayTime){
        DerenderPanels();
        playerIntelligence.text = playerLeader.IntelligenceStockpile.ToString();
        playerWealth.text = playerLeader.AffluenceStockpile.ToString();
        playerPolitics.text = playerLeader.PoliticsStockpile.ToString();
        Invoke("RenderMain", delayTime);
    }

    //Helper method for RenderMainScene(), used to add a delay time to the Render
    private void RenderMain() {
        mainScreen.enabled = true;
    }


    //Updates the TurnDisplay to a given String - the expected String is TurnHandler's GameTurns toString.
    public void updateTurnDisplay(String currentTurn, int planetsControlled, int state) {
        turnDisplay.text = currentTurn;
        turnDisplay.text += ", Planets Controlled: " + planetsControlled;
        if (state == 1) {
            turnDisplay.text += "\nYOU LOST!";
        } else if (state == 2) {
            turnDisplay.text += "\nYOU WON!";
        }
    }

    //Renders the PlanetInfo Screen
    public void RenderPlanetInfo(Planet clickedPlanet, float delayTime, List<(Leader, float)> influenceRatios) {
        try {
            DerenderPanels();
            planetName.text = clickedPlanet.Name;
            planetInfo.text = clickedPlanet.Name + " is owned by " + clickedPlanet.CurrentLeader.Name;
            planetIntelligencePriority.text = clickedPlanet.IntelligencePriority.ToString();
            planetPoliticsPriority.text = clickedPlanet.PoliticsPriority.ToString();
            planetWealthPriority.text = clickedPlanet.PoliticsPriority.ToString();

            //this is about to be the world's shnastiest code
            foreach ((Leader, float) influenceRatio in influenceRatios) {
                string leaderName = influenceRatio.Item1.Name;
                switch (leaderName) {
                    case "Leader 1":
                        pieChart.leader1 = influenceRatio.Item2;
                        break;
                    case "Leader 2":
                        pieChart.leader2 = influenceRatio.Item2;
                        break;
                    case "Leader 3":
                        pieChart.leader3 = influenceRatio.Item2;
                        break;
                    case "Leader 4":
                        pieChart.leader4 = influenceRatio.Item2;
                        break;
                    case "Leader 5":
                        pieChart.leader5 = influenceRatio.Item2;
                        break;
                    case "Leader 6":
                        pieChart.leader6 = influenceRatio.Item2;
                        break;
                }

            }


            Invoke("RenderPlanet", delayTime);
        } catch (Exception e) {
            Application.Quit();
        }
        
    }

    //Delayed method for RenderPlanetInfo
    private void RenderPlanet() {
        planetScreen.enabled = true;
        pieChartDoc.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    //Derenders all active panels
    private void DerenderPanels() {
        mainScreen.enabled = false;
        planetScreen.enabled = false;
        leaderScreen.enabled = false;
        pieChartDoc.rootVisualElement.style.display = DisplayStyle.None;
    }

    //Needs some shit to work first
    public void RenderLeaderInfo(Leader leader){
        /*
            Process for Rendering Leader Info:
            Create an Event that represents whether a button has been clicked or not
            Update that event when a button is clicked, passing in a reference to the Leader's name
            Check for that Event being called in GameManager
            Call GameManager.gameHandler.leaderHandler.leaders.get(name), passing in a Leader to RenderLeaderInfo()
            Render the Leader Info
        */
    }

    //TEMP for now - will be deleted after Prototype Build
    public void RenderLeaderInfo() {
        DerenderPanels();
        leaderScreen.enabled = true;
    }



    
}
