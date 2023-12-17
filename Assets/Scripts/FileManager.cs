using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class FileManager {
    private static readonly Regex whitespaceTrim = new(@"\s+");

    private const string MissingValueQualifier = "NOT_DEFINED";
    private static int MissingLeaderNameInt = 1;
    private static int MissingPlanetNameInt = 1;
    private const string GameDataFileExtension = ".nebfair";
    private const string GameDataFolder = "/GameData";
    private const string LeaderImageExtension = ".png";
    private const string GameplayConstantsFolder = "/GameplayConstants";
    private const string LeadersResourceFolder = "/Resources/Leaders";
    private const string LeaderGameplayDataFile = "/leaders";
    private const string PlanetGameplayDataFile = "/planets";
    private const string EventDialogueFile = "/EventDialogue";
    private const string QuestionDialogueFile = "/QuestionDialogue";
    private static readonly List<string> GameplayConstantsFiles = new(){ LeaderGameplayDataFile, PlanetGameplayDataFile };
  
    public static List<string> GetMissingGameDataFiles() {
        string gameplayConstantsPath = Path.Join(Application.streamingAssetsPath, GameDataFolder, GameplayConstantsFolder);
        List<string> missingFiles = new();
        foreach (string gameplayConstantFile in GameplayConstantsFiles) {
            string filePath = Path.Join(gameplayConstantsPath, gameplayConstantFile) + GameDataFileExtension;
            if (!File.Exists(filePath)) {
                missingFiles.Add(filePath);
            }
        }
        return missingFiles;
    }

    public static LeaderDialogue GetLeaderDialogue(Leader leader) {
        string leaderDialogueFile = Path.Join(Application.streamingAssetsPath, LeadersResourceFolder, EventDialogueFile) + GameDataFileExtension; // Full file path
        // Debug.Log(leaderDialogueFile);
        LeaderDialogue leaderDialogue = new(leader);
        try {
            StreamReader sr = new(leaderDialogueFile);
            string line = sr.ReadLine();

            while (line != null) {
                Regex leaderPattern = new(@"origin_leader: '(?<originLeader>\w+( \w+)*)' \.event_type: '(?<eventType>\w+( \w+)*.?)' \.interaction_leader: '(?<interactionLeader>\w+( \w+)*)' \.min_relationship: (?<minRelationship>\-?\d?.?\d*) \.max_relationship: (?<maxRelationship>\-?\d?.?\d*) \.dialogue: '(?<dialogue>.*)'");
                Match match = leaderPattern.Match(line);
                DialogueContextType dialogueContextType = GetDialogueContextType(match.Groups["eventType"].Value);
                string originLeader = match.Groups["originLeader"].Value;
                if (originLeader != leader.Name) {
                    // TODO: Please do something other than this.
                    line = sr.ReadLine();
                    continue;
                }
                string interactionLeader = match.Groups["interactionLeader"].Value;
                float minRelationship = float.Parse(match.Groups["minRelationship"].Value);
                float maxRelationship = float.Parse(match.Groups["maxRelationship"].Value);
                string dialogue = match.Groups["dialogue"].Value;
                LeaderDialogue.DialogueCondition dialogueCondition = new(minRelationship, maxRelationship, interactionLeader);
                leaderDialogue.AddContextEdge(dialogueContextType, dialogue, dialogueCondition);
                line = sr.ReadLine();
            }
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }

        // QUESTION DIALOGUE

        string questionDialogueFile = Path.Join(Application.streamingAssetsPath, LeadersResourceFolder, QuestionDialogueFile) + GameDataFileExtension; // Full file path
        // Debug.Log(leaderDialogueFile);
        try {
            StreamReader sr = new(questionDialogueFile);
            string line = sr.ReadLine();

            while (line != null) {
                Regex leaderPattern = new(@"origin_leader: '(?<originLeader>\w+( \w+)*)' \.min_relationship: (?<minRelationship>\-?\d?.?\d*) \.max_relationship: (?<maxRelationship>\-?\d?.?\d*) \.question: '(?<question>.*)' \.response: '(?<response>.*)'");
                Match match = leaderPattern.Match(line);
                DialogueContextType dialogueContextType = DialogueContextType.PersonalQuestion;
                string originLeader = match.Groups["originLeader"].Value;
                if (originLeader != leader.Name) {
                    // TODO: Please do something other than this.
                    line = sr.ReadLine();
                    continue;
                }
                float minRelationship = float.Parse(match.Groups["minRelationship"].Value);
                float maxRelationship = float.Parse(match.Groups["maxRelationship"].Value);
                string question = match.Groups["question"].Value;
                string response = match.Groups["response"].Value;
                LeaderDialogue.DialogueCondition questionCondition = new(minRelationship, maxRelationship, leader.Name);
                leaderDialogue.AddContextEdge(dialogueContextType, question, questionCondition);
                LeaderDialogue.DialogueCondition dialogueCondition = new(0, 1, leader.Name);
                leaderDialogue.AddDialogueEdge(question, response, dialogueCondition);
                line = sr.ReadLine();
            }
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }

        return leaderDialogue;
    }

    public static DialogueContextType GetDialogueContextType(string eventType) {
        switch (eventType) {
            case "Receive Trade":
                return DialogueContextType.ReceiveTrade;
            case "Accept Received Trade":
                return DialogueContextType.AcceptReceivedTrade;
            case "Refuse Received Trade":
                return DialogueContextType.RefuseReceivedTrade;
            case "Send Trade":
                return DialogueContextType.SendTrade;
            case "Sent Trade Accepted":
                return DialogueContextType.SentTradeAccepted;
            case "Sent Trade Refused":
                return DialogueContextType.SendTradeRejected;
            case "Leader Eliminated":
                return DialogueContextType.LeaderEliminated;
            case "Stolen Planet":
                return DialogueContextType.StolenPlanet;
            case "Personal Question":
                return DialogueContextType.PersonalQuestion;
        }
        Debug.LogError("FileManager.GetDialogueContextType: Parse failed!");
        return DialogueContextType.Default;
    }

    public static Dictionary<string, Dictionary<CurrencyType, int>> GetLeaderPreferences() { 
        string leaderGameplayConstantsFile = Path.Join(Application.streamingAssetsPath, GameDataFolder, GameplayConstantsFolder); // Full folder path
        leaderGameplayConstantsFile = Path.Join(leaderGameplayConstantsFile, LeaderGameplayDataFile) + GameDataFileExtension; // Appending file and extension
        Dictionary<string, Dictionary<CurrencyType, int>> leaderPreferences = new();
        try {
            StreamReader sr = new(leaderGameplayConstantsFile);
            string line = sr.ReadLine();
            
            while (line != null) {
                Debug.Log(line);
                Regex leaderPattern = new(@"leader_name: '(?<leaderName>\w+( \w+)*)' \.politics_preference: (?<politicsPreference>\-?\d) \.money_preference: (?<moneyPreference>\-?\d+) \.intellect_preference: (?<intellectPreference>\-?\d)");
                Match match = leaderPattern.Match(line);
                string leaderName = match.Groups["leaderName"].Value;
                Debug.Log(leaderName);
                if (leaderName == MissingValueQualifier) {
                    Debug.LogError($"FileManager.GetLeaderPreferences: A leader was missing a correctly formatted name! Provided name: {leaderName}, defaulted name: Leader {MissingLeaderNameInt}");
                    leaderName = "Leader " + MissingLeaderNameInt++;
                }
                int politicsPreference = 0;
                if (int.TryParse(match.Groups["politicsPreference"].Value, out int poliPref)) {
                    politicsPreference = poliPref;
                } else {
                    Debug.LogError($"FileManager.GetLeaderPreferences: {leaderName} was missing a correctly formatted politics preference value! Provided value: {match.Groups["politicsPreference"].Value}");
                }
                int moneyPreference = 0;
                if (int.TryParse(match.Groups["moneyPreference"].Value, out int moneyPref)) {
                    moneyPreference = moneyPref;
                } else {
                    Debug.LogError($"FileManager.GetLeaderPreferences: {leaderName} was missing a correctly formatted money preference value! Provided value: {match.Groups["moneyPreference"].Value}");
                }
                int intellectPreference = 0;
                if (int.TryParse(match.Groups["intellectPreference"].Value, out int intellectPref)) {
                    intellectPreference = intellectPref;
                } else {
                    Debug.LogError($"FileManager.GetLeaderPreferences: {leaderName} was missing a correctly formatted intellect preference value! Provided value: {match.Groups["intellectPreference"].Value}");
                }
                leaderPreferences.Add(leaderName, new Dictionary<CurrencyType, int>()
                { 
                    {CurrencyType.Politics, politicsPreference },
                    {CurrencyType.Affluence, moneyPreference },
                    {CurrencyType.Intellect, intellectPreference }
                });
                line = sr.ReadLine();
            }
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
        return leaderPreferences;
    }

    public static Dictionary<string, Dictionary<CurrencyType, int>> GetPlanetPreferences() {
        string planetGameplayConstantsFile = Path.Join(Application.streamingAssetsPath, GameDataFolder, GameplayConstantsFolder); // Full folder path
        planetGameplayConstantsFile = Path.Join(planetGameplayConstantsFile, PlanetGameplayDataFile) + GameDataFileExtension; // Appending file and extension
        Dictionary<string, Dictionary<CurrencyType, int>> planetPreferences = new();
        try {
            StreamReader sr = new(planetGameplayConstantsFile);
            string line = sr.ReadLine();

            while (line != null) {
                Debug.Log(line);
                Regex leaderPattern = new(@"planet_name: '(?<planetName>[\w'-]+[ \w'-]*)' \.initial_leader: '(?<initialLeader>\w+( \w+)*)' \.politics_preference: (?<politicsPreference>\-?\d) \.money_preference: (?<moneyPreference>\-?\d+) \.intellect_preference: (?<intellectPreference>\-?\d)");
                Match match = leaderPattern.Match(line);
                string planetName = match.Groups["planetName"].Value;
                Debug.Log(planetName);
                if (planetName == MissingValueQualifier) {
                    Debug.LogError($"FileManager.GetPlanetPreferences: A planet was missing a correctly formatted name! Provided name: {planetName}, defaulted name: Planet {MissingPlanetNameInt}");
                    planetName = "Planet " + MissingPlanetNameInt++;
                }
                int politicsPreference = 0;
                if (int.TryParse(match.Groups["politicsPreference"].Value, out int poliPref)) {
                    politicsPreference = poliPref;
                } else {
                    Debug.LogError($"FileManager.GetPlanetPreferences: {planetName} was missing a correctly formatted politics preference value! Provided value: {match.Groups["politicsPreference"].Value}");
                }
                int moneyPreference = 0;
                if (int.TryParse(match.Groups["moneyPreference"].Value, out int moneyPref)) {
                    moneyPreference = moneyPref;
                } else {
                    Debug.LogError($"FileManager.GetPlanetPreferences: {planetName} was missing a correctly formatted money preference value! Provided value: {match.Groups["moneyPreference"].Value}");
                }
                int intellectPreference = 0;
                if (int.TryParse(match.Groups["intellectPreference"].Value, out int intellectPref)) {
                    intellectPreference = intellectPref;
                } else {
                    Debug.LogError($"FileManager.GetPlanetPreferences: {planetName} was missing a correctly formatted intellect preference value! Provided value: {match.Groups["intellectPreference"].Value}");
                }
                planetPreferences.Add(planetName, new Dictionary<CurrencyType, int>()
                {
                    {CurrencyType.Politics, politicsPreference },
                    {CurrencyType.Affluence, moneyPreference },
                    {CurrencyType.Intellect, intellectPreference }
                });
                line = sr.ReadLine();
            }
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
        return planetPreferences;
    }

    public static Dictionary<string, string> GetInitialPlanetLeaders() {
        string planetGameplayConstantsFile = Path.Join(Application.streamingAssetsPath, GameDataFolder, GameplayConstantsFolder); // Full folder path
        planetGameplayConstantsFile = Path.Join(planetGameplayConstantsFile, PlanetGameplayDataFile) + GameDataFileExtension; // Appending file and extension
        Dictionary<string, string> initialPlanetLeaders = new();
        try {
            StreamReader sr = new(planetGameplayConstantsFile);
            string line = sr.ReadLine();

            while (line != null) {
                Debug.Log(line);
                Regex leaderPattern = new(@"planet_name: '(?<planetName>[\w'-]+[ \w'-]*)' \.initial_leader: '(?<initialLeader>\w+( \w+)*)' \.politics_preference: (?<politicsPreference>\-?\d) \.money_preference: (?<moneyPreference>\-?\d+) \.intellect_preference: (?<intellectPreference>\-?\d)");
                Match match = leaderPattern.Match(line);
                string planetName = match.Groups["planetName"].Value;
                if (planetName == MissingValueQualifier) {
                    Debug.LogError($"FileManager.GetInitialPlanetLeaders: A planet was missing a correctly formatted name! Provided name: {planetName}, defaulted name: Planet {MissingPlanetNameInt}");
                    planetName = "Planet " + MissingPlanetNameInt++;
                }
                string initialLeader = match.Groups["initialLeader"].Value;
                if (initialLeader == MissingValueQualifier) {
                    Debug.LogError($"FileManager.GetInitialPlanetLeaders: An initial leader was missing a correctly formatted name! Cannot assign planet to leader; closing game...");
                    Application.Quit();
                }
                initialPlanetLeaders.Add(planetName, initialLeader);
                line = sr.ReadLine();
            }
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
        return initialPlanetLeaders;
    }

    public static Sprite GetLeaderImageFromFileName(string name) {
        string leaderImageFilePath = "LeaderImages/" +  whitespaceTrim.Replace(name, "");
        if (Resources.Load<Sprite>(leaderImageFilePath) == null) {
            Debug.Log("):");
        }
        return Resources.Load<Sprite>(leaderImageFilePath);
    }

    public static Dictionary<LeaderResources.Perspectives, Dictionary<LeaderResources.Expressions, string>> GetLeaderImagePaths(string leaderName) {
        Dictionary<LeaderResources.Perspectives, Dictionary<LeaderResources.Expressions, string>> leaderImagePaths = new();
        foreach (LeaderResources.Perspectives perspective in Enum.GetValues(typeof(LeaderResources.Perspectives))) {
            Dictionary<LeaderResources.Expressions, string> expressions = new();
            foreach (LeaderResources.Expressions expression in Enum.GetValues(typeof(LeaderResources.Expressions))) {
                expressions.Add(expression, $"{leaderName}_{perspective.ToString().ToLower()}_{expression.ToString().ToLower()}");
            }
            leaderImagePaths.Add(perspective, expressions);
        }
        return leaderImagePaths;
    }
    
    public static Sprite GetPlanetImageFromFileName(string name) {
        name = name.Replace("-" , "").Replace("'", "");
        string planetImageFilePath = "PlanetImages/" +  whitespaceTrim.Replace(name, "");
        Debug.Log(planetImageFilePath);
        if (Resources.Load<Sprite>(planetImageFilePath) == null) {
            Debug.Log("):");
        }
        return Resources.Load<Sprite>(planetImageFilePath);
    }
}
