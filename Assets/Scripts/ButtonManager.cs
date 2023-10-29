#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public delegate void LeaderButtonHandler(int index);
    public static event LeaderButtonHandler? LeaderButtonPressed;

    public void LeaderScreen(int index) {
        LeaderButtonPressed?.Invoke(index);
    }
}
