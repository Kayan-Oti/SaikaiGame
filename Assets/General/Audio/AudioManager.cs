using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using MyBox;

public class AudioManager : Singleton<AudioManager>
{
    //Sliders Values
    [Range(0, 1)] public float masterVolume = 1;
    [Range(0, 1)] public float musicVolume = 1;
    [Range(0, 1)] public float ambienceVolume = 1;
    [Range(0, 1)] public float sfxVolume = 1;

    //Mixers
    private Bus masterBus;
    private Bus musicBus;
    private Bus ambienceBus;
    private Bus sfxBus;

    //Actives Sounds
    private List<EventInstance> _eventInstances;
    private List<StudioEventEmitter> _eventEmitters;
    private EventInstance _ambienceEventInstance;
    private EventInstance _musicEventInstance;

    #region Setup
    private void Awake(){
        _eventInstances = new List<EventInstance>();
        _eventEmitters = new List<StudioEventEmitter>();

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    public void UpdateVolume(){
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        ambienceBus.setVolume(ambienceVolume);
        sfxBus.setVolume(sfxVolume);
    }
    #endregion

    #region SFX
    public void PlayOneShot(EventReference sound, Vector3 worldPos = default){
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateEventInstance(EventReference eventReference){
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        _eventInstances.Add(eventInstance);
        return eventInstance;
    }
    #endregion

    #region EventEmitter - By Distance
    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject){
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        _eventEmitters.Add(emitter);
        return emitter;
    }
    #endregion

    #region Ambience
    public void InitializeAmbience(EventReference ambienceEventReference){
        _ambienceEventInstance = CreateEventInstance(ambienceEventReference);
        _ambienceEventInstance.start();
    }
    public void StopAmbience(){
        _ambienceEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    public void SetAmbienceParameter(string parameterName, float parameterValue){
        _ambienceEventInstance.setParameterByName(parameterName, parameterValue);
    }
    #endregion

    #region Music
    public void InitializeMusic(EventReference musicEventReference, MusicIntensity intensity = MusicIntensity.Intensity1){
        _musicEventInstance = CreateEventInstance(musicEventReference);
        SetMusicIntensity(intensity);
        _musicEventInstance.start();
    }

    public void StopMusic(){
        _musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void SetMusicIntensity(MusicIntensity intensity){
        _musicEventInstance.setParameterByName("MusicIntensity", (float)intensity);
    }
    #endregion

    #region CleanUp
    private void CleanUp(){
        foreach(EventInstance eventInstance in _eventInstances){
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        foreach(StudioEventEmitter emitter in _eventEmitters){
            emitter.Stop();
        }
    }

    private void OnDestroy() {
        Debug.Log("On Destroy");
        CleanUp();
    }
    #endregion
}
