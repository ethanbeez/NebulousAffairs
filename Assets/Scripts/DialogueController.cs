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
                //Chatting
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
        DisplayMessage(notifs[0]);
    }

   private void DisplayTrade(Notification notif)
    {
        uiController.AddToConverse(notif.NotificationMessage);
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

    private void TradeConfirm()
    {
        playerLeader.ProcessOutstandingTrade(currentLeader.Name, true );
        uiController.ClearConverseButtons();
        currentNotifs.Remove(currentNotif);
        GenerateButtons();
    }

    private void TradeDeny()
    {
        playerLeader.ProcessOutstandingTrade(currentLeader.Name, false);
        uiController.ClearConverseButtons();
        currentNotifs.Remove(currentNotif);
        GenerateButtons();
    }

    private void MessageConfirm()
    {
        uiController.ClearConverseButtons();
        currentNotifs.Remove(currentNotif);
        GenerateButtons();
    }




}
