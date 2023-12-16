#nullable enable

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LeaderButton {
    public const float PortraitScale = 0.1f;

    public delegate void LeaderButtonHandler(string leaderName);
    public static event LeaderButtonHandler? LeaderButtonPressed;
    public delegate void LeaderButtonHover(string leaderName, bool isEnter);
    public static event LeaderButtonHover? LeaderButtonEnterOrExit;

    private GameObject leaderButtonGameObject;
    private UnityAction buttonPressed;
    public string LeaderName { get; private set; }
    public string ImagePath { get; private set; }
    private Image leaderImage;
    private string leaderName;
    private Image tradeNotif;
    private Image messageNotif;

    public LeaderButton(string leaderName, string imagePath, GameObject leaderButtonGameObject) {
        this.leaderButtonGameObject = leaderButtonGameObject;
        leaderButtonGameObject.AddComponent<Button>();
        LeaderName = leaderName;
        this.leaderName = leaderName;
        ImagePath = imagePath;
        leaderImage = leaderButtonGameObject.GetComponent<Image>();
        leaderImage.sprite = FileManager.GetLeaderImageFromFileName(ImagePath);
        buttonPressed += LeaderClicked;
        leaderButtonGameObject.GetComponent<Button>().onClick.AddListener(LeaderClicked);
        var notifs = leaderButtonGameObject.GetComponentsInChildren<Image>();
        tradeNotif = notifs[0];
        messageNotif = notifs[1];

    }

    public void LeaderClicked() {
        LeaderButtonPressed!.Invoke(leaderName);
    }

    void OnMouseEnter()
    {
        LeaderButtonEnterOrExit.Invoke(leaderName, true);
    }

    void OnMouseExit()
    {
        LeaderButtonEnterOrExit.Invoke(leaderName, false);
    }

    public void ActivateNotif(bool isMessage)
    {
        if(isMessage)
        {
            messageNotif.enabled = true;
        }
        else
        {
            tradeNotif.enabled = true;
        }
    }

    public void DeactivateNotif(bool isMessage)
    {
        if (isMessage)
        {
            messageNotif.enabled = false;
        }
        else
        {
            tradeNotif.enabled = false;
        }
    }
}
