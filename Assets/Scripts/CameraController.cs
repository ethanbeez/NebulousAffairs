using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    public AnimationCurve freeCamReturnCurve;
    public AnimationCurve flyToCurve;
    private Vector3 freeCamStartMarker;
    public Vector3 freeCamEndMarker;
    public GameObject focusTarget;
    private Vector3 mapCamLocation = new(0, 0, 0);
    private Quaternion freeCamStartRotation;
    private Quaternion freeCamEndRotation = Quaternion.LookRotation(new Vector3(0, -1, 0), Vector3.up);
    private float startTime;
    // private float freeCamReturnLength; // TODO: REMOVE
    private bool freeCamReturnComplete;
    private float freeCamReturnTime = 1.75f;
    private Vector3 center;
    public float speed;
    private Vector3 flyToStartMarker;
    private Vector3 flyToEndMarker;
    private Quaternion flyToStartRotation;
    private Quaternion flyToEndRotation = Quaternion.identity;
    private bool flyToLocationComplete;
    private float flyToTime = 1.75f;
    private float startingDistance;

    public float acceleration = 50;
    public float accelerationFastMultiplier = 4;
    public Vector3 velocity;
    public float dampingCoefficient = 5;
    public float lookSensitivity = 1;
    public bool freeCam = false;

    public Vector3 planetClickCameraOffset = new();

    bool FreeCam { 
        get => freeCam;
        set {
            freeCam = value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    // Start is called before the first frame update
    void Start() {
        // planetClickCameraOffset = new();
        freeCamReturnComplete = true;
        flyToLocationComplete = true;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            FreeCam = true;
        } else if (Input.GetKeyDown(KeyCode.M)) {
            FreeCam = false;
            focusTarget = null;
            freeCamReturnComplete = false;
            startTime = Time.time;
            freeCamStartMarker = transform.position;
            freeCamEndMarker = mapCamLocation;
            center = (freeCamStartMarker + freeCamEndMarker) * 0.5f;
            center -= new Vector3(0, 1, 0);
            freeCamStartMarker -= center;
            freeCamEndMarker -= center;
            // freeCamReturnLength = Vector3.Distance(freeCamStartMarker, freeCamEndMarker);

            freeCamStartRotation = transform.rotation;
        }

        if (FreeCam) {
            UpdateInput();
            velocity = Vector3.Lerp(velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
            transform.position += velocity * Time.deltaTime;
        } else {
            if (!freeCamReturnComplete) {
                // Debug.Log(freeCamEndMarker);
                FlyToMapPosition();
            } else if (!flyToLocationComplete) {
                FlyToLocation();
            } else if (focusTarget != null) {
                transform.position = focusTarget.transform.position + planetClickCameraOffset;
            }
        }
    }

    void FlyToMapPosition() {
        // Debug.Log("Flying...");
        float completion = (Time.time - startTime) / freeCamReturnTime;
        // float journeyCompletion = distanceCovered / freeCamReturnLength;
        if (completion >= 1) {
            freeCamReturnComplete = true;
            // freeCamEndMarker = new(0, 0, 0);
            // return;
        }
        // TODO: Take optimization!
        transform.position = Vector3.Slerp(freeCamStartMarker, freeCamEndMarker, freeCamReturnCurve.Evaluate(completion));
        transform.rotation = Quaternion.Slerp(freeCamStartRotation, freeCamEndRotation, freeCamReturnCurve.Evaluate(completion));
        transform.position += center;
        if (freeCamReturnComplete) {
            transform.position = new(0, 0, 0);
        }
    }

    void UpdateInput() {
        velocity += GetAccelerationVector() * Time.deltaTime;
        Vector2 mouseDelta = lookSensitivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));
        Quaternion rotation = transform.rotation;
        Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);
        Quaternion vert = Quaternion.AngleAxis(mouseDelta.y, Vector3.right);
        transform.rotation = horiz * rotation * vert;
    }

    public void StartFly(GameObject focusTarget) {
        this.focusTarget = focusTarget;
        flyToLocationComplete = false;
        flyToStartMarker = transform.position;
        flyToStartRotation = transform.rotation;
        flyToEndMarker = focusTarget.transform.position;
        flyToEndRotation = focusTarget.transform.rotation;

        // center = (flyToStartMarker + flyToEndMarker * 0.5f);
        // center -= new Vector3(0, 1, 0);
        // flyToStartMarker -= center;
        // flyToEndMarker -= center;
        // flyToEndMarker += planetClickCameraOffset;
        startingDistance = Vector3.Distance(flyToStartMarker, flyToEndMarker);
        startTime = Time.time;
    }

    private void FlyToLocation() {
        // Debug.Log("Flying...");
        // float completion = (Time.time - startTime) / flyToTime;
        float distanceRemaining = Vector3.Distance(transform.position, focusTarget.transform.position);

        float completion = ((Time.time - startTime) * speed) / startingDistance;
        // float completion = (transform.position - focusTarget.transform.position) / focusTarget.transform.position;
        // float journeyCompletion = distanceCovered / freeCamReturnLength;
        if (completion >= 1) {
            flyToLocationComplete = true;
            
        }
        // TODO: Take optimization!
        // float step = 5f * Time.deltaTime;
        // transform.position = Vector3.MoveTowards(transform.position, focusTarget.transform.position, step);
        transform.position = Vector3.Slerp(flyToStartMarker, focusTarget.transform.position + planetClickCameraOffset, flyToCurve.Evaluate(completion));
        transform.rotation = Quaternion.Slerp(flyToStartRotation, focusTarget.transform.rotation, flyToCurve.Evaluate(completion));
        // transform.position += center; // TODO: why ):
    }

    Vector3 GetAccelerationVector() {
        Vector3 moveInput = default;
        if (Input.GetKey(KeyCode.W)) { 
            moveInput += transform.forward;
        }
        Vector3 direction = transform.TransformVector(moveInput);
        if (Input.GetKey(KeyCode.LeftShift)) {
            return acceleration * accelerationFastMultiplier * direction;
        }
        return acceleration * direction;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        if (focusTarget != null) {
            Gizmos.DrawSphere(focusTarget.transform.position, 1f);
        }
    }
}
