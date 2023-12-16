using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum NotificationType {
    Default = 0,
    TradeOffer = 1,
    Message = 2
}

public class NotificationHandler {
    private List<Notification> OutstandingNotifications;

    public NotificationHandler() {
        OutstandingNotifications = new List<Notification>();
    }

    public void ClearNotifications() {
        OutstandingNotifications.Clear();
    }

    public void AddNotification(Notification notification) {
        OutstandingNotifications.Add(notification);
    }

    public List<Notification> GetCurrentNotifications() {
        return OutstandingNotifications;
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
}
