using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarController : MonoBehaviour {
    public BlackHoleController blackHoleController;
    public Vector3 StarLocation => transform.position;
    public float CurrentSpeed { get; private set; }
    public float DefaultSpeed { get; set; }
    public float Radius { get; set; }
    public bool OrbitMotionEnabled { get; set; }
    // Start is called before the first frame update
    void Start() {
        
    }

    void FixedUpdate() {
        // if (NebulaController.NextTurnAnimActive) UpdateNextTurnAnimStarSpeed(NebulaController.NextTurnAnimProgress);
        // if (OrbitMotionEnabled) Orbit();
        // transform.RotateAround(blackHoleController.BlackHoleLocation, Vector3.up, 1 * Time.deltaTime);
    }

    public void SetCurrentSpeed(float speed) {
        /*if (progress >= 1) {
            CurrentSpeed = DefaultSpeed;
            return;
        }*/
        // Debug.Log(progress);
        CurrentSpeed = speed;
    }

    public void Orbit() {
        Vector3 normalized = Radius * Vector3.Normalize(transform.position - blackHoleController.BlackHoleLocation) + blackHoleController.BlackHoleLocation;
        transform.position = NebulaMath.RotateAroundBody(normalized, blackHoleController.BlackHoleLocation, Vector3.up, CurrentSpeed * Time.deltaTime);
    }
}
