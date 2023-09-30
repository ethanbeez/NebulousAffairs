using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NebulaController : MonoBehaviour {
    public List<Material> planetMaterials = new List<Material>(5);
    public GameObject planetPrefab;
    public GameObject starPrefab;
    public GameObject blackHolePrefab;
    public float galaxyDepth = -5f;
    public float blackHoleStarRadius = 20f;
    public float starPlanetMinRadius = 4f;
    public float starPlanetMaxRadius = 8f;

    public float planetLocationLeftBound = -8f;
    public float planetLocationRightBound = 12f;
    public float planetLocationTopBound = 4f;
    public float planetLocationBottomBound = -8f;
    public float planetLocationZ = 10f;
    public int planetsPerRow = 4;
    public int planetsPerCol = 3;
    public bool motionEnabled = false;

    void Start() {
        
    }


    void Update() {
        
    }

    public void InstantiateNebula(int systemsCount, int planetsPerSystem, List<(int ID, string name)> planetsInfo) {
        int currentPlanetListIndex = 0;
        int planetMaterialIndex = 0;
        GameObject blackHoleInstance = Instantiate(blackHolePrefab);
        blackHoleInstance.transform.position = new Vector3(0, galaxyDepth, 0);
        float systemAngDisp = 2 * Mathf.PI / systemsCount;
        for (int i = 0; i < systemsCount; i++) {
            float currentAngle = i * systemAngDisp;
            GameObject starInstance = Instantiate(starPrefab);
            StarController starController = starInstance.GetComponent<StarController>();
            starController.blackHoleController = blackHoleInstance.GetComponent<BlackHoleController>();
            starInstance.transform.position = new Vector3(Mathf.Cos(currentAngle) * blackHoleStarRadius, galaxyDepth, Mathf.Sin(currentAngle) * blackHoleStarRadius);
            List<(int ID, string name)> planetsForCurrentStar = planetsInfo.GetRange(0, planetsPerSystem);
            InstantiateSystems(starInstance, planetsForCurrentStar, ref planetMaterialIndex);
            currentPlanetListIndex += systemsCount;
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


            radiusFromStar = starPlanetMinRadius + (i * radiusIncrement);
            planetController.radius = radiusFromStar;
            planetInstance.transform.position = starInstance.transform.position + new Vector3(Mathf.Cos(currentAngle) * radiusFromStar, 0, Mathf.Sin(currentAngle) * radiusFromStar);
        }
    }

    public void InstantiatePlanets(List<(int ID, string name)> planetsInfo) {
        int planetMaterialIndex = 0;
        float planetX = planetLocationLeftBound;
        float planetY = planetLocationTopBound;
        float planetZ = planetLocationZ;
        foreach ((int ID, string name) planetInfo in planetsInfo) {
            GameObject planetGameObject = Instantiate(planetPrefab);
            Material nextPlanetMaterial = planetMaterials[planetMaterialIndex++ % planetMaterials.Count];
            planetGameObject.GetComponent<Renderer>().material = nextPlanetMaterial;
            planetGameObject.transform.position = new(planetX, planetY, planetZ);
            PlanetController planetController = planetGameObject.GetComponent<PlanetController>();
            planetController.name = planetInfo.name;
            planetController.planetName = planetInfo.name;
            planetController.planetID = planetInfo.ID;
            GetNextPlanetLocation(ref planetX, ref planetY);
        }
    }

    private void GetNextPlanetLocation(ref float planetX, ref float planetY) {
        planetX += Mathf.Abs(planetLocationLeftBound - planetLocationRightBound) / planetsPerRow;
        if (planetX >= planetLocationRightBound) {
            planetX = planetLocationLeftBound;
            planetY -= Mathf.Abs(planetLocationTopBound - planetLocationBottomBound) / planetsPerCol;
        }
    }
}
