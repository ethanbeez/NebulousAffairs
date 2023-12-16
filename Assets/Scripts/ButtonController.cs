#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController {
    GameObject leadersPanelGameObject;
    GameObject leaderButtonPrefab;
    public Dictionary<string, LeaderButton> leaderButtons;
    public delegate void leaderButtonHandler(string LeaderName);
    public static event leaderButtonHandler? LeaderButtonPressed;

    public ButtonController(GameObject leaderButtonPrefab) {
        leadersPanelGameObject = GameObject.Find("LeadersPanel");
        this.leaderButtonPrefab = leaderButtonPrefab;
        leaderButtons = new();
    }

    public void InstantiateLeaderButtons(List<(string, string)> leaderInfo) {
        int order = 0;
        foreach ((string, string) leader in leaderInfo) {
            GameObject leaderButtonGameObject = Object.Instantiate(leaderButtonPrefab);
            leaderButtonGameObject.name = leader.Item1 + "'s Portrait";
            LeaderButton leaderButton = new(leader.Item1, leader.Item2, leaderButtonGameObject);
            Sprite sprite = FileManager.GetLeaderImageFromFileName(leader.Item2);
            leaderButtonGameObject.GetComponent<Image>().sprite = sprite;
            RectTransform rectTransform = leaderButtonGameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector3.zero;
            rectTransform.localScale = new(LeaderButton.PortraitScale, LeaderButton.PortraitScale, LeaderButton.PortraitScale);
            rectTransform.sizeDelta = new(sprite.rect.width, sprite.rect.height);
            rectTransform.position = new(order++ * sprite.rect.width * LeaderButton.PortraitScale, 0, 0);
            leaderButtons.Add(leader.Item1, leaderButton);
            leaderButtonGameObject.transform.SetParent(leadersPanelGameObject.transform, false);
            
        }
        Debug.Log(leaderButtons.Count + "Buttons");
    }

    public void ActivateNotifs(Leader leader, bool isMessage)
    {
        Debug.Log("Message from " + leader.Name + isMessage);
        leaderButtons[leader.Name].ActivateNotif(isMessage);
    }

    public void DeactivateNotifs(Leader leader, bool isMessage)
    {
        leaderButtons[leader.Name].DeactivateNotif(isMessage);
    }

    public void DeactivateAllNotifs()
    {
        foreach(KeyValuePair<string, LeaderButton> keyValuePair in leaderButtons)
        {
            keyValuePair.Value.DeactivateNotif(true);
            keyValuePair.Value.DeactivateNotif(false);
        }

    }
}
