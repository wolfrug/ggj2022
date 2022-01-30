using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Doozy.Engine;
using UnityEngine;

public class SceneController : MonoBehaviour {
    public static SceneController instance;
    public List<SceneObject> sceneParents;
    public SceneObject startScene;
    public int currentScene;
    public bool saveOnSceneChange = true;

    void Awake () {
        if (instance == null) {
            instance = this;
        } else {
            Destroy (gameObject);
        }
        #if UNITY_EDITOR
        GetAllSceneObjects();
        #endif
    }

    public void Init () {
        if (!SaveManager.instance.IsNewGame) {
            startScene = GetScene (ES3.Load<int> (SaveManager.instance.CurrentSlot + "_currentScene", 0));
        }
        SetScene (startScene);
    }

    public void SetScene (SceneObject target) {
        SetScene (target, SceneEntrance.Default);
    }
    public SceneObject GetScene (int index) {
        if (index > 0 || index < sceneParents.Count) {
            return sceneParents[index];
        } else {
            Debug.LogWarning ("Could not get scene with index " + index + ", returning startscene");
            return startScene;
        }
    }

    public bool SetScene (SceneObject target, SceneEntrance entrance = SceneEntrance.Default) {
        if (target != null) {
            for (int i = 0; i < sceneParents.Count; i++) {
                if (sceneParents[i] == target) {
                    SetScene (i, entrance);
                    return true;
                }
            }
        }
        Debug.LogWarning ("No such scene in scene list! " + target);
        return false;
    }
    public bool SetScene (int index, SceneEntrance entrance = SceneEntrance.Default) {
        if (index > 0 || index < sceneParents.Count) {
            GameEventMessage.SendEvent ("HideScene");
            StartCoroutine (LoadScene (index, entrance));
            currentScene = index;
            return true;
        } else {
            Debug.LogWarning ("Tried to load invalid scene index: " + index);
            return false;
        }
    }

    void ChangeConfiner (SceneObject newScene) {
        if (newScene.m_sceneBoundingBox != null) {
            CameraManager.instance.SetBoundingBox (newScene);
        }
    }
    IEnumerator LoadScene (int index, SceneEntrance entrance = SceneEntrance.Default) {
        //yield return new WaitForSeconds (1f); // wait for doozy to hide the scene
        foreach (SceneObject go in sceneParents) {
            if (go != sceneParents[index]) {
                go.DeactivateSelf ();
            }
        }

        sceneParents[index].ActivateSelf ();
        // Move player!
        Vector3 newPos = sceneParents[index].GetEntrance (entrance).door.self.transform.position;
        Debug.Log ("Attempted move pos " + newPos);
        Debug.DrawLine (GameManager.instance.Player.transform.position, newPos);
        if (!GameManager.instance.Player.navMeshAgent.Warp (newPos)) { // attempt warp, otherwise force setpo
            GameManager.instance.Player.transform.position = newPos;
        }
        // Stop player movement
        GameManager.instance.Player.navMeshAgent.isStopped = true;
        ChangeConfiner (sceneParents[index]);
        yield return new WaitForSeconds (0.1f);
        GameEventMessage.SendEvent ("ShowScene");
        if (saveOnSceneChange) {
            GameManager.instance.SaveGame ();
        }
        GameManager.instance.Player.navMeshAgent.ResetPath ();
        GameManager.instance.Player.navMeshAgent.isStopped = false;
    }

    [NaughtyAttributes.Button]
    public void SetAllActive () { // For inits etc
        foreach (SceneObject go in sceneParents) {
            go.transform.position = transform.position;
            go.ActivateSelf ();
        }
    }

    [NaughtyAttributes.Button]
    void EditorSetScene () {
        foreach (SceneObject go in sceneParents) {
            if (go != sceneParents[currentScene]) {
                go.DeactivateSelf ();
            }
        }
        sceneParents[currentScene].ActivateSelf ();
    }

    [NaughtyAttributes.Button]
    void GetAllSceneObjects () {
        sceneParents.Clear ();
        foreach (SceneObject so in FindObjectsOfType<SceneObject> ()) {
            sceneParents.Add (so);
        }
    }

    public void SaveScene () {
        ES3.Save<int> (SaveManager.instance.CurrentSlot + "_currentScene", currentScene, SaveManager.instance.settings);
    }

    // Update is called once per frame
    void Update () {

    }
}