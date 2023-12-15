using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;
using static UnityEngine.RuleTile.TilingRuleOutput;

[RequireComponent(typeof(Camera))]
public class CameraStateContext : MonoBehaviour {
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
    private static Vector3 MainMenuCameraPosition = new(0, 2000, 0);
    private static Quaternion MainMenuCameraRotation = Quaternion.LookRotation(new Vector3(0, 0, 0), Vector3.up);

    private Quaternion freeCamStartRotation;
    private Quaternion freeCamEndRotation = Quaternion.LookRotation(new Vector3(0, -1, 0), Vector3.up);
    private float startTime;
    // private float freeCamReturnLength; // TODO: REMOVE
    // public bool freeCamReturnComplete;
    private float freeCamReturnTime = 1.75f;
    private Vector3 center;
    public float speed;
    private Vector3 flyToStartMarker;
    private Vector3 flyToEndMarker;
    private Quaternion flyToStartRotation;
    private Quaternion flyToEndRotation = Quaternion.identity;
    // public bool flyToLocationComplete;
    private float flyToTime = 1.75f;
    private float startingDistance;

    public float acceleration = 50;
    public float accelerationFastMultiplier = 4;
    public Vector3 velocity;
    public float dampingCoefficient = 5;
    public float lookSensitivity = 1;
    public bool freeCam = false;
    // public bool introFlyComplete;
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

    public enum FixedState { 
        Default = 0,
        MainMenu = 1,
        MapView = 2,
        PlanetView = 3,
        FreeCamera = 4
    }

    public FixedState state;
    public bool transitioning = false;

    public enum TransitionState {
        None = 0,
        MainMenuToMatchTransition = 1,
        MatchToMainMenuTransition = 2

    }

    BaseCameraState cameraState;

    public FixedState Fixed { get; private set; } = FixedState.Default;
    public TransitionState Transition { get; private set; } = TransitionState.None;
    // Start is called before the first frame update
    void Start() {
        volumeProfile = GameObject.Find("PostProcessVolume").GetComponent<UnityEngine.Rendering.Volume>()?.profile;
        if (volumeProfile.TryGet(out UnityEngine.Rendering.Universal.LensDistortion distort)) {
            lensDistortion = distort;
        }
        cameraState = new MainMenuCameraState(this);
        cameraState.Enter();
        state = FixedState.MainMenu;
        // ChangeState(FixedState.MainMenuMouseTrack);
        // if (!volumeProfile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));

        // if (!volumeProfile.TryGet(out UnityEngine.Rendering.Universal. vignette)) throw new System.NullReferenceException(nameof(vignette));

        // vignette.intensity.Override(0.5f);
        // ppVolume = GameObject.Find("PostProcessVolume").GetComponent<PostProcessVolume>();
        // planetClickCameraOffset = new();
        // transform.position = new(0, 5000, 0);
        // TODO: 11/16 open heart surgery comment out
        /*freeCamReturnComplete = true;
        flyToLocationComplete = true;
        introFlyComplete = false;
        screenFadedIn = false;
        warpVFX.Stop();
        StartIntroFly();*/
    }

    public void EnterMainMenuState() {
        EnterState(new MainMenuCameraState(this));
    }

    public void EnterMapState() {
        FixedState prevState = state;
        if (prevState == FixedState.MapView) return;
        state = FixedState.MapView;
        if (prevState == FixedState.MainMenu) {
            EnterState(new MainMenuToMapCameraState(this, Time.time, flyToWarpCurve, flyToDistortionCurve, transform.rotation));
        } else if (prevState == FixedState.PlanetView) {
            EnterState(new PlanetToMapCameraState(this, Time.time, transform.position, transform.rotation, freeCamReturnCurve));
        }
    }


    public void TrackPlanet(GameObject targetPlanet) {
        FixedState prevState = state;
        if (prevState == FixedState.PlanetView) return;
        state = FixedState.PlanetView;
        EnterState(new MapToPlanetCameraState(this, targetPlanet, Time.time, planetClickCameraOffset, flyToCurve));
    }

    public void EnterState(BaseCameraState cameraState) {
        this.cameraState = cameraState;
        cameraState.Enter();
    }

    // Update is called once per frame
    void Update() {
        cameraState.Update();
    }

    /*public void StartMapFly() {
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
    }*/

    /*public void StartIntroFly() {
        FreeCam = false;
        focusTarget = null;
        startTime = Time.time;
        freeCamStartMarker = MainMenuCameraPosition;
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
    }*/

    void FadeScreen(float duration) {
        float completion = (Time.time - startTime) / duration;
        if (completion >= 1) {
            screenFadedIn = true;
            canvas.alpha = 1;
        }
        canvas.alpha = Mathf.Lerp(0, 1, flyToWarpCurve.Evaluate(completion));
    }

    /*void UpdateInput() {
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
    }*/
}

public abstract class BaseCameraState {
    protected CameraStateContext cameraStateContext;
    public BaseCameraState(CameraStateContext context) { cameraStateContext = context; }
    public abstract void Enter();
    public abstract void Update();
}

public class MainMenuCameraState : BaseCameraState {
    private static Vector3 MainMenuCameraPosition = new(0, 2000, 0);
    private float angle = 0f;

    public MainMenuCameraState(CameraStateContext context) : base(context) {}

    public override void Enter() {
        cameraStateContext.warpVFX.Stop();
        cameraStateContext.transform.position = MainMenuCameraPosition;
    }

    public override void Update() {
        angle = (angle + Time.deltaTime * 0.02f) % 360;
        float x = Mathf.Cos(angle) * 3f;
        float y = Mathf.Sin(angle) * 3f;
        Debug.Log(y);
        Vector3 pos = new(x, y);
        Vector3 targetLook = new(x, y, 5f);
        
        cameraStateContext.transform.LookAt(targetLook, Vector3.up);
    }
}

public class MainMenuToMapCameraState : BaseCameraState {
    public AnimationCurve flyToWarpCurve;
    public AnimationCurve flyToDistortionCurve;
    private static Vector3 MainMenuCameraPosition = new(0, 2000, 0);
    private Quaternion startingRotation;
    private Quaternion mapCameraRotation = Quaternion.LookRotation(new Vector3(0, -1, 0), Vector3.up);
    private Vector3 MapCameraPosition = new(0, 0, 0);
    private const float duration = 9f;
    private float startTime;

    public MainMenuToMapCameraState(CameraStateContext context, float startTime, AnimationCurve flyToWarpCurve, AnimationCurve flyToDistortionCurve, Quaternion startingRotation) : base(context) {
        this.flyToWarpCurve = flyToWarpCurve;
        this.flyToDistortionCurve = flyToDistortionCurve;
        this.startTime = startTime;
        this.startingRotation = startingRotation;
    }

    public override void Enter() {
        cameraStateContext.lensDistortion.intensity.value = -1f;
        cameraStateContext.warpVFX.Play();
    }

    public override void Update() {
        float completion = (Time.time - startTime) / duration;
        if (completion >= 1) {
            Exit();
        }
        Debug.Log("In Main Menu to Map");
        cameraStateContext.transform.position = Vector3.Lerp(MainMenuCameraPosition, MapCameraPosition, flyToWarpCurve.Evaluate(completion));
        cameraStateContext.transform.rotation = Quaternion.Slerp(startingRotation, mapCameraRotation, flyToWarpCurve.Evaluate(completion));
        float value = Mathf.Lerp(0, -1, flyToDistortionCurve.Evaluate(completion));
        float vfxValue = Mathf.Lerp(1, 0, flyToDistortionCurve.Evaluate(completion));
        cameraStateContext.warpVFX.SetFloat("WarpAmount", Mathf.Clamp(vfxValue - 0.15f, 0, 1));
        cameraStateContext.lensDistortion.intensity.value = value;
        cameraStateContext.lensDistortion.scale.value = Mathf.Clamp(value + 1.4f, 0.01f, 1);
        // cameraStateContext.transform.position += center;
    }

    private void Exit() {
        cameraStateContext.warpVFX.Stop();
        cameraStateContext.transform.position = new(0, 0, 0);
        cameraStateContext.lensDistortion.intensity.value = 0;
        cameraStateContext.lensDistortion.scale.value = 0;
        cameraStateContext.EnterState(new MapCameraState(cameraStateContext));
    }
}

public class MapToMainMenuCameraState : BaseCameraState {
    public MapToMainMenuCameraState(CameraStateContext context) : base(context) {}


    public override void Enter() {
        throw new NotImplementedException();
    }

    public override void Update() {
        throw new NotImplementedException();
    }
}

public class MapCameraState : BaseCameraState {
    public MapCameraState(CameraStateContext context) : base(context) {}

    public override void Enter() {
    }

    public override void Update() {
        Debug.Log("In Map");
    }
}

public class PlanetCameraState : BaseCameraState {
    private Vector3 planetClickCameraOffset;
    private GameObject focusTarget;
    public PlanetCameraState(CameraStateContext context, GameObject focusTarget, Vector3 planetClickCameraOffset) : base(context) {
        this.planetClickCameraOffset = planetClickCameraOffset;
        this.focusTarget = focusTarget;
    }

    public override void Enter() {
        
    }

    public override void Update() {
        cameraStateContext.transform.position = focusTarget.transform.position + planetClickCameraOffset;
    }
}

public class MapToPlanetCameraState : BaseCameraState {
    private const float speed = 15f;
    private GameObject focusTarget;
    private float startTime;
    private float startingDistance;
    private Vector3 mapCameraLocation = new(0, 0, 0);
    private Vector3 planetCameraLocation;
    private Quaternion mapCameraRotation;
    private Vector3 planetClickCameraOffset;
    private AnimationCurve flyToCurve;
    public MapToPlanetCameraState(CameraStateContext context, GameObject focusTarget, float startTime, Vector3 planetClickCameraOffset, AnimationCurve flyToCurve) : base(context) {
        this.focusTarget = focusTarget;
        this.startTime = startTime;
        this.planetClickCameraOffset = planetClickCameraOffset;
        this.flyToCurve = flyToCurve;
    }

    public override void Enter() {
        mapCameraRotation = cameraStateContext.transform.rotation;
        startingDistance = Vector3.Distance(mapCameraLocation, focusTarget.transform.position);
    }

    public override void Update() {
        float completion = ((Time.time - startTime) * speed) / startingDistance;
        if (completion >= 1) {
            Exit();
        }
        // TODO: Take optimization!
        cameraStateContext.transform.position = Vector3.Slerp(mapCameraLocation, focusTarget.transform.position + planetClickCameraOffset, flyToCurve.Evaluate(completion));
        cameraStateContext.transform.rotation = Quaternion.Slerp(mapCameraRotation, new(0, 0, 0, 1), flyToCurve.Evaluate(completion));
    }

    public void Exit() {
        cameraStateContext.EnterState(new PlanetCameraState(cameraStateContext, focusTarget, planetClickCameraOffset));
    }
}

public class PlanetToMapCameraState : BaseCameraState {
    private float startTime;
    private const float freeCamReturnTime = 1.75f;
    private Vector3 center;
    private Vector3 mapCameraLocation = new(0, 0, 0);
    private Vector3 planetLocation;
    private Quaternion planetRotation;
    private Quaternion mapCameraRotation = Quaternion.LookRotation(new Vector3(0, -1, 0), Vector3.up);
    private AnimationCurve mapReturnCurve;
    public PlanetToMapCameraState(CameraStateContext context, float startTime, Vector3 planetLocation, Quaternion planetRotation, AnimationCurve mapReturnCurve) : base(context) {
        this.startTime = startTime;
        this.planetLocation = planetLocation;
        this.planetRotation = planetRotation;
        this.mapReturnCurve = mapReturnCurve;
    }

    public override void Enter() {
        center = (mapCameraLocation + planetLocation) * 0.5f;
        center -= new Vector3(0, 1, 0);
        mapCameraLocation -= center;
        planetLocation -= center;
    }

    public override void Update() {
        float completion = (Time.time - startTime) / freeCamReturnTime;
        if (completion >= 1) {
            Exit();
        }
        // TODO: Take optimization!
        cameraStateContext.transform.position = Vector3.Slerp(planetLocation, mapCameraLocation, mapReturnCurve.Evaluate(completion));
        cameraStateContext.transform.rotation = Quaternion.Slerp(planetRotation, mapCameraRotation, mapReturnCurve.Evaluate(completion));
        cameraStateContext.transform.position += center;
    }

    public void Exit() {
        cameraStateContext.transform.position = new(0, 0, 0);
        cameraStateContext.EnterState(new MapCameraState(cameraStateContext));
    }
}