using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour {

    public string m_taskName = "task1_";
    public InkStringtableManager m_namestringtableManager;
    public Button m_clickButton;
    public InkVariableListener[] m_statevariableListeners;
    // Start is called before the first frame update
    void Awake () {
        m_namestringtableManager.m_startingKnot = m_taskName + "name";
        m_clickButton.onClick.AddListener (() => GameManager.instance.PlayWriterQueueFromKnot (m_taskName + "description"));
        foreach (InkVariableListener listener in m_statevariableListeners) {
            listener.m_inkVariable = m_taskName + "state";
        }
    }

    public void PlayVictorySound () {
        AudioManager.instance.PlaySFX ("UI_Success");
    }

    [NaughtyAttributes.Button]
    void FillInEditor () {
        m_namestringtableManager.m_startingKnot = m_taskName + "name";
        m_clickButton.onClick.AddListener (() => GameManager.instance.PlayWriterQueueFromKnot (m_taskName + "description"));
        foreach (InkVariableListener listener in m_statevariableListeners) {
            listener.m_inkVariable = m_taskName + "state";
        }
    }

}