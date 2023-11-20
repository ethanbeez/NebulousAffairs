using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
public class UIController : MonoBehaviour {

    GameManager gameManager;
    InputManager inputManager;
    ButtonController buttonController;
    [SerializeField] GameObject leaderButtonPrefab;
    CameraController cameraController;

    [Header("Main Screen Components")]
    [SerializeField] public Animator UIAnim;
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
    [SerializeField] TextMeshProUGUI planetPoliticsYield;
    [SerializeField] TextMeshProUGUI planetWealthYield;
    [SerializeField] TextMeshProUGUI planetIntelligenceYield;
    [SerializeField] TextMeshProUGUI planetDescription;
    [SerializeField] Image planetImage;
    [SerializeField] PieChart pieChart;
    [SerializeField] Campaign campaign;
 
    [Header("Leader Screen Components")]
    [SerializeField] Image leaderImage;
    [SerializeField] TextMeshProUGUI leaderPoliticsPriority;
    [SerializeField] TextMeshProUGUI leaderIntelligencePriority;
    [SerializeField] TextMeshProUGUI leaderWealthPriority;
    [SerializeField] TextMeshProUGUI leaderPoliticsStockpile;
    [SerializeField] TextMeshProUGUI leaderIntelligenceStockpile;
    [SerializeField] TextMeshProUGUI leaderWealthStockpile;
    [SerializeField] TradeUIController tradeUI;
    [SerializeField] Transform converseData;
    [SerializeField] TextMeshProUGUI ConversePrefab;

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
        cameraController = FindObjectOfType<CameraController>();
        AddToLog("Good Luck - The Galaxy Depends on You");
    }

    public void InstantiateButtons(List<(string, string)> leaderButtonData) {
        this.leaderButtonData = leaderButtonData;
        buttonController.InstantiateLeaderButtons(leaderButtonData);
    }

    //Renders the Main Scene
    public void RenderMainScene(){
        UIAnim.SetTrigger("Back");
        UpdateMainScreen();
        cameraController.StartMapFly();
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
    public void RenderPlanetInfo(Planet clickedPlanet, List<(Leader, float)> influenceRatios, GameObject focusTarget) {
        planetName.text = clickedPlanet.Name;
        planetInfo.text = clickedPlanet.Name + " is owned by " + clickedPlanet.CurrentLeader.Name;

        planetPoliticsYield.text = clickedPlanet.PoliticsYield.ToString();
        planetIntelligenceYield.text = clickedPlanet.IntellectYield.ToString();
        planetWealthYield.text = clickedPlanet.AffluenceYield.ToString();
        pieChart.LoadPieChart(influenceRatios);
        Debug.Log(clickedPlanet.Name);
        planetImage.sprite = FileManager.GetPlanetImageFromFileName(clickedPlanet.Name);
        campaign.planet = clickedPlanet;
        
        if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Nebula")) {
            UIAnim.SetTrigger("ToPlanet");
            cameraController.StartFly(focusTarget);
        }
        
    }

    //Renders the LeaderInfo Screen Relative to a given Leader
    public void RenderLeaderInfo(Leader leader){
        
        currentLeader = leader; 
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
       
       leaderImage.sprite = FileManager.GetLeaderImageFromFileName(leader.GetLeaderImagePath(LeaderResources.Perspectives.Full, LeaderResources.Expressions.Neutral));
       ClearConverseLog();

       //check if coming from Nebula
       if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Nebula")) {
            UIAnim.SetTrigger("ToLeader");
       }
    } 

    private void ClearConverseLog() {
        for(int i = converseData.childCount; i > 0; i--) {
            GameObject.Destroy(converseData.GetChild(i - 1).gameObject);
        }
    }

    public void UpdateLog(IEnumerable<GameEvent> gameEvents) {
        Debug.Log(gameEvents);
        for(int i = LogData.childCount; i > 0; i--) {
            GameObject.Destroy(LogData.GetChild(i - 1).gameObject);
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
        if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Planet") || UIAnim.GetCurrentAnimatorStateInfo(0).IsName("NebulaToPlanet")) {
            cameraController.StartMapFly();
        }
        UIAnim.SetTrigger("Back");
        UpdateMainScreen();
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

    public void AddToConverse(string converseInfo) {
        var ConverseText = Instantiate(ConversePrefab, converseData);
        ConverseText.text = converseInfo;
    }

    
}
