using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Animator))]
public class GenericAnimatorExtender : MonoBehaviour {
    // to be used with e.g. buttons etc for more advanced Animator control!
    public Animator animator;
    public string floatName = "";
    public string intName = "";
    // Start is called before the first frame update
    void Awake () {
        if (animator == null) {
            animator = GetComponent<Animator> ();
        }
    }
    public void SetBoolTrue (string boolName) {
        animator.SetBool (boolName, true);
    }
    public void SetBoolFalse (string boolName) {
        animator.SetBool (boolName, false);
    }
    public void SetFloatName (string newFloatName) {
        floatName = newFloatName;
    }
    public void SetIntName (string newIntName) {
        intName = newIntName;
    }
    public void SetFloat (float targetFloat) {
        animator.SetFloat (floatName, targetFloat);
    }
    public void SetInt (int targetInt) {
        animator.SetInteger (intName, targetInt);
    }

    // Update is called once per frame
    void Update () {

    }
}