using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public static CameraManager instance;
    public Camera m_mainCam;
    public CinemachineVirtualCamera m_mainVcam;
    public CinemachineVirtualCamera m_dialogueCam;

    public CinemachineVirtualCamera m_focusCam;

    void Awake () {
        if (instance == null) {
            instance = this;
        } else {
            Destroy (gameObject);
        }
    }

    // Start is called before the first frame update
    void Start () {
        GameManager.instance.GetState (GameStates.NARRATIVE).evtStart.AddListener ((a) => EnableDialogueCamera ());
        GameManager.instance.GetState (GameStates.GAME).evtStart.AddListener ((a) => EnableMainCam ());
    }

    public void SetBoundingBox (SceneObject newScene) {
        m_mainVcam.GetComponent<CinemachineConfiner2D> ().m_BoundingShape2D = newScene.m_sceneBoundingBox;
        m_mainVcam.GetComponent<CinemachineConfiner2D> ().InvalidateCache ();
        m_dialogueCam.GetComponent<CinemachineConfiner2D> ().m_BoundingShape2D = newScene.m_sceneBoundingBox;
        m_dialogueCam.GetComponent<CinemachineConfiner2D> ().InvalidateCache ();
        m_focusCam.GetComponent<CinemachineConfiner2D> ().m_BoundingShape2D = newScene.m_sceneBoundingBox;
        m_focusCam.GetComponent<CinemachineConfiner2D> ().InvalidateCache ();
    }
    public void SetFocusCamTarget (Transform target) {
        m_focusCam.m_Follow = target;
    }
    public void EnableDialogueCamera () {
        m_dialogueCam.Priority = 11;
        m_mainVcam.Priority = 0;
        m_focusCam.Priority = 0;
    }
    public void EnableMainCam () {
        m_dialogueCam.Priority = 0;
        m_mainVcam.Priority = 11;
        m_focusCam.Priority = 0;
    }
    public void EnableFocusCam () {
        m_dialogueCam.Priority = 0;
        m_mainVcam.Priority = 0;
        m_focusCam.Priority = 11;
    }

}