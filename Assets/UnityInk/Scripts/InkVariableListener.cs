using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InkVariableEvent : UnityEvent<InkVariableListener, bool> { }
public enum InkValueExactness {
    Exactly,
    Less,
    More,
    This_or_less,
    This_or_more,
    Between,
}
public class InkVariableListener : MonoBehaviour {
    public string m_inkVariable;
    [Tooltip ("If set to true, the inkvariable is instead referring to a knot - there is no observer for this mode though!")]
    public bool m_useKnot = false;
    public int m_targetValue = 1;
    [Tooltip ("If using 'between', targetValue is the minimum, betweenMax is the maximum")]
    public int m_betweenMax = 1;
    public InkValueExactness m_exactness = InkValueExactness.Exactly;
    public bool m_runOnceOnStart = true; // Runs on start
    public bool m_createListener = false; // creates a listener and continually listens

    public InkVariableEvent m_eventSuccess;
    public InkVariableEvent m_eventFail;

    void Start () {
        if (m_inkVariable == "") {
            Debug.LogWarning ("No variable set for listener, cannot initialize!", gameObject);
            return;
        }
        StartCoroutine (SelfInitializeWaiter ());
    }

    public void Evaluate () {
        // Evaluates the ink variable!
        int currentValue = -1;
        if (m_useKnot) {
            currentValue = InkWriter.main.story.state.VisitCountAtPathString (m_inkVariable);
        } else {
            currentValue = (int) InkWriter.main.story.variablesState[(m_inkVariable)];
        };
        bool evaluateSuccess = false;
        switch (m_exactness) {
            case InkValueExactness.Exactly:
                {
                    if (currentValue == m_targetValue) {
                        evaluateSuccess = true;
                    }
                    break;
                }
            case InkValueExactness.Less:
                {
                    if (currentValue < m_targetValue) {
                        evaluateSuccess = true;
                    }
                    break;
                }
            case InkValueExactness.More:
                {
                    if (currentValue > m_targetValue) {
                        evaluateSuccess = true;
                    }
                    break;
                }
            case InkValueExactness.This_or_less:
                {
                    if (currentValue <= m_targetValue) {
                        evaluateSuccess = true;
                    }
                    break;
                }
            case InkValueExactness.This_or_more:
                {
                    if (currentValue >= m_targetValue) {
                        evaluateSuccess = true;
                    }
                    break;
                }
            case InkValueExactness.Between:
                {
                    if (currentValue >= m_targetValue && currentValue <= m_betweenMax) {
                        evaluateSuccess = true;
                    }
                    break;
                }

            default:
                break;
        }
        if (evaluateSuccess) { // success!
            m_eventSuccess.Invoke (this, true);
            Debug.Log ("Ink variable listener evaluated a <color=green>success</color> for ink variable " + m_inkVariable + "(Value was " + m_exactness + " to target (" + currentValue + ", " + m_targetValue + "))");
        } else {
            m_eventFail.Invoke (this, false);
            Debug.Log ("Ink variable listener evaluated a <color=red>fail</color> for ink variable " + m_inkVariable + "(Value was " + m_exactness + " to target (" + currentValue + ", " + m_targetValue + "))");
        }
    }

    IEnumerator SelfInitializeWaiter () {
        yield return new WaitUntil (() => InkWriter.main != null);
        yield return new WaitUntil (() => InkWriter.main.story != null);
        yield return new WaitUntil (() => InkWriter.storyLoaded);
        if (m_runOnceOnStart) {
            Evaluate ();
        };
        if (m_createListener && !m_useKnot) {
            InkWriter.main.story.ObserveVariable ((m_inkVariable), (string varName, object newValue) => {
                EventListener (varName, (int) newValue);
            });
        } else if (m_useKnot && m_createListener) {
            Debug.LogWarning ("Cannot create a direct listener for knot visits - will instead Evaluate whenever the narrative ends!");
            InkWriter.main.onWriterClose.AddListener ((a) => Evaluate ());
            // And also, just for this, when the gamemanager's queue thing ends
            GameManager.instance.m_thoughtWriter.m_queueStartedEvent.AddListener ((a) => Evaluate ());
        }
    }

    public void EventListener (string tag, int valuechange) { // from the tag listener
        // Debug.Log("New value from EventListener: tag=" + tag + " value=" + valuechange);
        Evaluate ();
    }

#if UNITY_EDITOR
    void OnDrawGizmos () {
        string exactnessSymbol = "=";
        string isKnot = "";
        if (m_useKnot) { isKnot = "(Knot) "; };
        string listenerActive = "";
        if (m_createListener) {
            listenerActive = "\n(Has Listener)";
        }
        switch (m_exactness) {
            case InkValueExactness.Exactly:
                {
                    exactnessSymbol = "==";
                    break;
                }
            case InkValueExactness.Less:
                {
                    exactnessSymbol = "<";
                    break;
                }
            case InkValueExactness.More:
                {
                    exactnessSymbol = ">";
                    break;
                }
            case InkValueExactness.This_or_less:
                {
                    exactnessSymbol = "<=";
                    break;
                }
            case InkValueExactness.This_or_more:
                {
                    exactnessSymbol = ">=";
                    break;
                }

            default:
                break;
        }
        Handles.Label (transform.position, "Listens to: " + isKnot + "'" + m_inkVariable + "'" + exactnessSymbol + m_targetValue + listenerActive);
    }
#endif
}