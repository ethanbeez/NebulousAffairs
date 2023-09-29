#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TurnHandler;

public class PlanetController : MonoBehaviour {
    public AnimationCurve orbitAnimCurve;
    public StarController starController;
    public float orbitRadius;
    public int orbitPhase;
    public float orbitPeriod = 10;
    private float startTime;
    private float centerOffset = 10;

    public string planetName;
    public int planetID;
    public delegate void PlanetClickHandler(int planetID, Vector3 location, string planetName); // TODO: Both for simplicity; planetName likely removed later
    public static event PlanetClickHandler? PlanetClicked;
    private Vector3 phase0Start;
    private Vector3 phase1Start;
    private Vector3 phase2Start;
    private Vector3 phase3Start;
    public Vector3 center;

    // Start is called before the first frame update
    void Start() {
        orbitPhase = 0;
        startTime = Time.time;
        Vector3 starLocation = starController.StarLocation;
        Debug.Log(starLocation);
        phase0Start = starLocation + new Vector3(0, 0, orbitRadius);
        Debug.Log(phase0Start);
        phase1Start = starLocation + new Vector3(orbitRadius, 0, 0);
        phase2Start = starLocation + new Vector3(0, 0, -orbitRadius);
        phase3Start = starLocation + new Vector3(-orbitRadius, 0, 0);
        this.transform.position = phase0Start;
    }

    // Update is called once per frame
    void FixedUpdate() {
        Orbit();
    }

    void OnMouseDown() {
        PlanetClicked?.Invoke(planetID, transform.position, planetName);
    }

    void Orbit() {
        float phaseCompletion = (Time.time - startTime) / orbitPeriod;
        
        if (phaseCompletion >= 1.0) {
            phaseCompletion = 0;
            orbitPhase = ++orbitPhase % 4;
            startTime = Time.time;
        }
        center = new();
        Vector3 startRelCenter = new();
        Vector3 endRelCenter = new();
        switch (orbitPhase) {
            case 0:
                center = (phase0Start + phase1Start) * 0.5f;
                center -= new Vector3(0, 0, centerOffset);
                // center += new Vector3(0, 5f, 0);
                startRelCenter = phase0Start - center;
                endRelCenter = phase1Start - center;
                break;
            case 1:
                center = (phase1Start + phase2Start) * 0.5f;
                center -= new Vector3(centerOffset, 0, 0);
                // center += new Vector3(0, 5f, 0);
                startRelCenter = phase1Start - center;
                endRelCenter = phase2Start - center;
                break;
            case 2:
                center = (phase2Start + phase3Start) * 0.5f;
                center -= new Vector3(0, 0, -centerOffset);
                // center += new Vector3(0, 5f, 0);
                startRelCenter = phase2Start - center;
                endRelCenter = phase3Start - center;
                break;
            case 3:
                center = (phase3Start + phase0Start) * 0.5f;
                center -= new Vector3(-centerOffset, 0, 0);
                // center += new Vector3(0, 5f, 0);
                startRelCenter = phase3Start - center;
                endRelCenter = phase0Start - center;
                break;
        }
        transform.position = Vector3.Slerp(startRelCenter, endRelCenter, orbitAnimCurve.Evaluate(phaseCompletion)) + center;
    }
}
