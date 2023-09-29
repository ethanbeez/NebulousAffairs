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

    public void RenderMainScene(){
        mainScreen.enabled = true;
        planetScreen.enabled = false;
    }

    //Updates the TurnDisplay to a given String - the expected String is TurnHandler's GameTurns toString.
    public void updateTurnDisplay(String currentTurn) {
        turnDisplay.text = currentTurn;
    }

    public void RenderPlanetInfo(Planet clickedPlanet, float delayTime) {
        mainScreen.enabled = false;
        planetName.text = clickedPlanet.Name;
        planetInfo.text = clickedPlanet.Name + " is owned by " + clickedPlanet.CurrentLeader.Name;
         planetIntelligencePriority.text = clickedPlanet.IntelligencePriority.ToString();
        planetPoliticsPriority.text = clickedPlanet.PoliticsPriority.ToString();
        planetWealthPriority.text = clickedPlanet.PoliticsPriority.ToString();

        Invoke("RenderPlanet", delayTime);
    }

    private void RenderPlanet() {
        planetScreen.enabled = true;
    }






    
}
