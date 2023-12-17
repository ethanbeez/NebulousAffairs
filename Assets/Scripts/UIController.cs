using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Rendering;
public class UIController : MonoBehaviour {

    ButtonController buttonController;
    [SerializeField] GameObject leaderButtonPrefab;
    CameraController cameraController;
    [SerializeField] DialogueController dialogueController;

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
    [SerializeField] TextMeshProUGUI LeaderName;
    [SerializeField] VerticalLayoutGroup ConverseGroup; 

    [Header("Espionage Components")]
    [SerializeField] Button PoliticsEspionage;
    [SerializeField] Button InfluenceEspionage;
    [SerializeField] Button WealthEspionage;

    [Header("Leader Influence Thresholds")]
    // Think about how to sort these.
    [SerializeField] [Range(0, 100)] int AngryThreshold;
    [SerializeField] [Range(0, 100)] int SadThreshold;
    [SerializeField] [Range(0, 100)] int NeutralThreshold;
    [SerializeField] [Range(0, 100)] int CuriousThreshold;
    [SerializeField] [Range(0, 100)] int HappyThreshold;

    [Header("End Screen Components")]
    [SerializeField] TextMeshProUGUI endText;
    [SerializeField] TextMeshProUGUI scoreText;
    private List<(string, string)> leaderButtonData; 
    Leader currentLeader;
    private int espionageResource;
    private int actionsLeft;
    public delegate void EspionageConfirm(CurrencyType resource, Leader enemyLeader);
    public static event EspionageConfirm? ConfirmEspionage;
    public delegate void QuitGame();
    public static event QuitGame? GameQuit;

    public delegate void StartGame();
    public static event StartGame GameStart;

    public delegate void PauseGame();
    public static event PauseGame GamePaused;

    public delegate void UnpauseGame();
    public static event UnpauseGame GameUnpaused;
    private List<Notification> notifs;
    public Player player;

    
    void OnEnable() {
        buttonController = new(leaderButtonPrefab);
        cameraController = FindObjectOfType<CameraController>();
    }

    public void InstantiateButtons(List<(string, string)> leaderButtonData) {
        this.leaderButtonData = leaderButtonData;
        buttonController.ClearLeaderButtons();
        buttonController.InstantiateLeaderButtons(leaderButtonData);
    }

    //Renders the Main Scene
    public void RenderMainScene(){
        UpdateMainScreen();
        UIAnim.SetTrigger("Back");
        cameraController.StartMapFly();
    }

    //Updates the TurnDisplay to a given String - the expected String is TurnHandler's GameTurns toString.
    public void UpdateTurnDisplay(String TurnInfo, List<Notification> notifications) {

        //Pie Chart should be updated when turn ends. yipee.
        //pieChart.LoadPieChart(influenceRatios);
        notifs = notifications;
        UpdateMainScreen();
        UpdateActionDisplay(3);

    }

    private void UpdateMainScreen() {
        playerIntelligence.text = playerLeader.IntelligenceStockpile.ToString();
        playerWealth.text = playerLeader.AffluenceStockpile.ToString();
        playerPolitics.text = playerLeader.PoliticsStockpile.ToString();


        if(playerLeader.IntelligenceYield >= 0)
            playerIntelligenceYield.text = "+" + playerLeader.IntelligenceYield.ToString();
        else
            playerIntelligenceYield.text = playerLeader.IntelligenceYield.ToString();

        if (playerLeader.AffluenceYield >= 0)
            playerWealthYield.text = "+" + playerLeader.AffluenceYield.ToString();
        else
            playerWealthYield.text = playerLeader.AffluenceYield.ToString();

        if (playerLeader.PoliticsYield >= 0)
            playerPoliticsYield.text = "+" + playerLeader.PoliticsYield.ToString();
        else
            playerPoliticsYield.text = playerLeader.PoliticsYield.ToString();


        buttonController.DeactivateAllNotifs();
        AddNotifs(notifs);


    }

    public void UpdateEliminatedLeaderButton(string leaderName) {
        buttonController.EliminateLeaderButton(leaderName);
    }

    private void AddNotifs(List<Notification> notifications)
    {
        Debug.Log(notifications.Count + " Notifications");
        foreach (Notification notif in notifications)
        {
            buttonController.ActivateNotifs(notif.OriginLeader, notif.Type == NotificationType.Message);
        }
    }

    //Renders the PlanetInfo Screen
    public void RenderPlanetInfo(Planet clickedPlanet, List<(Leader, float)> influenceRatios, GameObject focusTarget) {
        if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Planet") || UIAnim.GetCurrentAnimatorStateInfo(0).IsName("ToPlanet")) {
            return;
        }


        planetName.text = clickedPlanet.Name;
        planetInfo.text = clickedPlanet.Name + " is owned by " + clickedPlanet.CurrentLeader.Name;

        planetPoliticsYield.text = clickedPlanet.PoliticsYield.ToString();
        planetIntelligenceYield.text = clickedPlanet.IntellectYield.ToString();
        planetWealthYield.text = clickedPlanet.AffluenceYield.ToString();
        pieChart.LoadPieChart(influenceRatios);
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

        TooltipSystem.Hide();

       leaderIntelligenceStockpile.text = leader.IntelligenceStockpile.ToString();
       leaderPoliticsStockpile.text = leader.PoliticsStockpile.ToString();
       leaderWealthStockpile.text = leader.AffluenceStockpile.ToString();
       LeaderName.text = leader.Name;
        ClearConverseButtons();
        ClearConverseLog();
        dialogueController.UpdateData(notifs, currentLeader, player);
       

       
       leaderImage.sprite = FileManager.GetLeaderImageFromFileName(leader.GetLeaderImagePath(LeaderResources.Perspectives.Full, LeaderResources.Expressions.Neutral));
        

       //check if coming from Nebula
       if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Nebula")) {
            UIAnim.SetTrigger("ToLeader");
       }
    } 

    //Not Implemented, will get a leader's image or something. idk I wrote this code like a week and a half
    private void GetLeaderImage() {

    }

    private void ClearConverseLog() {
        for(int i = converseData.childCount; i > 0; i--) {
            GameObject.Destroy(converseData.GetChild(i - 1).gameObject);
        }
    }

    public void UpdateLog(IEnumerable<GameEvent> gameEvents) {
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
    public void AddToLog(string LogInfo) {
        var LogText = Instantiate(LogPrefab, LogData);
        LogText.text = LogInfo;
    }

    public void Back() {
        if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Planet") || UIAnim.GetCurrentAnimatorStateInfo(0).IsName("NebulaToPlanet")) {
            cameraController.StartMapFly();
        }
        if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Nebula"))
        {
            Pause();
        }
        if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Pause"))
        {
            UnPause();
        }
        UIAnim.SetTrigger("Back");
        UpdateMainScreen();
    }

    public void Trade() {
        if(actionsLeft == 0)
        {
            AddToLog("Not Enough Actions to Trade");
            return;
        }
        tradeUI.BeginTrade(playerLeader, currentLeader, leaderButtonData);
        UIAnim.SetTrigger("Trade");
    }

    public void UpdateActionDisplay(int PlayerTurnActionsLeft) {
        actionsLeft = PlayerTurnActionsLeft;
        actionCounter.UpdateActionDisplay(PlayerTurnActionsLeft);
    }

    public void StartEspionage() {
        if (actionsLeft == 0)
        {
            AddToLog("Not Enough Actions to Espionage");
            return;
        }
        UIAnim.SetTrigger("Espionage");
        WealthEspionage.interactable = playerLeader.GetLeaderPreferenceVisibility(currentLeader, CurrencyType.Affluence);
        InfluenceEspionage.interactable = playerLeader.GetLeaderPreferenceVisibility(currentLeader, CurrencyType.Intellect);
        PoliticsEspionage.interactable = playerLeader.GetLeaderPreferenceVisibility(currentLeader, CurrencyType.Politics);

        Debug.Log(PoliticsEspionage.enabled);

    }

    public void ChooseEspionage(int resource) {
        espionageResource = resource;
        UIAnim.SetTrigger("ToConfirm");

    }

    public void CompleteEspionage() {

        switch(espionageResource) {
            case 0:
                ConfirmEspionage.Invoke(CurrencyType.Politics, currentLeader);
                break;
            case 1:
                ConfirmEspionage.Invoke(CurrencyType.Intellect, currentLeader);
                break;
            case 2:
                ConfirmEspionage.Invoke(CurrencyType.Politics, currentLeader);
                break;
        }
        UIAnim.SetTrigger("ToLeader");
        WealthEspionage.interactable = false;
        InfluenceEspionage.interactable = false;
        PoliticsEspionage.interactable = false;
    }

    public void Campaign()
    {
        if(actionsLeft == 0)
        {
            AddToLog("Not Enough Actions to Campaign");
            return;
        }
        UIAnim.SetTrigger("Campaign");

    }


    //TODO: ethan - this is currently the way to add to the log on the player screen
    public void AddToConverse(string converseInfo) {
        var ConverseText = Instantiate(ConversePrefab, converseData);
        ConverseText.text = converseInfo;
    }

    //Unsafe way of doing this tbh, should be privatized to a QuitButton Class that can only be applied to the quit button. but alas
    public void Quit()
    {
        GameQuit.Invoke();
    }

    public void playGame()
    {
        cameraController.StartGame();
        GameStart.Invoke();
        //zoom
        UIAnim.SetTrigger("MainMenu");
    }

    public void EndGame(IEnumerable<GameEvent> gameEvents)
    {
        foreach(GameEvent gameEvent in gameEvents)
        {
            String stringy = gameEvent.ToString();
            if (stringy.Contains("won"))
            {
                endText.text = stringy;
                if (stringy.Contains("ended the game"))
                {
                    scoreText.text = stringy;
                }
            }
            else
            {
                endText.text = "You Lose!";
                scoreText.text = "";
            }
        }

        UIAnim.SetTrigger("EndGame");   
    }

    public void AddConverseButton(Button button)
    {
        button.gameObject.transform.SetParent(ConverseGroup.transform);
        button.gameObject.GetComponent<RectTransform>().localScale = Vector3.one;

    }

    public void ClearConverseButtons()
    {
        var buttons = ConverseGroup.GetComponentsInChildren<Button>();
        foreach(Button button in buttons)
        {
            GameObject.Destroy(button.gameObject);
        }
    }

    private void Pause()
    {
        GamePaused.Invoke();
    }

    private void UnPause()
    {
        GameUnpaused.Invoke();
    }
    
}
