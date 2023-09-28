#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnHandler;

public class PlanetController : MonoBehaviour {
    public string planetName;
    public int planetID;
    public delegate void PlanetClickHandler(int planetID, Vector3 location, string planetName); // TODO: Both for simplicity; planetName likely removed later
    public static event PlanetClickHandler? PlanetClicked;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    void OnMouseDown() {
        PlanetClicked?.Invoke(planetID, transform.position, planetName);
    }
}
