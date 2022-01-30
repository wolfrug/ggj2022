using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour {
    public SaveGameSlot m_data;
    public Button m_loadSaveButton;
    public Button m_deleteSaveButton;
    public TextMeshProUGUI m_slotNameText;
    public TextMeshProUGUI m_versionInfo;
    public TextMeshProUGUI m_dateInfo;

    [Tooltip ("Set to active if the save game version/game is invalid")]
    public GameObject m_invalidBox;

    public void UpdateSlot (SaveGameSlot newData) {
        m_data = newData;
        m_slotNameText.text = m_data.m_slotName;
        m_versionInfo.text = m_data.m_game + " " + m_data.m_version.ToString ();
        m_dateInfo.text = m_data.m_timeOfSave;
        m_loadSaveButton.onClick.RemoveAllListeners ();
        if (IsValid ()) {
            m_invalidBox?.SetActive (false);
            m_loadSaveButton.onClick.AddListener (() => SaveManager.instance.LoadSlot (m_data));
        } else {
            m_invalidBox?.SetActive (true);
        }
    }
    public bool IsValid () {
        return m_data.m_version == SaveManager.instance.CurrentVersion && m_data.m_game == SaveManager.instance.CurrentGame;

    }
}