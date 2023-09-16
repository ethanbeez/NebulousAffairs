using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    GameHandler gameHandler;
    // Start is called before the first frame update
    void Start() {
        gameHandler = new();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 1"));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 2"));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 3"));
        }


        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 4"));
        }


        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 5"));
        }


        if (Input.GetKeyDown(KeyCode.Alpha6)) {
            Debug.Log(gameHandler.DEBUG_GetLeaderInfoString("Leader 6"));
        }
    }
}
