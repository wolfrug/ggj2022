using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WriterAction {
    [TextArea]
    public string writeString;
    public float pauseUntilNext = 0.5f;
    public WriterStarted startedEvent;
    public WriterStopped finishedEvent;
}

public class TypeWriterQueue : MonoBehaviour {
    [NaughtyAttributes.ReorderableList]
    public WriterAction[] strings;
    public TypeWriter typeWriter;
    public bool playOnStart = true;
    public float waitBetweenStrings = 0.1f;

    public WriterStarted m_queueStartedEvent;
    public WriterStopped m_queueEndedEvent;
    private Coroutine queue;
    [SerializeField]
    private int index;
    // Start is called before the first frame update
    void Start () {
        if (playOnStart) {
            StartQueue (0);
        }
    }

    public void StartQueue (int startPoint) {
        if (queue == null) {
            Debug.Log ("Starting new typewriterqueue from scratch!", gameObject);
            queue = StartCoroutine (Queue (startPoint));
        } else {
            Debug.Log ("Starting new typewriterqueue, first cancelling the running one!", gameObject);
            StopQueue ();
            m_queueEndedEvent.Invoke (typeWriter);
            queue = StartCoroutine (Queue (startPoint));
        }
    }
    public void StopQueue () {
        if (queue != null) {
            StopCoroutine (queue);
            typeWriter.StopAllCoroutines ();
        }
    }
    public void SetQueue (WriterAction[] newQueue) {
        if (queue == null) {
            Debug.Log ("Creating new typewriterqueue!", gameObject);
            strings = newQueue;
        } else {
            Debug.Log ("Stopping queue before previous queue ended!", gameObject);
            StopQueue ();
            strings = newQueue;
            queue = null;
            m_queueEndedEvent.Invoke (typeWriter);
        }
    }
    IEnumerator Queue (int startPoint) {
        index = startPoint;
        m_queueStartedEvent.Invoke (typeWriter);
        while (index < strings.Length) {
            typeWriter.Write (strings[index].writeString);
            yield return null;
            strings[index].startedEvent.Invoke (typeWriter);
            yield return new WaitUntil (() => !typeWriter.isWriting_);
            yield return new WaitForSeconds (strings[index].pauseUntilNext);
            strings[index].finishedEvent.Invoke (typeWriter);
            index++;
            yield return new WaitForSeconds (waitBetweenStrings);
        }
        queue = null;
        m_queueEndedEvent.Invoke (typeWriter);
    }

    public static WriterAction[] CreateTypeWriterQueue (string[] stringArray, bool endWithEmpty = true, float defaultPause = 0.5f) {
        List<WriterAction> returnList = new List<WriterAction> { };
        foreach (string s in stringArray) {
            WriterAction newAction = new WriterAction {
                writeString = s,
                pauseUntilNext = defaultPause,
                startedEvent = new WriterStarted (),
                finishedEvent = new WriterStopped ()
            };
            returnList.Add (newAction);
        }
        if (endWithEmpty) {
            WriterAction newAction = new WriterAction {
                writeString = "",
                pauseUntilNext = defaultPause,
                startedEvent = new WriterStarted (),
                finishedEvent = new WriterStopped ()
            };
            returnList.Add (newAction);
        }
        if (returnList.Count > 0) {
            Debug.Log ("Created new TypeWriterQueue with length " + returnList.Count + " starting with " + returnList[0]);
        } else {
            Debug.LogWarning ("Attempted to create new TypeWriterQueue but failed");
        }
        return returnList.ToArray ();
    }

}