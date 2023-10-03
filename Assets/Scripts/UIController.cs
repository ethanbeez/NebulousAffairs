using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using TMPro;
using System.Runtime.InteropServices;
using System;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

    GameManager gameHandler;

    [Header("Main Screen Components")]
    [SerializeField] Canvas mainScreen;
    [SerializeField] TextMeshProUGUI turnDisplay;

    [Header("Planet Sceen Components")]
    [SerializeField] Canvas planetScreen;
    [SerializeField] TextMeshProUGUI planetName;
    [SerializeField] TextMeshProUGUI planetInfo;
    [SerializeField] TextMeshProUGUI planetPoliticsPriority;
    [SerializeField] TextMeshProUGUI planetWealthPriority;
    [SerializeField] TextMeshProUGUI planetIntelligencePriority;
    

    //Gets the GameHandler from the GameManager on wakeup
    void Awake() {
        gameHandler = FindObjectOfType<GameManager>();
    }

    //Renders the Main Scene
    public void RenderMainScene(float delayTime){
        DerenderPanels();
        Invoke("RenderMain", delayTime);
    }

    //Helper method for RenderMainScene(), used to add a delay time to the Render
    private void RenderMain() {
        mainScreen.enabled = true;
    }


    //Updates the TurnDisplay to a given String - the expected String is TurnHandler's GameTurns toString.
    public void updateTurnDisplay(String currentTurn) {
        turnDisplay.text = currentTurn;
    }

    //Renders the PlanetInfo Screen
    public void RenderPlanetInfo(Planet clickedPlanet, float delayTime) {
        DerenderPanels();
        planetName.text = clickedPlanet.Name;
        planetInfo.text = clickedPlanet.Name + " is owned by " + clickedPlanet.CurrentLeader.Name;
        planetIntelligencePriority.text = clickedPlanet.IntelligencePriority.ToString();
        planetPoliticsPriority.text = clickedPlanet.PoliticsPriority.ToString();
        planetWealthPriority.text = clickedPlanet.PoliticsPriority.ToString();

        Invoke("RenderPlanet", delayTime);
    }

    //Delayed method for RenderPlanetInfo
    private void RenderPlanet() {
        planetScreen.enabled = true;
    }

    //Derenders all active panels
    private void DerenderPanels() {
        mainScreen.enabled = false;
        planetScreen.enabled = false;
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



    
}
