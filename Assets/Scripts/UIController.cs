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
public class UIController : MonoBehaviour {

    GameManager gameManager;
    InputManager inputManager;
    ButtonController buttonController;
    [SerializeField] GameObject leaderButtonPrefab;

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
    [SerializeField] TextMeshProUGUI planetPoliticsPriority;
    [SerializeField] TextMeshProUGUI planetWealthPriority;
    [SerializeField] TextMeshProUGUI planetIntelligencePriority;
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
        AddToLog("Good Luck - The Galaxy Depends on You");
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
        planetIntelligenceYield.text = clickedPlanet.IntellectYield.ToString();
        planetWealthYield.text = clickedPlanet.AffluenceYield.ToString();
        pieChart.LoadPieChart(influenceRatios);
        Debug.Log(clickedPlanet.Name);
        planetImage.sprite = FileManager.GetPlanetImageFromFileName(clickedPlanet.Name);

        campaign.planet = clickedPlanet;
        string clickedPlanetName = clickedPlanet.Name;
        string planetFlavorText = "";
        // TODO: Please remove this or I will kill myself
        switch (clickedPlanetName) {
            case "Rhollian Remnants":
                planetFlavorText = "Long ago, an asteroid collided with the planet Rhollia. Both Rhollia and the asteroid broke apart, creating the barrier between the Far Empire and the rest of the Ckepbo Galaxy. Over the years, the Far Empire has slowly rebuilt their civilization from the rubble of their ruined planet.";
                break;
            case "Pykien":
                planetFlavorText = "Blocked from the rest of the galaxy, the Far Empire's wealth has amassed unspent in the vaults of the mountainous Pykien. Castles carved directly into the rocky surface lead down to subterranean cities. Volcanoes and deep oceans cover the majority of the planet. The rough terrain left temperate Rhollia the more suitable settlement for the Far Empire's governing body and upper class, though most surviving citizens emigrated to Pykien after Rhollia's destruction.";
                break;
            case "Havis-4":
                planetFlavorText = "Havis-4 is a tropical planet best known for its rare food exports. Among them is the highly sought-after Nyndas Roe, a delicacy for the elite of the Ckepbo Galaxy. The population of fishermen and tourists are centralized along the equator. The northern and southern hemispheres are uninhabitable and largely unexplored. Those who venture too far from the equator inexplicably disappear…";
                break;
            case "Sevshaa":
                planetFlavorText = "Sevshaa is a pirate's haven. The most valuable currency here is information—peddled by politicians' aides and interplanetary merchants looking for a quick buck. If you're hunting for dirt on your least favorite Emperor, this is the place to be. Though perhaps not for very long…the swampy surface of the planet is covered in thick mud, dangerous megafauna, and poisonous flora.";
                break;
            case "Pa'an":
                planetFlavorText = "Pa'an is home to the prestigious Zeridian Academy, a celebrated hub of academics and theologians alike. Professional Loreites must graduate from this university in order to serve in the Church. If you're not interested in Lore, the Academy offers many other subjects, ranging from Theoretic Campthran Bison Biology to Experimental H'eshol Ballet.";
                break;
            case "Lore's Hope":
                planetFlavorText = "Lore's Hope is the central hub of the Church of Lore. The gilded Grand Cathedral glitters as a beacon of hope for the faithful of Lore flocking to pay their required tithes. It is a solemn and quiet planet, as members of the Church who have yet to pay their tithe for the year must follow a vow of silence.";
                break;
            case "Tau Haren":
                planetFlavorText = "Tau Haren is the Ckepbo Galaxy's political hub. It is home to the Capitol Building, the central meeting location for the Emperors and other influential political leaders. Tau Haren's cities are said to be the most technologically advanced in the galaxy. However, most people live off-world and commute to Tau Haren for work. The living creatures that do stay on the planet full time are the native Worms, a race of psychic earthworms. The Worms float through the atmosphere, living off microscopic organisms.";
                break;
            case "Sotaldi":
                planetFlavorText = "Sotaldi is the manufacturing hub of the Ckepbo Galaxy. Here, spaceships of various sizes and utilities are constructed in its many factories. The planet combats pollution via massive pumps that transport smog from the atmosphere into orbit. The skies, as a result, are permanently gray and overcast.";
                break;
            case "Xyda-6":
                planetFlavorText = "Xyda-6 is a lush planet and model of environmental sustainability. Its citizens live in harmony with the indigenous flora and fauna of its jungle habitat. The people of Xyda-6 have a reputation for being friendly and kind, especially to tourists or travelers. There is always a place at the synthetic fireplace for a weary intergalactic explorer.";
                break;
            case "Jocania":
                planetFlavorText = "Jocania is one planet-sized museum. Artifacts, statues, art, and other historical items of ages past stand on display for visitors to endlessly browse. It is a favorite spot for politicians to meet and discuss matters of great secrecy and import. There are no cameras, no microphones, no technology of any sort on the planet. Spaceships must be parked far above the surface in the stratosphere.";
                break;
            case "Tacto":
                planetFlavorText = "Tacto is a frozen planet—frozen all the way to its core, in fact. The planet relies on imports from other civilizations to maintain its large population. The capital city, Phaoria, is a popular marketplace and vacation destination for visitors from all corners of the galaxy. Perhaps the most beautiful attraction on Tacto are the Golvyric Spires: a forest of strange spiral obelisks visible from Phaoria's observation points. No one knows where the spires come from or whether they are natural or man-made.";
                break;
            case "Gamma Vertera":
                planetFlavorText = "The desert planet of Gamma Vertera appears barren on its surface. However, despite its harsh environment, its biodiversity is the greatest in the galaxy. Tourists and locals gather in oases while most of the wildlife roam the vast sand dunes. In the scenic oases, politicians and business officials lounge and confer casually in comfort and luxury.";
                break;
        }
        planetDescription.text = planetFlavorText;

        UIAnim.SetTrigger("ToPlanet");
    }

    //Should be combined with the previous one with an if statement for debug
    public void UpdatePlanetInfo(Planet clickedPlanet, List<(Leader, float)> influenceRatios) {
        planetName.text = clickedPlanet.Name;
        planetInfo.text = clickedPlanet.Name + " is owned by " + clickedPlanet.CurrentLeader.Name;

        //Espionage Needs Implementation for these
        planetIntelligencePriority.text = clickedPlanet.IntelligencePriority.ToString();
        planetPoliticsPriority.text = clickedPlanet.PoliticsPriority.ToString();
        planetWealthPriority.text = clickedPlanet.AffluencePriority.ToString();
        planetPoliticsYield.text = clickedPlanet.PoliticsYield.ToString();
        planetIntelligenceYield.text = clickedPlanet.IntellectYield.ToString();
        planetWealthYield.text = clickedPlanet.AffluenceYield.ToString();
        pieChart.LoadPieChart(influenceRatios);
        string clickedPlanetName = clickedPlanet.Name;
        string planetFlavorText = "";
        // TODO: Please remove this or I will kill myself
        switch (clickedPlanetName) {
            case "Rhollian Remnants":
                planetFlavorText = "Long ago, an asteroid collided with the planet Rhollia. Both Rhollia and the asteroid broke apart, creating the barrier between the Far Empire and the rest of the Ckepbo Galaxy. Over the years, the Far Empire has slowly rebuilt their civilization from the rubble of their ruined planet.";
                break;
            case "Pykien":
                planetFlavorText = "Blocked from the rest of the galaxy, the Far Empire's wealth has amassed unspent in the vaults of the mountainous Pykien. Castles carved directly into the rocky surface lead down to subterranean cities. Volcanoes and deep oceans cover the majority of the planet. The rough terrain left temperate Rhollia the more suitable settlement for the Far Empire's governing body and upper class, though most surviving citizens emigrated to Pykien after Rhollia's destruction.";
                break;
            case "Havis-4":
                planetFlavorText = "Havis-4 is a tropical planet best known for its rare food exports. Among them is the highly sought-after Nyndas Roe, a delicacy for the elite of the Ckepbo Galaxy. The population of fishermen and tourists are centralized along the equator. The northern and southern hemispheres are uninhabitable and largely unexplored. Those who venture too far from the equator inexplicably disappear…";
                break;
            case "Sevshaa":
                planetFlavorText = "Sevshaa is a pirate's haven. The most valuable currency here is information—peddled by politicians' aides and interplanetary merchants looking for a quick buck. If you're hunting for dirt on your least favorite Emperor, this is the place to be. Though perhaps not for very long…the swampy surface of the planet is covered in thick mud, dangerous megafauna, and poisonous flora.";
                break;
            case "Pa'an":
                planetFlavorText = "Pa'an is home to the prestigious Zeridian Academy, a celebrated hub of academics and theologians alike. Professional Loreites must graduate from this university in order to serve in the Church. If you're not interested in Lore, the Academy offers many other subjects, ranging from Theoretic Campthran Bison Biology to Experimental H'eshol Ballet.";
                break;
            case "Lore's Hope":
                planetFlavorText = "Lore's Hope is the central hub of the Church of Lore. The gilded Grand Cathedral glitters as a beacon of hope for the faithful of Lore flocking to pay their required tithes. It is a solemn and quiet planet, as members of the Church who have yet to pay their tithe for the year must follow a vow of silence.";
                break;
            case "Tau Haren":
                planetFlavorText = "Tau Haren is the Ckepbo Galaxy's political hub. It is home to the Capitol Building, the central meeting location for the Emperors and other influential political leaders. Tau Haren's cities are said to be the most technologically advanced in the galaxy. However, most people live off-world and commute to Tau Haren for work. The living creatures that do stay on the planet full time are the native Worms, a race of psychic earthworms. The Worms float through the atmosphere, living off microscopic organisms.";
                break;
            case "Sotaldi":
                planetFlavorText = "Sotaldi is the manufacturing hub of the Ckepbo Galaxy. Here, spaceships of various sizes and utilities are constructed in its many factories. The planet combats pollution via massive pumps that transport smog from the atmosphere into orbit. The skies, as a result, are permanently gray and overcast.";
                break;
            case "Xyda-6":
                planetFlavorText = "Xyda-6 is a lush planet and model of environmental sustainability. Its citizens live in harmony with the indigenous flora and fauna of its jungle habitat. The people of Xyda-6 have a reputation for being friendly and kind, especially to tourists or travelers. There is always a place at the synthetic fireplace for a weary intergalactic explorer.";
                break;
            case "Jocania":
                planetFlavorText = "Jocania is one planet-sized museum. Artifacts, statues, art, and other historical items of ages past stand on display for visitors to endlessly browse. It is a favorite spot for politicians to meet and discuss matters of great secrecy and import. There are no cameras, no microphones, no technology of any sort on the planet. Spaceships must be parked far above the surface in the stratosphere.";
                break;
            case "Tacto":
                planetFlavorText = "Tacto is a frozen planet—frozen all the way to its core, in fact. The planet relies on imports from other civilizations to maintain its large population. The capital city, Phaoria, is a popular marketplace and vacation destination for visitors from all corners of the galaxy. Perhaps the most beautiful attraction on Tacto are the Golvyric Spires: a forest of strange spiral obelisks visible from Phaoria's observation points. No one knows where the spires come from or whether they are natural or man-made.";
                break;
            case "Gamma Vertera":
                planetFlavorText = "The desert planet of Gamma Vertera appears barren on its surface. However, despite its harsh environment, its biodiversity is the greatest in the galaxy. Tourists and locals gather in oases while most of the wildlife roam the vast sand dunes. In the scenic oases, politicians and business officials lounge and confer casually in comfort and luxury.";
                break;
        }
        planetDescription.text = planetFlavorText;
        campaign.planet = clickedPlanet;
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
        if(UIAnim.GetCurrentAnimatorStateInfo(0).IsName("Planet")) {
            inputManager.CameraToMapPosition();
        }
        for(int i = converseData.childCount; i > 0; i--) {
            GameObject.Destroy(converseData.GetChild(i - 1).gameObject);
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

    public void AddToConverse(string converseInfo) {
        var ConverseText = Instantiate(ConversePrefab, converseData);
        ConverseText.text = converseInfo;
    }

    
}
