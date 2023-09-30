using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour {

    public BlackHoleController blackHoleController;
    public Vector3 StarLocation => this.transform.position;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        this.transform.RotateAround(blackHoleController.BlackHoleLocation, Vector3.up, 1 * Time.deltaTime);
    }    
}
