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
    [SerializeField] Animator UIAnim;
    [SerializeField] TextMeshProUGUI playerPolitics;
    [SerializeField] TextMeshProUGUI playerWealth;
    [SerializeField] TextMeshProUGUI playerIntelligence;
    [SerializeField] TextMeshProUGUI playerPoliticsYield;
    [SerializeField] TextMeshProUGUI playerWealthYield;
    [SerializeField] TextMeshProUGUI playerIntelligenceYield;

    public Leader playerLeader;

    [Header("Planet Screen Components")]
    [SerializeField] TextMeshProUGUI planetName;
    [SerializeField] TextMeshProUGUI planetInfo;
    [SerializeField] TextMeshProUGUI planetPoliticsPriority;
    [SerializeField] TextMeshProUGUI planetWealthPriority;
    [SerializeField] TextMeshProUGUI planetIntelligencePriority;

    //[Header("Leader Screen Components")]
    //[SerializeField] Canvas leaderScreen;
    

    //Gets the GameHandler from the GameManager on wakeup
    void OnEnable() {
        gameManager = FindObjectOfType<GameManager>();
    }

    //UHHHHHHHHHH
    void Update() {
        /* playerIntelligence.text = playerLeader.IntelligenceStockpile.ToString();
        playerWealth.text = playerLeader.AffluenceStockpile.ToString();
        playerPolitics.text = playerLeader.PoliticsStockpile.ToString(); */
    }

    //Renders the Main Scene
    public void RenderMainScene(float delayTime){
        UIAnim.SetTrigger("ToNebula");
    }

    //Updates the TurnDisplay to a given String - the expected String is TurnHandler's GameTurns toString.
    public void updateTurnDisplay(String TurnInfo) {
        //whatever the turn counter is gonna be = currentTurn;


        //UpdateMainScreen
        playerIntelligence.text = playerLeader.IntelligenceStockpile.ToString();
        playerWealth.text = playerLeader.AffluenceStockpile.ToString();
        playerPolitics.text = playerLeader.PoliticsStockpile.ToString();
        playerIntelligenceYield.text = "+" + playerLeader.IntelligenceYield.ToString();
        playerWealthYield.text = "+" + playerLeader.AffluenceYield.ToString();
        playerPoliticsYield.text = "+" + playerLeader.PoliticsYield.ToString();

    }

    //Renders the PlanetInfo Screen
    public void RenderPlanetInfo(Planet clickedPlanet, float delayTime, List<(Leader, float)> influenceRatios) {
        planetName.text = clickedPlanet.Name;
        planetInfo.text = clickedPlanet.Name + " is owned by " + clickedPlanet.CurrentLeader.Name;
        planetIntelligencePriority.text = clickedPlanet.IntellectYield.ToString();
        planetPoliticsPriority.text = clickedPlanet.PoliticsYield.ToString();
        planetWealthPriority.text = clickedPlanet.AffluenceYield.ToString();
    }

    //Needs some shit to work first
    public void RenderLeaderInfo(Leader leader){
       
    }

    public void Back() {
        if(mainScreen.enabled) {
            UIAnim.SetTrigger("ToPause");
        } else {
            UIAnim.SetTrigger("ToNebula");
        }
    }



    
}
