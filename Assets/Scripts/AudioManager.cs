using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

[System.Serializable]
public class FMODSoundObject {
    public string id = "default";
    [FMODUnity.EventRef]
    public string fmodEvent;
}

public class AudioManager : MonoBehaviour {
    public static AudioManager instance;

    public StudioEventEmitter sfxEmitter;
    public StudioEventEmitter musicEmitter;

    public FMOD_MasterVolume volumeController;
    public bool singleton = true;

    [NaughtyAttributes.ReorderableList]
    public List<FMODSoundObject> eventRefs = new List<FMODSoundObject> { };

    private Dictionary<string, FMODSoundObject> soundDict = new Dictionary<string, FMODSoundObject> { };
    // Start is called before the first frame update
    void Awake () {
        if (singleton) {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad (gameObject);
            } else {
                Destroy (gameObject);
            }
        };
        foreach (FMODSoundObject soundObj in eventRefs) {
            soundDict.Add (soundObj.id, soundObj);
        }
    }

    public void LoadVolume () {
        float savedVolume = ES3.Load<float> (SaveManager.instance.CurrentSlot + "_masterVolume", 1f);
        if (volumeController != null) {
            volumeController.MasterVolumeLevel (savedVolume);
        }
    }
    public void SaveVolume () {
        if (volumeController != null) {
            ES3.Save<float> (SaveManager.instance.CurrentSlot + "_masterVolume", AudioManager.instance.volumeController.masterVolume, SaveManager.instance.settings);
        }
    }

    public void PlaySFX (string id) {
        FMOD.Studio.EventInstance playerState = FMODUnity.RuntimeManager.CreateInstance (soundDict[id].fmodEvent);
        playerState.start ();
    }
    public void PlayMusic (string id) {
        musicEmitter.Event = soundDict[id].fmodEvent;
        musicEmitter.Play ();
    }
    public void StopSFX () {
        sfxEmitter.Stop ();
    }
    public void StopMusic () {
        musicEmitter.Stop ();
    }

    void OnDisable () {
        StopMusic ();
        if (singleton) {
            instance = null;
        }
    }

    // Update is called once per frame
    void Update () {

    }
}