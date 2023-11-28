using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

// Code Heavily Sourced From:  https://www.youtube.com/watch?v=rcBHIOjZDpk&ab_channel=ShapedbyRainStudios

public class AudioManager : MonoBehaviour
{    
    public static AudioManager instance {get; private set;}
    private List<EventInstance> eventInstances;
    private EventInstance ambienceEventInstance;
    private EventInstance musicEventInstance;
    private List<StudioEventEmitter> eventEmitters;

    private void Awake() {
        if( instance != null) {
            Debug.LogError("Found more than one Audio Manager in the scene.");
        }
        instance = this;
        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();
    }

    private void Start() {
        InitializeMusic(FMODEvents.instance.music);
    }

    private void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambienceEventInstance = CreateEventInstance(ambienceEventReference);
        ambienceEventInstance.start();
    }

    private void InitializeMusic(EventReference musicEventReference)
    {
        musicEventInstance = CreateEventInstance(musicEventReference);
        musicEventInstance.start();

    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos) {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateEventInstance(EventReference eventReference) {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }
    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject) {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    private void CleanUp() 
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        foreach (StudioEventEmitter eventEmitter in eventEmitters)
        {
            eventEmitter.Stop();
        }
    }

    private void OnDestroy() {
        CleanUp();
    }
}
