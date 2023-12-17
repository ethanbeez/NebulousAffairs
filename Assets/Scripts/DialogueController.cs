using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueController : MonoBehaviour
{
    List<Notification> notifs;
    Leader currentLeader;
    Player playerLeader;
    [SerializeField] GameObject DialogueButton;
    private List<Notification> currentNotifs;
    UIController uiController;
    Notification currentNotif;
    public delegate void RequestDialogueQuestions(DialogueController dialogueController, string leaderName);
    public static event RequestDialogueQuestions? DialogueRequest;
    List<LeaderDialogue.DialogueNode> currentDialogues;
    public void OnEnable()
    {
        uiController = FindObjectOfType<UIController>();
    }

    public void UpdateData(List<Notification> notifications, Leader currentLeader, Player playerLeader)
    {
        this.playerLeader = playerLeader;
        notifs = notifications;
        this.currentLeader = currentLeader;
        ReadNotifs();
        GenerateButtons();
    }

    private void ReadNotifs()
    {
        currentNotifs = new();
        foreach(Notification notif in notifs)
        {
            if(notif.OriginLeader.Name.Equals(currentLeader.Name))
            {
                Debug.Log(notif.NotificationMessage);
                currentNotifs.Add(notif);
            }
        }
    }

    private void GenerateButtons()
    {
        Debug.Log(currentNotifs.Count);
        switch (currentNotifs.Count)
        {
            case 0:
                DisplayConversation();
                break;
            case 1:
                SelectButtons(currentNotifs[0]);
                break;
            default:
                FindTrade();
                break;


        }
    }

    private void SelectButtons(Notification notif)
    {
        Debug.Log(notif.Type);
        switch (notif.Type)
        {
            case NotificationType.TradeOffer:
                DisplayTrade(notif);
                break;
            case NotificationType.Message:
                DisplayMessage(notif);
                break;
        }
    }

    private void FindTrade()
    {
        foreach(Notification notif in currentNotifs)
        {
            if(notif.Type == NotificationType.TradeOffer)
            {
                DisplayTrade(notif);
                return;
            }
        }
        DisplayMessage(currentNotifs[0]);
    }

   private void DisplayTrade(Notification notif)
    {
        Debug.Log("DisplayingTrade");
        var tradetext = OfferTradeText(playerLeader.OutstandingTrades[currentLeader.Name]);
        uiController.AddToConverse(notif.NotificationMessage);
        uiController.AddToConverse("I'll offer you " + tradetext[0].Item2 + " " + tradetext[0].Item1 + "in exchange for " + tradetext[1].Item2 + " of your " + tradetext[1].Item1);
        var button = Instantiate(DialogueButton);
        currentNotifs.Remove(notif);
        currentNotif = notif;
        button.GetComponent<Button>().onClick.AddListener(TradeConfirm);
        button.GetComponentInChildren<TextMeshProUGUI>().text = "Yes";
        uiController.AddConverseButton(button.GetComponent<Button>());
        var button2 = Instantiate(DialogueButton);
        button2.GetComponent<Button>().onClick.AddListener(TradeDeny);
        button2.GetComponentInChildren<TextMeshProUGUI>().text = "No";
        uiController.AddConverseButton(button2.GetComponent<Button>());

    }

    private List<(string, int)> OfferTradeText(TradeAction tradeAction)
    {
        var output = new List<(string, int)>();
        if(tradeAction.OfferedAffluence > 0)
        {
            output.Add(("Wealth", tradeAction.OfferedAffluence));
        }
        if (tradeAction.OfferedIntellect > 0)
        {
            output.Add(("Influence", tradeAction.OfferedIntellect));
        }
        if (tradeAction.OfferedPolitics > 0)
        {
            output.Add(("Politics", tradeAction.OfferedPolitics));
        }

        if (tradeAction.RequestedAffluence > 0)
        {
            output.Add(("Wealth", tradeAction.RequestedAffluence));
        }
        if (tradeAction.RequestedIntellect > 0)
        {
            output.Add(("Influence", tradeAction.RequestedIntellect));
        }
        if (tradeAction.RequestedPolitics > 0)
        {
            output.Add(("Politics", tradeAction.RequestedPolitics));
        }

        if(output.Count > 2)
        {
            Debug.LogError("Too Many Trade Variables: " + output.Count);
        }
        return output;
    }

    private void DisplayMessage(Notification notif)
    {
        uiController.AddToConverse(notif.NotificationMessage);
        var button = Instantiate(DialogueButton);
        currentNotifs.Remove(notif);
        currentNotif = notif;
        button.GetComponent<Button>().onClick.AddListener(MessageConfirm);
        button.GetComponentInChildren<TextMeshProUGUI>().text = "OK";
        
        uiController.AddConverseButton(button.GetComponent<Button>());
    }

    private void DisplayConversation()
    {
        DialogueRequest(this, currentLeader.Name);
    }

    public void EngageDialogue(List<LeaderDialogue.DialogueNode> availableDialogues)
    {
        currentDialogues = availableDialogues;
        Debug.Log(availableDialogues.Count);
        foreach(var availableDialogue in availableDialogues)
        {

            var button = Instantiate(DialogueButton);
            button.GetComponent<Button>().onClick.AddListener(() => DialogueConfirm(availableDialogue.Dialogue));
            button.GetComponentInChildren<TextMeshProUGUI>().text = availableDialogue.Dialogue;
            uiController.AddConverseButton(button.GetComponent<Button>());

        }
    }

    private void DialogueConfirm(string dialogueText)
    {
        foreach( var availableDialogue in currentDialogues)
        {
            if(dialogueText.Equals(availableDialogue.Dialogue))
                uiController.AddToConverse(availableDialogue.GetQuestionResponse().Dialogue);
        }
    }

    private void TradeConfirm()
    {
        playerLeader.ProcessOutstandingTrade(currentLeader.Name, true );
        Debug.Log("Trade Processed");
        uiController.ClearConverseButtons();
        currentNotifs.Remove(currentNotif);
        notifs.Remove(currentNotif);
        GenerateButtons();
        uiController.AddToConverse(currentLeader.GetEventDialogueResponse(currentLeader, playerLeader.Leader, currentLeader, DialogueContextType.SentTradeAccepted).Dialogue);
    }

    private void TradeDeny()
    {
        playerLeader.ProcessOutstandingTrade(currentLeader.Name, false);
        uiController.ClearConverseButtons();
        currentNotifs.Remove(currentNotif);
        notifs.Remove(currentNotif);
        GenerateButtons();
        uiController.AddToConverse(currentLeader.GetEventDialogueResponse(currentLeader, playerLeader.Leader, currentLeader, DialogueContextType.SendTradeRejected).Dialogue);

    }

    private void MessageConfirm()
    {
        uiController.ClearConverseButtons();
        currentNotifs.Remove(currentNotif);
        notifs.Remove(currentNotif);
        GenerateButtons();
    }




}
