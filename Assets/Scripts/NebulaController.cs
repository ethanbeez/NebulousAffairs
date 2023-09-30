using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NebulaController : MonoBehaviour {
    private bool nebulaMotionEnabled;
    // public AnimationCurve nextTurnAnimCurve;
    [SerializeField]
    private AnimationCurve turnTransitionAnimCurve;
    public List<Material> planetMaterials = new List<Material>(5);
    public GameObject planetPrefab;
    public GameObject starPrefab;
    public GameObject blackHolePrefab;
    public List<GameObject> planetGameObjects;
    public List<GameObject> starGameObjects;
    public GameObject blackHole;
    public float galaxyDepth = -5f;
    public float blackHoleStarRadius = 20f;
    public float starPlanetMinRadius = 4f;
    public float starPlanetMaxRadius = 8f;

    public float NextTurnAnimTimeElapsed;
    public float TurnAnimSpeedModifier = 2f; // 200%
    private float NextTurnAnimLength = 8f;
    private bool turnTransitionAnimActive;
    private float nextTurnAnimStart;
    private float turnTransitionAnimProgress;

    public float planetLocationZ = 10f;
    public int planetsPerRow = 4;
    public int planetsPerCol = 3;
    public bool motionEnabled = false;
    private bool motionTrailsEnabled;

    void Start() {
        motionTrailsEnabled = false;
        nebulaMotionEnabled = false;
        // turnTransitionAnimCurve = nextTurnAnimCurve;
        turnTransitionAnimActive = false;
        planetGameObjects = new();
        starGameObjects = new();
    }


    void FixedUpdate() {
        if (!nebulaMotionEnabled) return;
        if (turnTransitionAnimActive) {
            UpdateTurnTransitionAnimProgress();
            UpdateGalaxySpeed();
        }
        UpdateGalaxyOrbit();
    }

    public void UpdateTurnTransitionAnimProgress() {
        // Debug.Log(Time.time - nextTurnAnimStart);
        float progress = (Time.time - nextTurnAnimStart) / NextTurnAnimLength;
        // Debug.Log(progress);
        if (progress >= 1) {
            Debug.Log("Anim is over!");
            turnTransitionAnimActive = false;
        }
        turnTransitionAnimProgress = Mathf.Lerp(0f, TurnAnimSpeedModifier, turnTransitionAnimCurve.Evaluate(progress));
    }

    public void StartTurnTransitionAnim() {
        // NextTurnAnimTimeElapsed = 0f;
        turnTransitionAnimActive = true;
        nextTurnAnimStart = Time.time;
        Debug.Log("Anim begins!");
    }

    public void UpdateGalaxySpeed() {
        foreach (GameObject star in starGameObjects) {
            StarController starController = star.GetComponent<StarController>();
            starController.SetCurrentSpeed(starController.DefaultSpeed + (turnTransitionAnimProgress * starController.DefaultSpeed));
        }
        foreach (GameObject planet in planetGameObjects) {
            PlanetController planetController = planet.GetComponent<PlanetController>();
            planetController.SetCurrentSpeed(planetController.DefaultSpeed + (turnTransitionAnimProgress * planetController.DefaultSpeed));
        }
    }

    public void UpdateGalaxyOrbit() {
        foreach (GameObject star in starGameObjects) {
            star.GetComponent<StarController>().Orbit();
        }
        foreach (GameObject planet in planetGameObjects) {
            planet.GetComponent<PlanetController>().Orbit();
        }
    }

    public void ToggleGalaxyMotionTrails() { 
        motionTrailsEnabled = !motionTrailsEnabled;
        foreach (GameObject planet in planetGameObjects) {
            planet.GetComponent<TrailRenderer>().enabled = motionTrailsEnabled;
        }
    }

    public void ToggleNebulaMotion() {
        nebulaMotionEnabled = !nebulaMotionEnabled;
        /*foreach (GameObject star in starGameObjects) {
            star.GetComponent<StarController>().OrbitMotionEnabled = !star.GetComponent<StarController>().OrbitMotionEnabled;
        }
        foreach (GameObject planet in planetGameObjects) {
            planet.GetComponent<PlanetController>().OrbitMotionEnabled = !planet.GetComponent<PlanetController>().OrbitMotionEnabled; // TODO: How do I make this less ugly WAHH
        }*/
    }

    public void InstantiateNebula(int systemsCount, int planetsPerSystem, List<(int ID, string name)> planetsInfo) {
        int currentPlanetListIndex = 0;
        int planetMaterialIndex = 0;
        GameObject blackHoleInstance = Instantiate(blackHolePrefab);
        blackHoleInstance.transform.position = new Vector3(0, galaxyDepth, 0);
        this.blackHole = blackHoleInstance;
        float systemAngDisp = 2 * Mathf.PI / systemsCount;
        for (int i = 0; i < systemsCount; i++) {
            float currentAngle = i * systemAngDisp;
            GameObject starInstance = Instantiate(starPrefab);
            StarController starController = starInstance.GetComponent<StarController>();
            starController.blackHoleController = blackHoleInstance.GetComponent<BlackHoleController>();
            starInstance.transform.position = new Vector3(Mathf.Cos(currentAngle) * blackHoleStarRadius, galaxyDepth, Mathf.Sin(currentAngle) * blackHoleStarRadius);
            starController.Radius = 15f; // TODO: Don't hardcode these
            starController.DefaultSpeed = 0.5f;
            List<(int ID, string name)> planetsForCurrentStar = planetsInfo.GetRange(0, planetsPerSystem);
            InstantiateSystems(starInstance, planetsForCurrentStar, ref planetMaterialIndex);
            currentPlanetListIndex += systemsCount;

            starGameObjects.Add(starInstance);
        }
    }

    public void InstantiateSystems(GameObject starInstance, List<(int ID, string name)> starPlanetsInfo, ref int planetMaterialIndex) {
        int numPlanets = starPlanetsInfo.Count;
        float planetAngDisp = 2 * Mathf.PI / numPlanets;
        float radiusFromStar;
        float radiusIncrement = (starPlanetMaxRadius - starPlanetMinRadius) / numPlanets;
        for (int i = 0; i < numPlanets; i++) {
            float currentAngle = i * planetAngDisp;
            GameObject planetInstance = Instantiate(planetPrefab);

            PlanetController planetController = planetInstance.GetComponent<PlanetController>();
            planetController.name = starPlanetsInfo[i].name;
            planetController.planetName = starPlanetsInfo[i].name;
            planetController.planetID = starPlanetsInfo[i].ID;
            planetController.starController = starInstance.GetComponent<StarController>();

            Material nextPlanetMaterial = planetMaterials[planetMaterialIndex++ % planetMaterials.Count];
            planetInstance.GetComponent<Renderer>().material = nextPlanetMaterial;
            TrailRenderer trailRenderer = planetInstance.GetComponent<TrailRenderer>();
            // trailRenderer.material = nextPlanetMaterial;
            Color planetMaterialColor = nextPlanetMaterial.color;
            trailRenderer.startColor = new(planetMaterialColor.r, planetMaterialColor.g, planetMaterialColor.b, 0.4f);
            trailRenderer.endColor = new(planetMaterialColor.r, planetMaterialColor.g, planetMaterialColor.b, 0);
            radiusFromStar = starPlanetMinRadius + (i * radiusIncrement);
            planetController.radius = radiusFromStar;
            planetController.DefaultSpeed = 5f;
            planetInstance.transform.position = starInstance.transform.position + new Vector3(Mathf.Cos(currentAngle) * radiusFromStar, 0, Mathf.Sin(currentAngle) * radiusFromStar);

            planetGameObjects.Add(planetInstance);
        }
    }
}
