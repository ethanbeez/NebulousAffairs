using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.VFX;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    public CanvasGroup canvas;
    public VisualEffect warpVFX;
    private bool warpActive;
    public AnimationCurve freeCamReturnCurve;
    public AnimationCurve flyToCurve;
    public AnimationCurve flyToWarpCurve;
    public AnimationCurve flyToDistortionCurve;
    // public AnimationCurve introCurve;
    private Vector3 freeCamStartMarker;
    public Vector3 freeCamEndMarker;
    public GameObject focusTarget;
    private Vector3 mapCamLocation = new(0, 0, 0);
    private Vector3 introCamLocation = new(0, 2000, 0);
    private Quaternion freeCamStartRotation;
    private Quaternion freeCamEndRotation = Quaternion.LookRotation(new Vector3(0, -1, 0), Vector3.up);
    private float startTime;
    // private float freeCamReturnLength; // TODO: REMOVE
    public bool freeCamReturnComplete;
    private float freeCamReturnTime = 1.75f;
    private Vector3 center;
    public float speed;
    private Vector3 flyToStartMarker;
    private Vector3 flyToEndMarker;
    private Quaternion flyToStartRotation;
    private Quaternion flyToEndRotation = Quaternion.identity;
    public bool flyToLocationComplete;
    private float flyToTime = 1.75f;
    private float startingDistance;

    public float acceleration = 50;
    public float accelerationFastMultiplier = 4;
    public Vector3 velocity;
    public float dampingCoefficient = 5;
    public float lookSensitivity = 1;
    public bool freeCam = false;
    public bool introFlyComplete;
    public bool screenFadedIn;

    public Vector3 planetClickCameraOffset = new();

    public UnityEngine.Rendering.VolumeProfile volumeProfile;
    public UnityEngine.Rendering.Universal.LensDistortion lensDistortion;

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
        volumeProfile = GameObject.Find("PostProcessVolume").GetComponent<UnityEngine.Rendering.Volume>()?.profile;
        if (volumeProfile.TryGet(out UnityEngine.Rendering.Universal.LensDistortion distort)) {
            lensDistortion = distort;
        }
        // if (!volumeProfile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));

        // You can leave this variable out of your function, so you can reuse it throughout your class.

        // if (!volumeProfile.TryGet(out UnityEngine.Rendering.Universal. vignette)) throw new System.NullReferenceException(nameof(vignette));

        // vignette.intensity.Override(0.5f);
        // ppVolume = GameObject.Find("PostProcessVolume").GetComponent<PostProcessVolume>();
        // planetClickCameraOffset = new();
        // transform.position = new(0, 5000, 0);
        freeCamReturnComplete = true;
        flyToLocationComplete = true;
        introFlyComplete = false;
        screenFadedIn = false;
        warpVFX.Stop();
        StartIntroFly();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            FreeCam = true;
        }

        if (FreeCam) {
            UpdateInput();
            velocity = Vector3.Lerp(velocity, Vector3.zero, dampingCoefficient * Time.deltaTime);
            transform.position += velocity * Time.deltaTime;
        } else {
            if (!introFlyComplete) {
                if (!screenFadedIn) FadeScreen(3);
                FlyToMapPosition(9);
            } else if (!freeCamReturnComplete) {
                // Debug.Log(freeCamEndMarker);
                // FlyToMapPosition(10);
                FlyToMapPosition();
            } else if (!flyToLocationComplete) {
                FlyToLocation();
            } else if (focusTarget != null) {
                transform.position = focusTarget.transform.position + planetClickCameraOffset;
            }
        }
    }

    public void StartMapFly() {
        if (!introFlyComplete || !freeCamReturnComplete || !flyToLocationComplete) return;
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

    public void StartIntroFly() {
        FreeCam = false;
        focusTarget = null;
        startTime = Time.time;
        freeCamStartMarker = introCamLocation;
        freeCamEndMarker = mapCamLocation;
        center = (freeCamStartMarker + freeCamEndMarker) * 0.5f;
        center -= new Vector3(0, 1, 0);
        freeCamStartMarker -= center;
        freeCamEndMarker -= center;
        // freeCamReturnLength = Vector3.Distance(freeCamStartMarker, freeCamEndMarker);
        
        freeCamStartRotation = transform.rotation;
        lensDistortion.intensity.value = -1f;
        warpActive = true;
        warpVFX.Play();
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

    void FlyToMapPosition(float duration) {
        // Debug.Log("Flying...");
        float completion = (Time.time - startTime) / duration;
        if (completion >= 1) {
            introFlyComplete = true;
            warpVFX.Stop();
        }
        // TODO: Take optimization!
        transform.position = Vector3.Lerp(freeCamStartMarker, freeCamEndMarker, flyToWarpCurve.Evaluate(completion));
        float value = Mathf.Lerp(0, -1, flyToDistortionCurve.Evaluate(completion));
        float vfxValue = Mathf.Lerp(1, 0, flyToWarpCurve.Evaluate(completion));
        // Debug.Log(value);
        warpVFX.SetFloat("WarpAmount", Mathf.Clamp(vfxValue - 0.45f, 0, 1));
        lensDistortion.intensity.value = value;
        lensDistortion.scale.value = Mathf.Clamp(value + 1.4f, 0.01f, 1);
        // transform.rotation = Quaternion.Lerp(freeCamStartRotation, freeCamEndRotation, freeCamReturnCurve.Evaluate(completion));
        transform.position += center;
        if (introFlyComplete) {
            transform.position = new(0, 0, 0);
            lensDistortion.intensity.value = 0;
            lensDistortion.scale.value = 0;
        }
    }

    void FadeScreen(float duration) {
        float completion = (Time.time - startTime) / duration;
        if (completion >= 1) {
            screenFadedIn = true;
            canvas.alpha = 1;
        }
        canvas.alpha = Mathf.Lerp(0, 1, flyToWarpCurve.Evaluate(completion));
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
        if (!introFlyComplete || !flyToLocationComplete || !freeCamReturnComplete) return;
        this.focusTarget = focusTarget;
        flyToLocationComplete = false;
        flyToStartMarker = transform.position;
        Debug.Log(flyToStartMarker);
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
        // float distanceRemaining = Vector3.Distance(transform.position, focusTarget.transform.position);

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
        // Debug.Log(focusTarget.transform.rotation);
        transform.rotation = Quaternion.Slerp(flyToStartRotation, new(0, 0, 0, 1), flyToCurve.Evaluate(completion));
        // ppVolume.instance
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
