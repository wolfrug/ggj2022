using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour {
    // Start is called before the first frame update
    public Button continueGameButton;
    public Button newGameButton;
    public Button loadGame;
    public Button quitGame;
    public GameObject loadGamePanel;
    public Transform slotPrefabParent;
    public GameObject slotPrefab;
    public string startGameScene;
    public string continueGameScene;
    void Start () {
        bool hasSaved = false;
        if (SaveManager.instance != null) {
            hasSaved = !SaveManager.instance.IsNewGame;
        };
        if (hasSaved && continueGameButton != null) { // if we've saved and can continue, make the continue button active, otherwise deactivate
            continueGameButton.gameObject.SetActive (true);
            continueGameButton.onClick.AddListener (ContinueGame);
        } else {
            if (continueGameButton != null) {
                continueGameButton.gameObject.SetActive (false);
                if (loadGame != null) { // also set the load game button to deactive
                    loadGame.gameObject.SetActive (false);
                }
            };
        }
        if (newGameButton != null) {
            if (hasSaved) {
                if (SaveManager.instance.AllSlots.Count >= 5) {
                    newGameButton.gameObject.SetActive (false); // can't have more than 5 active save slots
                }
            }
            newGameButton.onClick.AddListener (NewGame);
        };
        if (loadGame != null) {
            loadGame.onClick.AddListener (LoadGame);
        };
        if (quitGame != null) {
            quitGame.onClick.AddListener (QuitGame);
        };
    }

    void ContinueGame () {
        SceneManager.LoadScene (continueGameScene);
    }

    void NewGame () {
        if (SaveManager.instance != null) {
            SaveManager.instance.StartNewSlot ();
        };
        SceneManager.LoadScene (startGameScene);
    }
    void ResetGame () {
        if (SaveManager.instance != null) {
            SaveManager.instance.IsNewGame = true;
        };
        SceneManager.LoadScene (startGameScene);
    }

    void LoadGame () {
        loadGamePanel.SetActive (true);
        // Clear any previous game slots
        foreach (Transform child in slotPrefabParent) {
            Destroy (child.gameObject);
        }
        // Create and update a new prefab for every existing slot
        foreach (SaveGameSlot slot in SaveManager.instance.AllSlots) {
            GameObject newSlotPrefab = Instantiate (slotPrefab, slotPrefabParent);
            SaveSlot slotComponent = newSlotPrefab.GetComponent<SaveSlot> ();
            slotComponent.UpdateSlot (slot);
            if (slotComponent.IsValid ()) {
                slotComponent.m_loadSaveButton.onClick.AddListener (ContinueGame);
            };
            slotComponent.m_deleteSaveButton.onClick.AddListener (() => DeleteSaveSlot (slotComponent));
        }
    }
    void DeleteSaveSlot (SaveSlot slot) {
        SaveManager.instance.DeleteSlot (slot.m_data);
        Destroy (slot.gameObject);
        if (SaveManager.instance.AllSlots.Count < 5) { // set the new game button to active again
            newGameButton.gameObject.SetActive (true);
        }
        SaveManager.instance.SaveCache ();
    }

    void QuitGame () {
        Application.Quit ();
    }
    public void InvokeStartButton () {
        continueGameButton.onClick.Invoke ();
    }

    public void LoadScene (string sceneName) {
        SceneManager.LoadScene (sceneName);
    }
    // Update is called once per frame
    void Update () {

    }
}