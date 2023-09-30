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
    private float centerOffset = -5;

    public bool OrbitMotionEnabled { get; set; }
    public float CurrentSpeed { get; private set; }
    public float DefaultSpeed { get; set; }



    public string planetName;
    public int planetID;
    public delegate void PlanetClickHandler(int planetID, GameObject focusTarget, string planetName); // TODO: Both for simplicity; planetName likely removed later
    public static event PlanetClickHandler? PlanetClicked;
    private Vector3 phase0Start;
    private Vector3 phase1Start;
    private Vector3 phase2Start;
    private Vector3 phase3Start;
    public Vector3 center;
    public Vector3 startRelCenter;
    public Vector3 endRelCenter;
    public float randomOrbitSpeed;
    public float radius;

    // Start is called before the first frame update
    void Start() {
        OrbitMotionEnabled = false;
        randomOrbitSpeed = (Random.value * 5f);
        DefaultSpeed += randomOrbitSpeed;
        CurrentSpeed = DefaultSpeed;
        // orbitPhase = 0;
        // startTime = Time.time;
        // Vector3 starLocation = starController.StarLocation;
        // phase0Start = starLocation + new Vector3(0, 0, orbitRadius);
        // Debug.Log(phase0Start);
        // phase1Start = starLocation + new Vector3(orbitRadius, 0, 0);
        // phase1Start = starLocation + new Vector3(0, 0, -orbitRadius);
        // phase3Start = starLocation + new Vector3(-orbitRadius, 0, 0);
        // this.transform.position = phase0Start;
    }

    void FixedUpdate() {
        // if (NebulaController.NextTurnAnimActive) UpdateNextTurnAnimStarSpeed(NebulaController.NextTurnAnimProgress);
        // if (OrbitMotionEnabled) Orbit();
    }

    public void SetCurrentSpeed(float speed) {
        /*if (progress >= 1) {
            CurrentSpeed = DefaultSpeed;
            return;
        }*/
        CurrentSpeed = speed;
    }

    void OnMouseDown() {
        PlanetClicked?.Invoke(planetID, gameObject, planetName);
    }

    public void Orbit() {
        Vector3 normalized = radius * Vector3.Normalize(transform.position - starController.StarLocation) + starController.StarLocation;
        transform.position = NebulaMath.RotateAroundBody(normalized, starController.StarLocation, Vector3.up, CurrentSpeed * Time.deltaTime);
    }

    // TODO: Semi-deprecated, remove if we don't want fine-tuned control over paths
    /* void Orbit() {
        float phaseCompletion = (Time.time - startTime) / orbitPeriod;
        
        if (phaseCompletion >= 1.0) {
            phaseCompletion = 0;
            orbitPhase = ++orbitPhase % 2;
            startTime = Time.time;
        }
        center = new();
        startRelCenter = new();
        endRelCenter = new();
        switch (orbitPhase) {
            case 0:
                // center = (phase0Start + phase1Start) * 0.5f;
                center = new Vector3(-1f, centerOffset, 0);
                // center += new Vector3(0, 5f, 0);
                startRelCenter = phase0Start - center;
                endRelCenter = phase1Start - center;
                break;
            case 1:
                // center = (phase1Start + phase2Start) * 0.5f;
                center = new Vector3(1f, centerOffset, 0);
                // center += new Vector3(0, 5f, 0);
                startRelCenter = phase1Start - center;
                endRelCenter = phase0Start - center;
                break;
            case 2:
                // center = (phase2Start + phase3Start) * 0.5f;
                center -= new Vector3(0, 0, -centerOffset);
                // center += new Vector3(0, 5f, 0);
                startRelCenter = phase2Start - center;
                endRelCenter = phase3Start - center;
                break;
            case 3:
                // center = (phase3Start + phase0Start) * 0.5f;
                center -= new Vector3(-centerOffset, 0, 0);
                // center += new Vector3(0, 5f, 0);
                startRelCenter = phase3Start - center;
                endRelCenter = phase0Start - center;
                break;
        }
        transform.position = Vector3.Slerp(startRelCenter, endRelCenter, orbitAnimCurve.Evaluate(phaseCompletion)) + center;
    }*/
}
