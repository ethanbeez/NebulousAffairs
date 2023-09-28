using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NebulaController : MonoBehaviour {
    public List<Material> planetMaterials = new List<Material>(5);
    public GameObject planetPrefab;
    public float planetLocationLeftBound = -8f;
    public float planetLocationRightBound = 12f;
    public float planetLocationTopBound = 4f;
    public float planetLocationBottomBound = -8f;
    public float planetLocationZ = 10f;
    public int planetsPerRow = 4;
    public int planetsPerCol = 3;

    void Start() {
        
    }


    void Update() {
        
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
