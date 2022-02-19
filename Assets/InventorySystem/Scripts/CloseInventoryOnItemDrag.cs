using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Inventory {

    public class CloseInventoryOnItemDrag : MonoBehaviour { // Runs the chosen action when the player drags an item into it

        public GenericClickable[] m_targetZones;
        public InventoryController[] m_targetInventories; // Leave empty to have it listen to -all- inventories
        public InventoryItemRemoved m_action; // when entered with dragging item
        public DragEnded m_endAction; // when dragging stops

        public bool m_itemIsBeingDragged = false;
        private bool m_actionHasTriggered = false;
        public Item_DragAndDrop m_currentDragTarget = null;
        public Item_DragAndDrop m_currentFakeItem = null;
        public Transform m_currentDragPoint = null;
        public Canvas m_fakeCanvas = null; // Used to house the fake item - set it up to have similar properties as the parent inventory canvases!

        // Start is called before the first frame update
        void Start () {
            foreach (GenericClickable clickable in m_targetZones) {
                clickable.onMouseEnterEvent.AddListener (OnItemDragged);
            }
            foreach (GenericClickable clickable in m_targetZones) {
                //clickable.onMouseExitEvent.AddListener ((a) => StopDrag (null)); // if we want it...
            }
            if (m_targetInventories.Length < 1) {
                m_targetInventories = InventoryController.GetAllInventories (InventoryType.NONE, null, false).ToArray ();
            }
            if (m_targetInventories.Length > 0) {
                foreach (InventoryController inv in m_targetInventories) {
                    inv.itemDragStartedEvent.AddListener (StartDrag);
                    inv.itemDragEndedEvent.AddListener (StopDrag);

                }
            }
        }

        void StartDrag (Item_DragAndDrop itemdragged) {
            m_itemIsBeingDragged = true;
            m_currentDragTarget = itemdragged;
        }
        void StopDrag (Item_DragAndDrop itemdragged) {
            if (m_actionHasTriggered) {
                m_itemIsBeingDragged = false;
                m_actionHasTriggered = false;
                //  DespawnFakeItem ();
                m_currentDragTarget = null;
                m_endAction.Invoke (itemdragged);
            };
        }
        void OnItemDragged (GenericClickable clickable) { // Mouse has entered!
            if (m_itemIsBeingDragged) {
                //  SpawnFakeItem (m_currentDragTarget);
                m_action.Invoke (null, m_currentDragTarget);
                m_actionHasTriggered = true;
            };
        }
        void SpawnFakeItem (Item_DragAndDrop origin) { // Spawns a fake item
            if (origin != null) {
                GameObject newFakeItem = Instantiate (origin.gameObject, m_fakeCanvas.transform);
                m_currentFakeItem = newFakeItem.GetComponent<Item_DragAndDrop> ();
                m_currentFakeItem.targetCanvas = m_fakeCanvas;
                m_currentFakeItem.StartDrag ();
                m_currentDragPoint = origin.DragPoint;
            } else {
                Debug.LogError ("Could not spawn fake item - origin is null??");
            }
        }
        void DespawnFakeItem () {
            m_itemIsBeingDragged = false;
            if (m_currentFakeItem != null) {
                Destroy (m_currentFakeItem.gameObject);
                m_currentFakeItem = null;
                m_currentDragPoint = null;
            }
        }

        // Update is called once per frame
        void Update () {
            if (m_currentFakeItem != null && m_currentDragTarget != null) {
                //m_currentFakeItem.targetTransform.position = m_currentDragTarget.DragPoint.position;
            }
        }
    }
}