using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.UI;
using System;

// I'm genuinely really proud of this code, it's super elegant and required very little brute force
public class TradeUIController : MonoBehaviour
{
    [Header("Player")]
    // 1
    [SerializeField] TextMeshProUGUI playerPolitics;
    // 2
    [SerializeField] TextMeshProUGUI playerInfluence;
    // 3
    [SerializeField] TextMeshProUGUI playerWealth;
    [SerializeField] Image playerIcon;

    [Header("Leader")]
    // 4
    [SerializeField] TextMeshProUGUI leaderPolitics;
    // 5
    [SerializeField] TextMeshProUGUI leaderInfluence;
    // 6
    [SerializeField] TextMeshProUGUI leaderWealth;
    [SerializeField] Image leaderIcon;
    Leader enemyLeader;

    public delegate void TradeHandler(int[] vals, Leader enemyLeader);
    public static event TradeHandler? TradeConfirmPressed;

    int[] vals;
    int[] maxes;

    //Called when Trade Begins, fills out the trade menu
    public void BeginTrade(Leader playerLeader, Leader enemyLeader, List<(string, string)> leaderButtonData ) {
        vals  =  new int[]{0, 0, 0, 0, 0, 0};
        maxes = new int[]{playerLeader.PoliticsStockpile, playerLeader.IntelligenceStockpile, playerLeader.AffluenceStockpile - 2, 
            enemyLeader.PoliticsStockpile, enemyLeader.IntelligenceStockpile, enemyLeader.AffluenceStockpile};
        this.enemyLeader = enemyLeader;
        //foreach((string, string) leader in leaderButtonData) {
            // if(leader.Item1.Equals(playerLeader.Name)) {
            playerIcon.sprite = FileManager.GetLeaderImageFromFileName(playerLeader.GetLeaderImagePath(LeaderResources.Perspectives.Portrait, LeaderResources.Expressions.Neutral));
            // }
            // if(leader.Item1.Equals(enemyLeader.Name)) {
            leaderIcon.sprite = FileManager.GetLeaderImageFromFileName(enemyLeader.GetLeaderImagePath(LeaderResources.Perspectives.Portrait, LeaderResources.Expressions.Neutral));
            // }
        // }
        UpdateText();
    }

    public void ChangeValue(int buttonIndex) {
        if(buttonIndex > 0) {
            buttonIndex--;
            if(++vals[buttonIndex] > maxes[buttonIndex])
                vals[buttonIndex] = 0;
        }
        else {
            buttonIndex = Math.Abs(buttonIndex) - 1;
            if(--vals[buttonIndex] < 0)
                vals[buttonIndex] = maxes[buttonIndex];
        }
        UpdateText();        

    }

    private void UpdateText() {
        playerPolitics.text = vals[0].ToString();
        playerInfluence.text = vals[1].ToString();
        playerWealth.text = vals[2].ToString();
        leaderPolitics.text = vals[3].ToString();
        leaderInfluence.text = vals[4].ToString();
        leaderWealth.text = vals[5].ToString();
    }

    public void Confirm() {
        TradeConfirmPressed.Invoke(vals, enemyLeader);
        Reset();
    }

    public void Reset() {
        playerPolitics.text = 0.ToString();
        playerInfluence.text = 0.ToString();
        playerWealth.text = 0.ToString();
        leaderPolitics.text = 0.ToString();
        leaderInfluence.text = 0.ToString();
        leaderWealth.text = 0.ToString();

        FindObjectOfType<UIController>().UIAnim.SetTrigger("Back");
    }
}
