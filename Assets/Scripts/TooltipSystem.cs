using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

//Source: https://www.youtube.com/watch?v=HXFoUGw7eKk
public class TooltipSystem : MonoBehaviour
{
    private static TooltipSystem current;

    public Tooltip tooltip;

    public void Awake() {
        current = this;
    }

    public static void Show(string content) {
        current.tooltip.SetText(content);
        current.tooltip.gameObject.SetActive(true);
    }

    public static void Hide() {
        current.tooltip.gameObject.SetActive(false);
    }
}
