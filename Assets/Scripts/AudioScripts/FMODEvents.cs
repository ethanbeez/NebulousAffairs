using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using System.Runtime.InteropServices;

// Heavily Referenced from https://www.youtube.com/watch?v=rcBHIOjZDpk&ab_channel=ShapedbyRainStudios
public class FMODEvents : MonoBehaviour
{
    [field: Header("Sound")]
    [field: SerializeField] public EventReference music {get; private set;}

    // Start is called before the first frame update
    public static FMODEvents instance {get; private set;}
    private void Awake() {
        if( instance != null) {
            Debug.LogError("Found more than one FMOD Events Instance in the scene.");
        }
        instance = this;
    }
}
