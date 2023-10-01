#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    public delegate void SpacePressHandler(); 
    public static event SpacePressHandler? SpacePressed;

    public delegate void MPressHandler(); 
    public static event MPressHandler? MPressed;

    public delegate void SPressHandler();
    public static event SPressHandler? SPressed;


    public delegate void TPressHandler();
    public static event TPressHandler? TPressed;

    public delegate void EscapePressHandler();
    public static event EscapePressHandler? EscapePressed;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            SpacePressed?.Invoke();
        } else if (Input.GetKeyDown(KeyCode.M)) {
            MPressed?.Invoke();
        } else if (Input.GetKeyDown(KeyCode.S)) {
            SPressed?.Invoke();
        } else if (Input.GetKeyDown(KeyCode.T)) {
            TPressed?.Invoke();
        } else if (Input.GetKeyDown(KeyCode.Escape)) { 
            EscapePressed?.Invoke();
        }
    }
}
