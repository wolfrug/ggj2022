using System.Collections;
using System.Collections.Generic;
using Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace Inventory {

    [System.Serializable]
    public class ItemCountChanged : UnityEvent<int> { }

    [System.Serializable]
    public class InkItemCountWatcherInstance {
        public ItemData m_data;
        public ItemGameTrait m_trait;
        public ItemCountChanged m_changeEvent;
    }

    public class InkItemCountWatcher : MonoBehaviour {
        // A simple script that can be used to watch the item count of some item in a given inventory
        // Combine with InkVariableSetter to set an ink variable
        public InventoryController m_targetInventory;
        public InkItemCountWatcherInstance[] m_watchers;

        public bool m_debugMessages = false;
        private Dictionary<ItemData, InkItemCountWatcherInstance> m_watcherDict = new Dictionary<ItemData, InkItemCountWatcherInstance> { };
        private Dictionary<ItemGameTrait, InkItemCountWatcherInstance> m_watcherDictTraits = new Dictionary<ItemGameTrait, InkItemCountWatcherInstance> { };

        public void Init (InkItemCountWatcherInstance[] watchers) { // external instantiation
            if (watchers.Length > 0) {
                foreach (InkItemCountWatcherInstance inst in watchers) {
                    if (inst.m_data != null && !m_watcherDict.ContainsKey (inst.m_data)) {
                        m_watcherDict.Add (inst.m_data, inst);
                        UpdateItemCount (inst.m_data);
                    } else {
                        if (inst.m_trait != ItemGameTrait.NONE && !m_watcherDictTraits.ContainsKey (inst.m_trait)) {
                            m_watcherDictTraits.Add (inst.m_trait, inst);
                            UpdateItemCount (inst.m_trait);
                        }
                    }

                }

            } else {
                Debug.LogWarning ("Initing ink item count watcher with an empty array!", gameObject);
            }
        }

        void Awake () {
            if (m_watchers != null) {
                foreach (InkItemCountWatcherInstance inst in m_watchers) {
                    if (inst.m_data != null) {
                        m_watcherDict.Add (inst.m_data, inst);
                    } else {
                        if (inst.m_trait != ItemGameTrait.NONE) {
                            m_watcherDictTraits.Add (inst.m_trait, inst);
                        }
                    }
                }
            };
        }

        void Start () {

            m_targetInventory.itemAddedEvent.AddListener (ItemChanged);
            m_targetInventory.itemRemovedEvent.AddListener (ItemChanged);
            foreach (InventoryController cnt in InventoryController.GetAllInventories (InventoryType.NONE, null, false)) {
                cnt.stackManipulator.combineFinishedEvent.AddListener (ItemChanged);
                cnt.stackManipulator.splitFinishedEvent.AddListener (ItemChanged);
            }

        }

        public void UpdateAllWatcherInstances () { // updates all values!
            foreach (KeyValuePair<ItemData, InkItemCountWatcherInstance> kvp in m_watcherDict) {
                UpdateItemCount (kvp.Key);
            }
            foreach (KeyValuePair<ItemGameTrait, InkItemCountWatcherInstance> kvp in m_watcherDictTraits) {
                UpdateItemCount (kvp.Key);
            }
        }

        void ItemChanged (InventoryController controller, Item_DragAndDrop item) {
            UpdateItemCount (item.targetBox.data);
        }
        void ItemChanged (Item_DragAndDrop item1, Item_DragAndDrop item2) {
            UpdateItemCount (item1.targetBox.data);
        }
        void ItemChanged (Item_DragAndDrop item, int amount) {
            UpdateItemCount (item.targetBox.data);
        }

        void UpdateItemCount (ItemData item) {
            if (m_debugMessages) {
                Debug.Log ("<color=blue>Updating item count for " + item.m_id + "</color>");
            };
            int itemCount = 0;
            InkItemCountWatcherInstance tryGetValue = null;
            m_watcherDict.TryGetValue (item, out tryGetValue);
            if (tryGetValue != null) {
                itemCount = m_targetInventory.CountItem (item);
                if (itemCount < 0) { itemCount = 0; }; // Fix the -1
                tryGetValue.m_changeEvent.Invoke (itemCount);
            } else {
                foreach (ItemGameTrait trait in item.m_gameTraits) {
                    UpdateItemCount (trait);
                }
            }
        }
        void UpdateItemCount (ItemGameTrait trait) {
            InkItemCountWatcherInstance tryGetValue = null;
            int itemCount = 0;
            m_watcherDictTraits.TryGetValue (trait, out tryGetValue);
            if (tryGetValue != null) {
                if (m_debugMessages) {
                    Debug.Log ("<color=blue>Found matching trait " + trait + "</color>");
                };
                foreach (Item_DragAndDrop itemswithtrait in m_targetInventory.ItemsByGameTrait (trait)) {
                    itemCount += itemswithtrait.targetBox.StackSize;
                }
                tryGetValue.m_changeEvent.Invoke (itemCount);
            }
        }

        public List<InkItemCountWatcherInstance> GenerateWatcherInstanceArray (BlueprintComponent[] components) {
            List<InkItemCountWatcherInstance> generatedList = new List<InkItemCountWatcherInstance> { };
            foreach (BlueprintComponent comp in components) {
                if (comp.data != null || comp.trait != ItemGameTrait.NONE) {
                    generatedList.Add (new InkItemCountWatcherInstance { m_data = comp.data, m_trait = comp.trait, m_changeEvent = new ItemCountChanged () });
                }
            }
            return generatedList;
        }

    }
}