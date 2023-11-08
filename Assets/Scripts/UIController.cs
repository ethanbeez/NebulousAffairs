using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using TMPro;
using System.Runtime.InteropServices;
using System;
using UnityEngine.UI;
using System.Linq;
public class UIController : MonoBehaviour {

    GameManager gameManager;
    InputManager inputManager;
    ButtonController buttonController;
    [SerializeField] GameObject leaderButtonPrefab;

    [Header("Main Screen Components")]
    [SerializeField] Animator UIAnim;
    [SerializeField] ActionCounter actionCounter;
    [SerializeField] TextMeshProUGUI playerPolitics;
    [SerializeField] TextMeshProUGUI playerWealth;
    [SerializeField] TextMeshProUGUI playerIntelligence;
    [SerializeField] TextMeshProUGUI playerPoliticsYield;
    [SerializeField] TextMeshProUGUI playerWealthYield;
    [SerializeField] TextMeshProUGUI playerIntelligenceYield;
    [SerializeField] Transform LogData;
    [SerializeField] TextMeshProUGUI LogPrefab;
    public Leader playerLeader;

    [Header("Planet Screen Components")]
    [SerializeField] TextMeshProUGUI planetName;
    [SerializeField] TextMeshProUGUI planetInfo;
    [SerializeField] TextMeshProUGUI planetPoliticsPriority;
    [SerializeField] TextMeshProUGUI planetWealthPriority;
    [SerializeField] TextMeshProUGUI planetIntelligencePriority;
    [SerializeField] TextMeshProUGUI planetPoliticsYield;
    [SerializeField] TextMeshProUGUI planetWealthYield;
    [SerializeField] TextMeshProUGUI planetIntelligenceYield;
    [SerializeField] PieChart pieChart;
 
    [Header("Leader Screen Components")]
    [SerializeField] Image leaderImage;
    [SerializeField] TextMeshProUGUI leaderPoliticsPriority;
    [SerializeField] TextMeshProUGUI leaderIntelligencePriority;
    [SerializeField] TextMeshProUGUI leaderWealthPriority;
    [SerializeField] TextMeshProUGUI leaderPoliticsStockpile;
    [SerializeField] TextMeshProUGUI leaderIntelligenceStockpile;
    [SerializeField] TextMeshProUGUI leaderWealthStockpile;
    [SerializeField] TradeUIController tradeUI;

    private List<(string, string)> leaderButtonData; 
    Leader currentLeader;
    private int espionageResource;
    public delegate void EspionageConfirm(int resource, Leader enemyLeader);
    public static event EspionageConfirm? ConfirmEspionage;
    

    //Gets the GameHandler from the GameManager on wakeup
    void OnEnable() {
        buttonController = new(leaderButtonPrefab);
        gameManager = FindObjectOfType<GameManager>();
        inputManager = FindObjectOfType<InputManager>();
        AddToLog("Good Luck - The Galaxy Depends on You");
    }

    void Update() {
        
    }

    public void InstantiateButtons(List<(string, string)> leaderButtonData) {
        this.leaderButtonData = leaderButtonData;
        buttonController.InstantiateLeaderButtons(leaderButtonData);
    }

    //Renders the Main Scene
    public void RenderMainScene(){
        UIAnim.SetTrigger("Back");
    }

    //Updates the TurnDisplay to a given String - the expected String is TurnHandler's GameTurns toString.
    public void UpdateTurnDisplay(String TurnInfo) {
        //whatever the turn counter is gonna be = currentTurn;


        //Pie Chart should be updated when turn ends. yipee.
        //pieChart.LoadPieChart(influenceRatios);
        UpdateMainScreen();
        UpdateActionDisplay(3);

    }

    private void UpdateMainScreen() {
        playerIntelligence.text = playerLeader.IntelligenceStockpile.ToString();
        playerWealth.text = playerLeader.AffluenceStockpile.ToString();
        playerPolitics.text = playerLeader.PoliticsStockpile.ToString();
        playerIntelligenceYield.text = "+" + playerLeader.IntelligenceYield.ToString();
        playerWealthYield.text = "+" + playerLeader.AffluenceYield.ToString();
        playerPoliticsYield.text = "+" + playerLeader.PoliticsYield.ToString();
    }

    //Renders the PlanetInfo Screen
    public void RenderPlanetInfo(Planet clickedPlanet, List<(Leader, float)> influenceRatios) {
        planetName.text = clickedPlanet.Name;
        planetInfo.text = clickedPlanet.Name + " is owned by " + clickedPlanet.CurrentLeader.Name;

        //Espionage Needs Implementation for these
        planetIntelligencePriority.text = clickedPlanet.IntelligencePriority.ToString();
        planetPoliticsPriority.text = clickedPlanet.PoliticsPriority.ToString();
        planetWealthPriority.text = clickedPlanet.AffluencePriority.ToString();
        planetPoliticsYield.text = clickedPlanet.PoliticsYield.ToString();
        planetIntelligencePriority.text = clickedPlanet.IntellectYield.ToString();
        planetWealthYield.text = clickedPlanet.AffluenceYield.ToString();
        pieChart.LoadPieChart(influenceRatios);
        
        UIAnim.SetTrigger("ToPlanet");
    }

    //Renders the LeaderInfo Screen Relative to a given Leader
    public void RenderLeaderInfo(Leader leader){


       currentLeader = leader; 
       // Should be determined by Espionage
       if(!playerLeader.GetLeaderPreferenceVisibility(leader, CurrencyType.Intellect)) {
            leaderIntelligencePriority.text = leader.IntellectPreference.ToString();
       }
       else {
            leaderIntelligencePriority.text = "?";
       }
       if(!playerLeader.GetLeaderPreferenceVisibility(leader, CurrencyType.Politics)) {
            leaderPoliticsPriority.text = leader.PoliticsPreference.ToString();
       }
       else {
            leaderPoliticsPriority.text = "?";
       }
       if(!playerLeader.GetLeaderPreferenceVisibility(leader, CurrencyType.Affluence)) {
            leaderWealthPriority.text = leader.AffluencePreference.ToString();
       }
       else {
            leaderWealthPriority.text = "?";
       }

       leaderIntelligenceStockpile.text = leader.IntelligenceStockpile.ToString();
       leaderPoliticsStockpile.text = leader.PoliticsStockpile.ToString();
       leaderWealthStockpile.text = leader.AffluenceStockpile.ToString();
       
       //Need Ethan to Work on this
       leaderImage.sprite = FileManager.GetLeaderImageFromFileName(leader.GetLeaderImagePath(LeaderResources.Perspectives.Full, LeaderResources.Expressions.Neutral));

       //check if coming from Planet
       if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Planet"))
            inputManager.CameraToMapPosition();
        
        UIAnim.SetTrigger("ToLeader");

        UpdateMainScreen();
    } 

    public void UpdateLog(IEnumerable<GameEvent> gameEvents) {
        Debug.Log(gameEvents);
        for(int i = LogData.childCount; i > 0; i--) {
            GameObject.Destroy(LogData.GetChild(0).gameObject);
        }
        foreach(GameEvent gameEvent in gameEvents) {
            AddToLog(gameEvent.ToString());
        }
    }
    
    /// <summary>
    /// Adds a given string into the Log, formatting with Font and Text Size, while making visible to the player
    /// </summary>
    /// <param name="LogInfo"> The info that is added to the log</param>
    /// <returns>the current number of unique text instances within the log</returns>
    private void AddToLog(string LogInfo) {
        var LogText = Instantiate(LogPrefab, LogData);
        LogText.text = LogInfo;
    }

    public void Back() {
        if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Planet")) {
            inputManager.CameraToMapPosition();
        }
        UIAnim.SetTrigger("Back");
    }

    public void Trade() {
        tradeUI.BeginTrade(playerLeader, currentLeader, leaderButtonData);
        UIAnim.SetTrigger("Trade");
    }

    public void UpdateActionDisplay(int PlayerTurnActionsLeft) {
        actionCounter.UpdateActionDisplay(PlayerTurnActionsLeft);
    }

    public void ChooseEspionage(int resource) {
        espionageResource = resource;
        UIAnim.SetTrigger("ToConfirm");

    }

    public void CompleteEspionage() {
        ConfirmEspionage.Invoke(espionageResource, currentLeader);
        UIAnim.SetTrigger("ToLeader");
    }

    
}
