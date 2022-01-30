using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SaveGameSlot {
    [SerializeField] public string m_slotName;
    [SerializeField] public string m_timeOfSave;
    [SerializeField] public float m_version;
    [SerializeField] public string m_game;
    [SerializeField] public int m_slotIndex;
    [SerializeField] public string[] m_savedKeys;
    public SaveGameSlot (string slotname, string timeofsave, float version, string game, int index, string[] keys) {
        m_slotName = slotname;
        m_timeOfSave = timeofsave;
        m_version = version;
        m_game = game;
        m_slotIndex = index;
        m_savedKeys = keys;
    }
}

public class SaveManager : MonoBehaviour {
    public static SaveManager instance;

    private ES3Settings m_settings;

    [SerializeField]
    private bool m_newGame = true;

    [SerializeField]
    private string m_currentSlot = "slot1";
    [SerializeField]
    private float m_version = 0.1f;
    [SerializeField]
    private float m_subversion = 0.01f; // changing this does not mess up savefiles!
    private string m_game = "GGJ2022";
    private string m_slotnameConvention = "Save ";
    [SerializeField]
    private string m_lastSlot = "";
    [SerializeField]
    private List<SaveGameSlot> m_allSlots = new List<SaveGameSlot> { };

    private int m_actualCurrentSlot;
    void Awake () {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad (gameObject);
            m_settings = new ES3Settings (ES3.Location.Cache);
            if (ES3.KeyExists (m_game + m_version + "_lastSlot")) {
                m_lastSlot = ES3.Load<string> (m_game + m_version + "_lastSlot");
            } else {
                m_lastSlot = "";
            }
            m_currentSlot = m_lastSlot;
            LoadAllSlots ();
            IsNewGame = m_allSlots.Count < 1 || m_lastSlot == "";
        } else {
            Destroy (gameObject);
        }
    }

    public bool IsNewGame {
        get {
            return m_newGame;
        }
        set {
            m_newGame = value;
            if (!value) {
                Debug.Log ("Saving slot " + m_currentSlot + " into key " + m_game + m_version + "_lastSlot");
                ES3.Save<string> (m_game + m_version + "_lastSlot", m_currentSlot, settings);
            }
        }
    }

    public string CurrentSlot {
        get {
            return m_game + m_currentSlot + "_" + m_version.ToString ();
        }
    }

    public float CurrentVersion {
        get {
            return m_version;
        }
    }
    public float CurrentSubVersion {
        get {
            return m_subversion;
        }
    }
    public string CurrentGame {
        get {
            return m_game;
        }
    }

    public ES3Settings settings {
        get {
            return m_settings;
        }
    }

    public List<SaveGameSlot> AllSlots {
        get {
            return m_allSlots;
        }
    }

    public void DeleteSlot (SaveGameSlot slot) {
        if (m_allSlots.Contains (slot)) {
            Debug.Log ("Deleting slot " + slot.m_slotName);
            foreach (string key in slot.m_savedKeys) {
                if (ES3.KeyExists (key)) {
                    ES3.DeleteKey (key);
                };
            }
            m_allSlots.Remove (slot);
        }
    }

    void LoadAllSlots () {
        if (ES3.KeyExists (m_game + "_AllSlots")) {
            m_allSlots = ES3.Load<List<SaveGameSlot>> (m_game + "_AllSlots", new List<SaveGameSlot> () { });
        } else {
            Debug.LogWarning ("Tried to load key " + m_game + "_AllSlots" + " but the key did not exist??");
        }
    }
    void SaveSlots () {
        ClearNonSavedSlots ();
        ES3.Save<List<SaveGameSlot>> (m_game + "_AllSlots", m_allSlots, settings);
        Debug.Log ("Saving key " + m_game + "_AllSlots");
    }

    [NaughtyAttributes.Button]
    void ClearNonSavedSlots () { // clear all slots that don't have any keys - we don't want to save those!
        List<SaveGameSlot> slotsToClear = new List<SaveGameSlot> { };
        foreach (SaveGameSlot slot in m_allSlots) {
            if (slot.m_savedKeys.Length == 0) {
                slotsToClear.Add (slot);
            }
        }
        m_allSlots.RemoveAll ((x) => x.m_savedKeys.Length == 0);
    }

    public SaveGameSlot StartNewSlot () {
        // Code for starting a new slot
        // name it by counting the number of slots in allslots and adding +1

        int index = 0;
        if (m_allSlots.Count > 0) {
            index = m_allSlots[m_allSlots.Count - 1].m_slotIndex + 1;
        }
        string newSlotName = m_slotnameConvention + index.ToString ();
        DateTime savedTime = DateTime.Now;
        SaveGameSlot newSlot = new SaveGameSlot ();
        newSlot.m_slotName = newSlotName;
        newSlot.m_timeOfSave = savedTime.ToString ("F");
        newSlot.m_slotIndex = index;
        newSlot.m_version = m_version;
        newSlot.m_game = m_game;
        newSlot.m_savedKeys = new string[] { };
        m_allSlots.Add (newSlot);
        m_currentSlot = newSlotName;
        IsNewGame = true;
        return newSlot;
    }

    public void LoadSlot (SaveGameSlot slot) {
        m_currentSlot = slot.m_slotName;
        m_actualCurrentSlot = GetSaveGameSlot (slot.m_slotName);
    }

    public void AddSlotKey (string newKey) {
        int targetSlotIndex = GetSaveGameSlot (m_currentSlot);
        if (targetSlotIndex == -1) {
            Debug.LogError ("No slot currently saved, somehow. Bad bug, bad!");
            StartNewSlot ();
            targetSlotIndex = 0;
        }
        SaveGameSlot targetSlot = AllSlots[targetSlotIndex];
        List<string> mutableKeyList = new List<string> { };
        if (targetSlot.m_savedKeys != null) {
            foreach (string key in targetSlot.m_savedKeys) {
                if (key == newKey) {
                    // already exists - we can skip
                    return;
                } else {
                    mutableKeyList.Add (key);
                }
            }
        };
        // Did not find it, add it to the mutable list
        mutableKeyList.Add (newKey);
        // Update the timing because why  not
        DateTime savedTime = DateTime.Now;
        string newTimeOfSave = savedTime.ToString ("F");
        // replace old list with new array
        targetSlot = new SaveGameSlot (targetSlot.m_slotName, newTimeOfSave, targetSlot.m_version, targetSlot.m_game, targetSlot.m_slotIndex, mutableKeyList.ToArray ());
        m_allSlots[targetSlotIndex] = targetSlot;

    }

    int GetSaveGameSlot (string targetName) {
        foreach (SaveGameSlot slot in m_allSlots) {
            if (slot.m_slotName == targetName) {
                return m_allSlots.IndexOf (slot);
            }
        }
        return -1;
    }

    public void SaveCache () {
        SaveSlots ();
        ES3.StoreCachedFile ();
    }
}