using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class OnGenericPoolChanged : UnityEvent<float> { }

public class GenericUIPool : MonoBehaviour {
    // Start is called before the first frame update
    [SerializeField]
    private float m_poolCurrent;
    public Vector2 m_poolMinMax;

    [Tooltip ("Casts all m_pool values into int?")]
    public bool m_isInteger = true;
    public bool m_startWithEventInvoke = true;

    [Tooltip ("By default, 0 = current pool, 1 = max pool, 2 = min pool, 3 = default min, 4 = default max")]
    public string m_textformat = "{0}/{1}";
    [Tooltip ("By default, 0 = change value, 1 = optional + if positive, 2 = current pool")]
    public string m_changeTextFormat = "{1}{0}";

    [Tooltip ("Set to true to use the default max for the current percentage")]
    public bool m_useDefaultForPercentage = false;
    public List<Selectable> m_setActivePlus = new List<Selectable> { };
    public List<Selectable> m_setActiveMinus = new List<Selectable> { };

    public OnGenericPoolChanged m_changedEvent;
    public OnGenericPoolChanged m_currentValueEvent;
    public OnGenericPoolChanged m_currentValuePercentageEvent;

    private float m_defaultStartPool;
    private Vector2 m_defaultPoolMinMax;
    private string m_defaultValueFormat;
    private string m_defaultChangeValueFormat;
    private bool m_defaultIsInteger;

    private float m_lastChange = 0f;

    void Start () {
        UpdateDefaultsToCurrent ();
        m_changedEvent.AddListener ((arg0) => UpdateSelectableWatchers ());
        if (m_startWithEventInvoke) {
            InvokeEvents ();
        };
    }

    public void ChangePool (float change) {
        ChangePoolR (change);
    }
    public void ChangePool (int change) {
        ChangePoolR ((float) change);
    }
    public void ChangePoolInverted (float change) {
        ChangePoolInvertedR (change);
    }
    public void ChangePoolInverted (int change) {
        ChangePoolInvertedR ((float) change);
    }
    public float ChangePoolR (float change) {
        float newValue = Mathf.Clamp (Value + change, m_poolMinMax.x, m_poolMinMax.y);
        float eventValue = change;
        if (newValue == Value + change) { // It worked out ok
            eventValue = change;
        } else { // It didn't
            eventValue = newValue - Value;
        }
        Value = newValue;
        //Debug.Log ("Pool changed: " + eventValue);
        m_lastChange = eventValue;
        m_changedEvent.Invoke (eventValue);
        return eventValue;
    }
    public float ChangePoolR (int change) {
        return (int) ChangePoolR ((float) change);
    }
    public float ChangePoolInvertedR (float change) {
        change *= -1f;
        float newValue = Mathf.Clamp (Value + change, m_poolMinMax.x, m_poolMinMax.y);
        float eventValue = change;
        if (newValue == Value + change) { // It worked out ok
            eventValue = change;
        } else { // It didn't
            eventValue = newValue - Value;
        }
        Value = newValue;
        Debug.Log ("Pool changed (inverted): " + eventValue * -1);
        m_lastChange = eventValue;
        m_changedEvent.Invoke (eventValue);
        return eventValue;
    }
    public float ChangePoolInvertedR (int change) {
        return (int) ChangePoolInvertedR ((float) change);
    }
    public void SetPool (float newValue) {
        float newSetValue = Mathf.Clamp (newValue, m_poolMinMax.x, m_poolMinMax.y);
        float eventValue = newValue;
        if (newValue == newSetValue) { // It worked out ok
            eventValue = Value - newValue;
        } else { // It didn't
            eventValue = newSetValue - Value;
        }
        Value = newSetValue;
        m_lastChange = eventValue;
        m_changedEvent.Invoke (eventValue);
    }
    public void SetPool (int newValue) {
        SetPool ((float) newValue);
    }

    public void SetMinMax (Vector2 newMinMax) {
        m_poolMinMax = newMinMax;
    }
    public void SetTextFormat (string newFormat) {
        m_textformat = newFormat;
    }
    public void SetChangeTextFormat (string newFormat) {
        m_changeTextFormat = newFormat;
    }

    public void UpdateTextWithValue (Text textTarget) { // Updates the text to the value given
        textTarget.text = string.Format (m_textformat, Value, m_poolMinMax.y, m_poolMinMax.x, m_defaultPoolMinMax.x, m_defaultPoolMinMax.y);
    }
    public void UpdateTextWithValue (TextMeshProUGUI textTarget) {
        textTarget.text = string.Format (m_textformat, Value, m_poolMinMax.y, m_poolMinMax.x, m_defaultPoolMinMax.x, m_defaultPoolMinMax.y);
    }
    public void UpdateTextWithLastChange (Text textTarget) { // Updates the text with the latest change value
        if (textTarget != null) {
            string plus = "";
            if (m_lastChange > 0) {
                plus = "+";
            }
            textTarget.text = string.Format (m_changeTextFormat, m_lastChange, plus, Value);
        } else {
            Debug.LogWarning ("Attempted to update UI Pool Last Change with null textTarget!", gameObject);
        }
    }
    public void UpdateTextWithLastChange (TextMeshProUGUI textTarget) { // Updates the text with the latest change value
        if (textTarget != null) {
            string plus = "";
            if (m_lastChange > 0) {
                plus = "+";
            }
            textTarget.text = string.Format (m_changeTextFormat, m_lastChange, plus, Value);
        } else {
            Debug.LogWarning ("Attempted to update UI Pool Last Change with null textTarget!", gameObject);
        }
    }

    public void SetSelectableActivePlus (Selectable target) { // Sets the selectable as active if the pool is < max
        if (Value < m_poolMinMax.y) {
            target.interactable = true;
        } else {
            target.interactable = false;
        }
    }
    public void SetSelectableActiveMinus (Selectable target) { // Sets the selectable as active if the pool is > min
        if (Value > m_poolMinMax.x) {
            target.interactable = true;
        } else {
            target.interactable = false;
        }
    }
    public void SetSelectableActiveMax (Selectable target) { // Sets target interactable when the pool is at max
        if (Value == m_poolMinMax.y) {
            target.interactable = true;
        } else {
            target.interactable = false;
        }
    }
    public void SetSelectableActiveMin (Selectable target) { // Sets target interactable when the pool is at min
        if (Value == m_poolMinMax.x) {
            target.interactable = true;
        } else {
            target.interactable = false;
        }
    }

    public string GetText () {
        if (m_isInteger) {
            return string.Format (m_textformat, Value, (int) m_poolMinMax.y, (int) m_poolMinMax.x);
        } else {
            return string.Format (m_textformat, Value, m_poolMinMax.y, m_poolMinMax.x);
        }
    }
    public float Value {
        get {
            if (m_isInteger) {
                return (int) m_poolCurrent;
            } else {
                return m_poolCurrent;
            };
        }
        set {
            m_poolCurrent = value;
            m_currentValueEvent.Invoke (m_poolCurrent);
            m_currentValuePercentageEvent.Invoke (Percentage);
        }
    }
    public float Percentage {
        get {
            return m_poolCurrent / (m_useDefaultForPercentage ? m_defaultPoolMinMax.y : m_poolMinMax.y);
        }
    }

    [NaughtyAttributes.Button]
    public void Reset () {
        m_poolCurrent = m_defaultStartPool;
        m_poolMinMax = m_defaultPoolMinMax;
        m_textformat = m_defaultValueFormat;
        m_changeTextFormat = m_defaultChangeValueFormat;
        if (m_startWithEventInvoke) InvokeEvents ();
    }

    [NaughtyAttributes.Button]
    void InvokeEvents () {
        m_changedEvent.Invoke (0);
        m_currentValueEvent.Invoke (Value);
        m_currentValuePercentageEvent.Invoke (m_poolCurrent / m_poolMinMax.y);
    }

    public void UpdateDefaultsToCurrent () {
        m_defaultStartPool = m_poolCurrent;
        m_defaultPoolMinMax = m_poolMinMax;
        m_defaultChangeValueFormat = m_changeTextFormat;
        m_defaultValueFormat = m_textformat;
        m_defaultIsInteger = m_isInteger;
    }
    void UpdateSelectableWatchers () { // Watching them selectables yo
        if (m_setActiveMinus.Count > 0) {
            foreach (Selectable sel in m_setActiveMinus) {
                SetSelectableActiveMinus (sel);
            }
        }
        if (m_setActivePlus.Count > 0) {
            foreach (Selectable sel in m_setActivePlus) {
                SetSelectableActivePlus (sel);
            }
        }
    }
}