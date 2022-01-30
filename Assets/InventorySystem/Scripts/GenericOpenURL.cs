using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericOpenURL : MonoBehaviour {
    // Start is called before the first frame update,
    public void OpenURL (string urlTarget) {
        Application.OpenURL (urlTarget);
    }
}