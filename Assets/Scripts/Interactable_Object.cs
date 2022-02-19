﻿using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Inventory;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ObjectActivatedEvent : UnityEvent<Interactable_Object, BasicAgent> { }

public enum Interactions {
    NONE = 0000,
    PICK_UP = 1000,
    DROP = 1100,
    LOOK_AT = 2000,
    ACTIVATE = 3000,
    OPEN = 3100, // alt to activate
    TALK = 3200, // alt to activate
    USE_ITEM = 3300,
    DEACTIVATE = 4000,
    CLOSE = 4100, // alt to deactivate
}
public class Interactable_Object : MonoBehaviour {
    public GenericClickable clickableController;
    public List<Interactions> interactions = new List<Interactions> { Interactions.NONE };
    public Interactions chosenInteraction = Interactions.NONE;
    public bool hasContextMenu = true;
    // if false then right-click context menu
    public bool leftClickInteract = true;
    public string inspectKnotName = "inspectTest";
    public bool inspectWithThoughtBubble = true; // if set to true, do a thoughtbubble of the knot instead of going
    public string activateKnotName = "";
    public Inventory.Item_UseItemSpot useItemSpot;
    public ItemData currentActiveUseItem;
    public string invalidUseItemKnot = "cannot_use_item";

    public InventoryController inventory;
    public bool disableInventoryIfEmpty = true; // just to make everything easier
    public string activateActionTrigger = "Interact";
    public string deactivateActionTrigger = "Interact";
    public float activateDistance = 1f;

    public bool canBeCarried = true;
    public bool interactableWhileCarried = false;
    public GameObject self;
    public Rigidbody rb;
    public Material[] normal_selected_materials;
    public Renderer targetRenderer;
    public bool active = false;
    public float condition = 1f;

    public ObjectActivatedEvent activateObjectEvent;
    public ObjectActivatedEvent openedObjectEvent;
    public ObjectActivatedEvent talkObjectEvent;
    public ObjectActivatedEvent sitObjectEvent;
    public ObjectActivatedEvent deactivateObjectEvent;
    // Start is called before the first frame update
    void Start () {
        if (self == null) {
            self = gameObject;
        }
        if (rb == null) {
            rb = GetComponent<Rigidbody> ();
        }
        if (hasContextMenu) {
            if (GenericContextMenu.GetMenuOfType (ContextMenuType.WORLD) != null) {
                GenericContextMenu.GetMenuOfType (ContextMenuType.WORLD).selectedOptionEvent.AddListener (ContextMenuAction);
            };
            if (GenericContextMenu.GetMenuOfType (ContextMenuType.WORLD_LEFT) != null) {
                GenericContextMenu.GetMenuOfType (ContextMenuType.WORLD_LEFT).selectedOptionEvent.AddListener (ContextMenuAction);
            };
            if (GenericContextMenu.GetMenuOfType (ContextMenuType.WORLD_RIGHT) != null) {
                GenericContextMenu.GetMenuOfType (ContextMenuType.WORLD_RIGHT).selectedOptionEvent.AddListener (ContextMenuAction);
            };
        }
        if (targetRenderer != null) {
            targetRenderer.enabled = false;
        }
        CheckInventoryEmpty ();
    }

    public void CheckInventoryEmpty () {
        if (inventory != null) {
            if (inventory.allItemBoxes.Count == 0) {
                if (disableInventoryIfEmpty) {
                    DisableInventory ();
                }
            }
        }
    }

    public void DisableInventory () {
        if (inventory != null) {
            inventory.Visible = false;
            if (interactions.Contains (Interactions.ACTIVATE)) { // Remove the interaction
                interactions.Remove (Interactions.ACTIVATE);
                if (chosenInteraction == Interactions.ACTIVATE && interactions.Count > 0) {
                    chosenInteraction = interactions[0];
                }
                if (interactions.Count == 1) { // Remove the context menu too :O :O
                    hasContextMenu = false;
                }
            }
        }
    }

    public void SetChosenInteraction (Interactions choice) {
        if (interactions.Contains (choice)) {
            chosenInteraction = choice;
        }
    }

    public void HoverEnter (GenericClickable clickable) {
        if (GameManager.instance.GameState == GameStates.GAME) {
            if (normal_selected_materials.Length > 0 && targetRenderer != null) {
                targetRenderer.material = normal_selected_materials[1];
            };
            if (targetRenderer != null) {
                targetRenderer.enabled = true;
            }
        };
        /*if (leftClickContextMenu && GameManager.instance.GameState == GameStates.GAME) {
            GameManager.instance.StopPlayerClickToMove (true);
        }*/
    }
    public void HoverExit (GenericClickable clickable) {
        if (GameManager.instance.GameState == GameStates.GAME) {
            if (normal_selected_materials.Length > 0 && targetRenderer != null) {
                targetRenderer.material = normal_selected_materials[0];
            };
            if (targetRenderer != null) {
                targetRenderer.enabled = false;
            }
        }
        /*if (leftClickContextMenu && GameManager.instance.GameState == GameStates.GAME) {
            GameManager.instance.StopPlayerClickToMove (false);
        }*/
    }

    public void RightClickOn (GenericClickable clickable) {
        if (GameManager.instance.GameState == GameStates.GAME) {
            if (hasContextMenu && !leftClickInteract) {
                GenericContextMenu.GetWorldMenuWithDirection ().PopulateDropDownDefaults (ConvertInteractionsToContextMenuEntries (), gameObject);
                //GameManager.instance.StopPlayerMovement (true);
            } else { // no context menu so we just do the selected action, whatever it is, if we can
                if (chosenInteraction != Interactions.NONE && !leftClickInteract) {
                    Action (chosenInteraction);
                }
            }
        };
    }
    public void LeftClickOn (GenericClickable clickable) {
        if (GameManager.instance.GameState == GameStates.GAME) {
            if (hasContextMenu && leftClickInteract) {
                GenericContextMenu.GetWorldMenuWithDirection ().PopulateDropDownDefaults (ConvertInteractionsToContextMenuEntries (), gameObject);
                //GameManager.instance.StopPlayerMovement (true);
            } else { // no context menu so we just do the selected action, whatever it is, if we can
                if (chosenInteraction != Interactions.NONE && leftClickInteract) {
                    Action (chosenInteraction);
                }
            }
        };
    }

    List<ContextMenuEntryType> ConvertInteractionsToContextMenuEntries () {
        List<ContextMenuEntryType> returnList = new List<ContextMenuEntryType> { };
        foreach (Interactions interaction in interactions) {
            switch (interaction) {
                case Interactions.PICK_UP:
                    {
                        if (!IsCarried ()) {
                            if (!active || interactableWhileCarried) {
                                returnList.Add (ContextMenuEntryType.PICK_UP);
                            };
                        };
                        break;
                    }
                case Interactions.DROP:
                    {
                        if (IsCarried ()) {
                            returnList.Add (ContextMenuEntryType.DROP);
                        };
                        break;
                    }
                case Interactions.LOOK_AT:
                    {
                        returnList.Add (ContextMenuEntryType.LOOK_AT);
                        break;
                    }
                case Interactions.ACTIVATE:
                    {
                        if (!active || !interactions.Contains (Interactions.DEACTIVATE)) { // If there is no deactivate, we can just keep activating
                            if (!IsCarried () || interactableWhileCarried) {
                                returnList.Add (ContextMenuEntryType.ACTIVATE);
                            };
                        };
                        break;
                    }
                case Interactions.DEACTIVATE:
                    {
                        if (active) {
                            if (!IsCarried () || interactableWhileCarried) {
                                returnList.Add (ContextMenuEntryType.DEACTIVATE);
                            };
                        };
                        break;
                    }
                case Interactions.OPEN:
                    {
                        if (!active || !interactions.Contains (Interactions.CLOSE)) { // If there is no close, we can just keep activating
                            if (!IsCarried () || interactableWhileCarried) {
                                returnList.Add (ContextMenuEntryType.OPEN);
                            };
                        };
                        break;
                    }
                case Interactions.TALK:
                    {
                        // If there is no close, we can just keep activating
                        if (!IsCarried () || interactableWhileCarried) {
                            returnList.Add (ContextMenuEntryType.TALK);
                        };
                        break;
                    }
                case Interactions.CLOSE:
                    {
                        if (active) {
                            if (!IsCarried () || interactableWhileCarried) {
                                returnList.Add (ContextMenuEntryType.CLOSE);
                            };
                        };
                        break;
                    }
            }
        }
        return returnList;
    }
    public Interactions ConvertContextMenuEntryToInteraction (ContextMenuEntryType type) {
        Interactions returnInteraction = Interactions.NONE;
        switch (type) {
            case ContextMenuEntryType.PICK_UP:
                {
                    returnInteraction = Interactions.PICK_UP;
                    break;
                }
            case ContextMenuEntryType.DROP:
                {
                    returnInteraction = Interactions.DROP;
                    break;
                }
            case ContextMenuEntryType.LOOK_AT:
                {
                    returnInteraction = Interactions.LOOK_AT;
                    break;
                }
            case ContextMenuEntryType.ACTIVATE:
                {
                    returnInteraction = Interactions.ACTIVATE;
                    break;
                }
            case ContextMenuEntryType.DEACTIVATE:
                {
                    returnInteraction = Interactions.DEACTIVATE;
                    break;
                }
            case ContextMenuEntryType.OPEN: // alts
                {
                    returnInteraction = Interactions.OPEN;
                    break;
                }
            case ContextMenuEntryType.CLOSE: // alts
                {
                    returnInteraction = Interactions.CLOSE;
                    break;
                }
            case ContextMenuEntryType.TALK: // alts
                {
                    returnInteraction = Interactions.TALK;
                    break;
                }
            case ContextMenuEntryType.USE_ITEM: // alts
                {
                    returnInteraction = Interactions.USE_ITEM;
                    break;
                }
        }
        return returnInteraction;
    }

    public void TriggerActivate (BasicAgent agent, GenericActivate self) {
        Interact (agent, chosenInteraction);
    }
    public void Action (Interactions interaction) {
        switch (interaction) {
            case Interactions.PICK_UP:
                {
                    // Pick up actions
                    if (Vector3.Distance (GameManager.instance.Player.transform.position, transform.position) <= activateDistance) {
                        Interact (GameManager.instance.Player, Interactions.PICK_UP);
                    } else {
                        GameManager.instance.Player.StartAutoTask (this, ContextMenuEntryType.PICK_UP, true);
                    }
                    break;
                }
            case Interactions.DROP:
                {
                    // Drop actions
                    if (IsCarried (true)) {
                        Interact (GameManager.instance.Player, Interactions.DROP);
                    };
                    break;
                }
            case Interactions.LOOK_AT:
                {
                    if (inspectKnotName != "" && GameManager.instance.GameState != GameStates.NARRATIVE) {
                        if (!inspectWithThoughtBubble) {
                            InkWriter.main.GoToKnot (inspectKnotName);
                        } else {
                            GameManager.instance.PlayWriterQueueFromKnot (inspectKnotName);
                        }
                    }
                    break;
                }
            case Interactions.ACTIVATE:
                {
                    // Activate actions
                    if (Vector3.Distance (GameManager.instance.Player.transform.position, transform.position) <= activateDistance) {
                        Interact (GameManager.instance.Player, Interactions.ACTIVATE);
                    } else {
                        GameManager.instance.Player.StartAutoTask (this, ContextMenuEntryType.ACTIVATE, true);
                    }
                    break;
                }
            case Interactions.DEACTIVATE:
                {
                    // Deactivate actions
                    if (Vector3.Distance (GameManager.instance.Player.transform.position, transform.position) <= activateDistance) {
                        Interact (GameManager.instance.Player, Interactions.DEACTIVATE);
                    } else {
                        GameManager.instance.Player.StartAutoTask (this, ContextMenuEntryType.DEACTIVATE, true);
                    }
                    break;
                }
            case Interactions.OPEN:
                {
                    // Activate actions
                    if (Vector3.Distance (GameManager.instance.Player.transform.position, transform.position) <= activateDistance) {
                        Interact (GameManager.instance.Player, Interactions.OPEN);
                    } else {
                        GameManager.instance.Player.StartAutoTask (this, ContextMenuEntryType.OPEN, true);
                    }
                    break;
                }
            case Interactions.CLOSE:
                {
                    // Deactivate actions
                    if (Vector3.Distance (GameManager.instance.Player.transform.position, transform.position) <= activateDistance) {
                        Interact (GameManager.instance.Player, Interactions.CLOSE);
                    } else {
                        GameManager.instance.Player.StartAutoTask (this, ContextMenuEntryType.CLOSE, true);
                    }
                    break;
                }
            case Interactions.TALK:
                {
                    // Activate actions
                    if (Vector3.Distance (GameManager.instance.Player.transform.position, transform.position) <= activateDistance) {
                        Interact (GameManager.instance.Player, Interactions.TALK);
                    } else {
                        GameManager.instance.Player.StartAutoTask (this, ContextMenuEntryType.TALK, true);
                    }
                    break;
                }
            case Interactions.USE_ITEM:
                {
                    // Activate actions
                    if (Vector3.Distance (GameManager.instance.Player.transform.position, transform.position) <= activateDistance) {
                        Interact (GameManager.instance.Player, Interactions.USE_ITEM);
                    } else {
                        GameManager.instance.Player.StartAutoTask (this, ContextMenuEntryType.USE_ITEM, true);
                    }
                    break;
                }
        }
    }
    public void ContextMenuAction (ContextMenuEntryType actionType, GameObject target) {
        if (target == gameObject) {
            if (interactions.Contains (ConvertContextMenuEntryToInteraction (actionType))) {
                Interactions interaction = ConvertContextMenuEntryToInteraction (actionType);
                Action (interaction);
            }
        }
    }

    public void Interact (BasicAgent agent, Interactions interaction) {
        if (interactions.Contains (interaction)) {
            switch (interaction) {
                case Interactions.PICK_UP:
                    {
                        PickUpAction (agent);
                        break;
                    }
                case Interactions.DROP:
                    {
                        DropAction (agent);
                        break;
                    }
                case Interactions.ACTIVATE:
                    {
                        ActivateAction (agent, Interactions.ACTIVATE);
                        break;
                    }
                case Interactions.DEACTIVATE:
                    {
                        DeactivateAction (agent);
                        break;
                    }
                case Interactions.OPEN:
                    {
                        ActivateAction (agent, Interactions.OPEN);
                        break;
                    }
                case Interactions.CLOSE:
                    {
                        DeactivateAction (agent);
                        break;
                    }
                case Interactions.TALK:
                    {
                        ActivateAction (agent, Interactions.TALK);
                        break;
                    }
                case Interactions.USE_ITEM:
                    {
                        UseItemAction (agent);
                        break;
                    }
            }
        };
    }
    void PickUpAction (BasicAgent agent) {

        if (agent.StartCarry (1.4f)) {
            if (rb != null) {
                rb.isKinematic = true;
            }
            agent.SetCarriedObject (this);
            Sequence smoothMove = DOTween.Sequence ();
            smoothMove.AppendInterval (0.9f);
            smoothMove.AppendCallback (() => self.transform.SetParent (agent.carryParent, true));
            smoothMove.Append (self.transform.DOLocalMove (Vector3.zero, 0.5f));
            smoothMove.Append (self.transform.DOLocalMove (Vector3.zero, 0f));
            //self.transform.DOMove (agent.carryParent.position, 0.1f);
            //self.transform.localPosition = Vector3.zero;
            SetChosenInteraction (Interactions.DROP);
        };
    }
    void DropAction (BasicAgent agent) {
        if (agent.StopCarry (1f)) {
            self.transform.parent = null;
            SetChosenInteraction (Interactions.PICK_UP);
            if (rb != null) {
                rb.isKinematic = false;
            }
            agent.SetCarriedObject (null);
        };
    }

    void ActivateAction (BasicAgent agent, Interactions type) {
        if ((type == Interactions.ACTIVATE && interactions.Contains (Interactions.DEACTIVATE)) || (type == Interactions.OPEN && interactions.Contains (Interactions.CLOSE))) { // We only set this if we need to follow it
            active = true;
        };
        switch (type) {
            case Interactions.ACTIVATE:
                {
                    activateObjectEvent.Invoke (this, agent);
                    break;
                }
            case Interactions.TALK:
                {
                    talkObjectEvent.Invoke (this, agent);
                    break;
                }
            case Interactions.OPEN:
                {
                    openedObjectEvent.Invoke (this, agent);
                    break;
                }
        }

        agent.ActivateAction (activateActionTrigger, 0.5f, true);
        if (activateKnotName != "") {
            InkWriter.main.GoToKnot (activateKnotName);
        }
    }

    void DeactivateAction (BasicAgent agent) {
        if (active) {
            active = false;
            deactivateObjectEvent.Invoke (this, agent);
            agent.ActivateAction (deactivateActionTrigger, 0.5f, true);
        };
    }

    public void StartUseItemAction (ItemData data, int amount) { // For use with events externally
        Debug.Log ("Starting use item action!");
        currentActiveUseItem = data;
        Action (Interactions.USE_ITEM);
    }
    void UseItemAction (BasicAgent agent) {
        Debug.Log ("Use item action successfully completed!");
        agent.ActivateAction (activateActionTrigger, 0.5f, true);
        // So the way we do it is by seeing what the -description- is.
        string description = useItemSpot.GetDefinition (currentActiveUseItem).m_description;
        if (description == "INVALID") { // There's nothing
            GameManager.instance.PlayWriterQueueFromKnot (invalidUseItemKnot);
        } else { // Else we run the knot in the description!
            InkWriter.main.GoToKnot (description);
        }

    }
    public void SetInternalActive (bool set) {
        active = set;
    }
    bool IsCarried (bool byPlayer = true) {
        return false;
        if (canBeCarried) {
            if (transform.parent != null) {
                if (transform.parent.root == GameManager.instance.Player.transform && byPlayer) {
                    return true;
                } else {
                    return true;
                }
            }
        };
        return false;
    }

    // Update is called once per frame
    void Update () {

    }
}