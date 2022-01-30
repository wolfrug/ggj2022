using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/*
Manages using Ink to retrieve strings for whatever reason. Will go to the knot, continue until it cannot any more, then return the string.
Cannot be used as an ersatz writer, will not deal with options, but otherwise should retrieve the text accurately.
*/

public class InkStringtableManager : MonoBehaviour {

    [Tooltip ("If set to not '', waits until it can then sets the string to the default knot")]
    public string m_startingKnot;
    public string m_lineBreakCharacter = "\n";

    [Tooltip ("Trims extra linebreaks etc from the string, especially useful for single-line things")]
    public bool m_trimStrings = true;
    [Tooltip ("Not obligatory, but can be used for ease of use.")]
    public TextMeshProUGUI m_textObject;
    // Start is called before the first frame update
    void Start () {
        if (m_textObject == null) {
            m_textObject = GetComponentInChildren<TextMeshProUGUI> ();
        }

        if (m_startingKnot != "" && m_textObject != null) {
            StartCoroutine (SelfInitializeWaiter ());
        }
    }

    IEnumerator SelfInitializeWaiter () {
        yield return new WaitUntil (() => InkWriter.main != null);
        yield return new WaitUntil (() => InkWriter.main.story != null);
        SetText (m_startingKnot);
    }

    public string GetKnot (string targetKnot) {
        string returnText = "";
        if (GameManager.instance.GameState != GameStates.NARRATIVE) {
            InkWriter.main.story.ChoosePathString (targetKnot);
            while (InkWriter.main.story.canContinue) {
                returnText += InkWriter.main.story.Continue ();
                returnText += m_lineBreakCharacter;
            }
        } else {
            Debug.LogWarning ("Tried to get stringtable knot " + targetKnot + " during narrative - cancelled!", gameObject);
        }
        return returnText;
    }
    public void SetText (string targetKnot) {
        if (GameManager.instance.GameState != GameStates.NARRATIVE) {
            if (m_textObject != null) {
                m_textObject.SetText (GetKnot (targetKnot));
            } else {
                Debug.LogWarning ("Attempted to set Ink stringtable text to knot " + targetKnot + "but no text object was assigned!", gameObject);
            }
        } else {
            Debug.LogWarning ("Tried to set stringtable text to knot " + targetKnot + " during narrative - cancelled!", gameObject);
        }
    }

    // Creates a string array of all the strings in a specific knot
    public string[] CreateStringArray (string targetKnot) {
        string returnText = "";
        List<string> returnArray = new List<string> { };
        if (GameManager.instance.GameState != GameStates.NARRATIVE) {
            InkWriter.main.story.ChoosePathString (targetKnot);
            while (InkWriter.main.story.canContinue) {
                returnText = InkWriter.main.story.Continue ();
                returnArray.Add (returnText);
            }
        } else {
            Debug.LogWarning ("Tried to get stringtable knot " + targetKnot + " during narrative - cancelled!", gameObject);
        }
        return returnArray.ToArray ();
    }

    // TEMPORARY THING JUST USED FOR THIS GAME, DELETE AS NECESSARY
    public void PlayStringArray (string targetKnot) {
        GameManager.instance.PlayWriterQueueFromKnot (targetKnot);
    }

}