using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class GameHistory {
    public const int LogEventLegnth = 100;
    public const bool LogIncludesTradeActions = true;
    public const bool LogIncludesDiplomacyActions = true;
    public const bool LogIncludesEspionageActions = false;
    public const bool LogIncludesElectionOutcomes = true;
    public LinkedList<GameEvent> EventHistory { get; private set; }
    public GameHistory() {
        EventHistory = new();
    }

    public IEnumerable<GameEvent> GetEventHistory() {
        return EventHistory;
    }

    public IEnumerable<GameEvent> GetEventHistory(int range) {
        return EventHistory.ToList().GetRange(0, range); // TODO: This can be improved a bit.
    }

    public void LogGameEvent(GameEvent gameEvent) {
        EventHistory.AddFirst(gameEvent);
        if (EventHistory.Count > LogEventLegnth) {
            EventHistory.RemoveLast();
        }
    }
}

public class GameEvent { 
    public string EventMessage { get; set; }
    public GameEvent(string eventMessage) {
        EventMessage = eventMessage;
    }

    public override string ToString() {
        return EventMessage;
    }
}
