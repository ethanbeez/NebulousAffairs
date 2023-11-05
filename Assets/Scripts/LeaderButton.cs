#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderButton {
    public const float PortraitScale = 0.2f;

    public delegate void LeaderButtonHandler(int index);
    public static event LeaderButtonHandler? LeaderButtonPressed;
    // public GameObject buttonObject;
    private GameObject leaderButtonGameObject;
    public string LeaderName { get; private set; }
    public string ImagePath { get; private set; }
    private Image leaderImage;
    private Button button;

    public LeaderButton(string leaderName, string imagePath, GameObject leaderButtonGameObject) {
        this.leaderButtonGameObject = leaderButtonGameObject;
        // button = new();
        LeaderName = leaderName;
        ImagePath = imagePath;
        // leaderImage = new();
        // button.clicked += LeaderClicked;
    }

    public static void LeaderClicked() { 
        
    }
}
