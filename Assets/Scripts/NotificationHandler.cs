using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum NotificationType {
    Default = 0,
    TradeOffer = 1,
    Message = 2
}

public class NotificationHandler {
    private Dictionary<string, List<Notification>> OutstandingNotifications;
    // private List<Notification> OutstandingNotifications;

    public NotificationHandler(List<Leader> leaders) {
        OutstandingNotifications = new();
        foreach (Leader leader in leaders) {
            OutstandingNotifications.Add(leader.Name, new());
        }
    }

    public void ClearNotifications() {
        foreach (List<Notification> notifications in OutstandingNotifications.Values) {
            notifications.Clear();
        }
    }

    public void AddNotification(Notification notification) {
        OutstandingNotifications[notification.OriginLeader.Name].Add(notification);
    }

    public List<Notification> GetCurrentNotifications() {
        List<Notification> list = new();
        foreach (List<Notification> notifs in OutstandingNotifications.Values) { 
            foreach (Notification notification in notifs) {
                list.Add(notification);
            }
        }
        return list;
    }

    public void RemoveLeaderNotifications(string leaderName) {
        OutstandingNotifications[leaderName].Clear();
    }
}

public class Notification {
    public Leader OriginLeader;
    public NotificationType Type;
    public int TurnRaised;
    public string NotificationMessage; // Packed into Notification class to avoid need for specific context with which it was raised.
    public Notification(Leader originLeader, NotificationType type, int turnRaised, string notificationMessage) {
        OriginLeader = originLeader;
        TurnRaised = turnRaised;
        Type = type;
        NotificationMessage = notificationMessage;
    }
}
