using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Campaign : MonoBehaviour
{
    public delegate void CampaignConfirm(CurrencyType increased, CurrencyType decreased, Planet planet);
    public static event CampaignConfirm? ConfirmCampaign;
    public Planet planet;
    [SerializeField] GameObject[] increaseButtons;
    [SerializeField] GameObject[] decreaseButtons;
    [SerializeField] GameObject yesButton;
    private CurrencyType increased;
    private CurrencyType decreased;
    bool increaseSelected;
    bool decreaseSelected;

    public void Increase(int increased) {
        DisableButtons(increaseButtons, increased);
        increaseSelected = true;
        AllowCompletion();
        switch(increased) {
            case(0):
                this.increased = CurrencyType.Politics;
                break;
            case(1):
                this.increased = CurrencyType.Intellect;
                break;
            case(2):
                this.increased = CurrencyType.Affluence;
                break;
        }
    }

    public void Decrease(int decreased) {
        DisableButtons(decreaseButtons, decreased);
        decreaseSelected = true;
        AllowCompletion();
        switch(decreased) {
            case(0):
                this.decreased = CurrencyType.Politics;
                break;
            case(1):
                this.decreased = CurrencyType.Intellect;
                break;
            case(2):
                this.decreased = CurrencyType.Affluence;
                break;
        }
    }

    private void DisableButtons(GameObject[] gameObjects, int preserved) {
        for(int i = 0; i < gameObjects.Length; i++) {
            if(preserved != i) {
                gameObjects[i].SetActive(false);
            }
        }
    }

    private void AllowCompletion() {
        yesButton.SetActive(increaseSelected && decreaseSelected);
    }

    public void Complete() {
        ConfirmCampaign.Invoke(increased, decreased, planet);
        Reset();
    }

    private void Reset() {
        foreach(GameObject gameObject in increaseButtons) {
            gameObject.SetActive(true);
        }
        foreach(GameObject gameObject in decreaseButtons) {
            gameObject.SetActive(true);
        }
        increaseSelected = false;
        decreaseSelected = false;
        yesButton.SetActive(false);
        FindObjectOfType<UIController>().UIAnim.SetTrigger("Back");
    }

    public void Back() {
        Reset();
    }
}
