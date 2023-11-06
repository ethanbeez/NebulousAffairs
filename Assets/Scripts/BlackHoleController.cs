using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleController : MonoBehaviour {
    [SerializeField]
    public GameObject blackHole;
    public Vector3 BlackHoleLocation => blackHole.transform.position;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        // Debug.Log(BlackHoleLocation);
    }
}
