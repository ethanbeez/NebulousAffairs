using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ActionCounter : MonoBehaviour
{
    [SerializeField] Image[] images;
    [SerializeField] Sprite fullSprite;
    [SerializeField] Sprite emptySprite;
    Animator actionCounter;

    public void UpdateActionDisplay(int PlayerTurnActionsLeft) {
        
        Debug.Log(PlayerTurnActionsLeft);
        for(int i = 0; i < images.Length; i++) {
            if(PlayerTurnActionsLeft > i) {
                images[i].sprite = fullSprite;
            }
            else {
                images[i].sprite = emptySprite;
            }
        }
    }
}
