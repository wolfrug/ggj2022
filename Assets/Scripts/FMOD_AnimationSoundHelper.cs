using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class FMOD_AnimationSoundHelper : MonoBehaviour {
    public string soundToPlay = "";
    public StudioEventEmitter eventEmitter;
    // Start is called before the first frame update

    public void PlaySound () {
        if (eventEmitter == null) {
            AudioManager.instance.PlaySFX (soundToPlay);
        } else {
            eventEmitter.Play ();
        }
    }
}