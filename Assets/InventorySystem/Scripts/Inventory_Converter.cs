using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory {

    [System.Serializable]
    public class ItemConversionDefinition {
        [Tooltip ("If origin inventory has this trait...")]
        public ItemGameTrait m_originTrait; // if the origin item has this trait...
        [Tooltip ("Then this item is created in the target inventory")]
        public ItemData m_itemInTarget; // it creates this item

    }

    [System.Serializable]
    public class ItemGameTraitToAmount {
        [Tooltip ("This is the same as the trait in ItemConversionDefinition")]
        public ItemGameTrait m_masterTrait;
        [Tooltip ("Having this trait on top of the masterTrait then converts to a specific amount")]
        public ItemGameTrait m_trait;
        public int m_amount;

        public int GetAmount (ItemGameTrait trait) {
            if (trait == m_trait) {
                return m_amount;
            } else {
                return 0;
            }
        }
    }
    public class Inventory_Converter : MonoBehaviour { // Poorly, poorly named
        // Start is called before the first frame update

        public InventoryController m_originInventory;
        public InventoryController m_targetInventory;
        public ItemConversionDefinition[] m_definitions;
        private Dictionary<ItemGameTrait, ItemData> m_definitionsDict = new Dictionary<ItemGameTrait, ItemData> { };
        public ItemGameTraitToAmount[] m_amountDefinitions;
        private Dictionary<ItemGameTrait, List<ItemGameTraitToAmount>> m_amountDefinitionsDict = new Dictionary<ItemGameTrait, List<ItemGameTraitToAmount>> { };

        private Dictionary<ItemData, int> m_initValues = new Dictionary<ItemData, int> { };
        public InventoryData m_nulltargetInventory;

        void Awake () {
            // Make the dictionaries
            foreach (ItemConversionDefinition def in m_definitions) {
                m_definitionsDict.Add (def.m_originTrait, def.m_itemInTarget);
            }
            foreach (ItemGameTraitToAmount def in m_amountDefinitions) {
                if (m_amountDefinitionsDict.ContainsKey (def.m_masterTrait)) {
                    m_amountDefinitionsDict[def.m_masterTrait].Add (def);
                } else {
                    m_amountDefinitionsDict.Add (def.m_masterTrait, new List<ItemGameTraitToAmount> { def });
                }
            }
            // Update itself whenever the origininventory closes!
            m_originInventory.inventoryClosedEvent.AddListener ((arg0) => UpdateInventory ());
            m_originInventory.inventoryOpenedEvent.AddListener ((arg0) => UpdateInventory ());
            // Init values, just in case
            InitValues ();
        }

        [NaughtyAttributes.Button]
        void DebugInit () {
            Init ();
        }
        public void Init (float multiplier = 1f) { // multiplier, e.g. 0.5f halves all values
            // start by clearing the inventory & max value dictionary
            m_targetInventory.InitInventory (m_nulltargetInventory, m_nulltargetInventory.m_clearOnInit);
            m_initValues.Clear ();
            // Go through each item and add appropriate items to the target inventory!!
            foreach (Item_DragAndDrop box in m_originInventory.allItemBoxes) {
                ItemData data = box.targetBox.data;
              //  Debug.Log ("Found data " + data + ", with trait count " + data.m_gameTraits.Length);
                foreach (ItemGameTrait trait in data.m_gameTraits) {
                    ItemData outData = null;
                    m_definitionsDict.TryGetValue (trait, out outData);
                    if (outData != null) { // Contains key!
                        int amount = GetAmount (box.targetBox.data, box.targetBox.StackSize, trait);
                       // Debug.Log ("Found data " + outData + " in def dictionary, adding " + amount + " to targetInv");
                        AddItemToTarget (outData, amount);
                    }
                }
            }
            Dictionary<ItemData, int> copyDict = new Dictionary<ItemData, int> (m_initValues);
            foreach (KeyValuePair<ItemData, int> kvp in copyDict) {
                float multiplierValue = (float) kvp.Value * multiplier;
                if (multiplierValue != (float) kvp.Value) { // different value!
                    if ((int) multiplierValue > kvp.Value) {
                        AddItemToTarget (kvp.Key, (int) multiplierValue - kvp.Value);
                    } else if ((int) multiplierValue < kvp.Value) {
                        RemoveItemFromTarget (kvp.Key, kvp.Value - (int) multiplierValue);
                    }
                }
                Debug.Log ("<color=blue>Init Values for " + kvp.Key.m_id + ": " + kvp.Value + "</color>");
            }
        }

        [NaughtyAttributes.Button]
        public void UpdateInventory () { // E.g. after finishing adding/removing units
            Dictionary<ItemData, int> currentValue = new Dictionary<ItemData, int> { };
            foreach (Item_DragAndDrop box in m_originInventory.allItemBoxes) {
                ItemData data = box.targetBox.data;
                foreach (ItemGameTrait trait in data.m_gameTraits) {
                    ItemData outData = null;
                    m_definitionsDict.TryGetValue (trait, out outData);
                    if (outData != null) { // Contains key!
                        int amount = GetAmount (box.targetBox.data, box.targetBox.StackSize, trait);
                        if (currentValue.ContainsKey (outData)) { // set up the current values for this item
                            currentValue[outData] += amount;
                        } else {
                            currentValue.Add (outData, amount);
                        }
                    }
                }
            }
            foreach (KeyValuePair<ItemData, int> kvp in currentValue) {
                int tryGetValue = 0;
                m_initValues.TryGetValue (kvp.Key, out tryGetValue);
                if (tryGetValue > 0) { // there used to be a value
                    if (tryGetValue < kvp.Value) { // it is -smaller- than the current value
                        AddItemToTarget (kvp.Key, kvp.Value - tryGetValue); // add the difference
                    } else { // it is -larger-....do we care?
                        int actualAmount = m_targetInventory.CountItem (kvp.Key);
                        if (actualAmount > kvp.Value) { // there is more left than there can be
                            RemoveItemFromTarget (kvp.Key, actualAmount - kvp.Value);
                        };
                    }
                } else { // no value existed, so we can just add the full value
                    AddItemToTarget (kvp.Key, kvp.Value);
                }

            }
            // Remove all that no longer exist!
            Dictionary<ItemData, int> copyDict = new Dictionary<ItemData, int> (m_initValues);
            foreach (KeyValuePair<ItemData, int> kvp in copyDict) {
                if (!currentValue.ContainsKey (kvp.Key)) {
                    RemoveItemFromTarget (kvp.Key, m_targetInventory.CountItem (kvp.Key));
                }
            }
            //Debug.Log ("<color=blue>Current / Init Values for" + kvp.Key.m_id + ": " + kvp.Value + "/" + m_initValues[kvp.Key] + "</color>");
        }

        public void InitValues () { // -only- inits the values
            m_initValues.Clear ();
            // Go through each item to init values
            foreach (Item_DragAndDrop box in m_originInventory.allItemBoxes) {
                ItemData data = box.targetBox.data;
                foreach (ItemGameTrait trait in data.m_gameTraits) {
                    ItemData outData = null;
                    m_definitionsDict.TryGetValue (trait, out outData);
                    if (outData != null) { // Contains key!
                        int amount = GetAmount (box.targetBox.data, box.targetBox.StackSize, trait);
                        //Debug.Log ("Found data " + outData + " in def dictionary, adding " + amount + " to targetInv");
                        m_initValues.Add (outData, amount);
                    }
                }
            }
            Debug.Log ("<color=green>Finished initializing values for Converter!</color>");
        }

        public void UnitsRemoved (ItemData target, int amountRemoved) {

            ItemData data = target;
            foreach (ItemGameTrait trait in data.m_gameTraits) {
                ItemData outData = null;
                m_definitionsDict.TryGetValue (trait, out outData);
                if (outData != null) { // Contains key!
                    int amount = GetAmount (data, amountRemoved, trait);
                    Debug.Log ("Found data " + outData + " in def dictionary, removing " + amount + " from targetInv");
                    RemoveItemFromTarget (outData, amount);
                }
            }
        }

        int GetAmount (ItemData data, int stackSize, ItemGameTrait masterTrait) {
            int returnValue = 0;
            List<ItemGameTraitToAmount> amountDefList = new List<ItemGameTraitToAmount> { };
            m_amountDefinitionsDict.TryGetValue (masterTrait, out amountDefList);
            if (amountDefList != null) {
                foreach (ItemGameTrait trait in data.m_gameTraits) {
                    foreach (ItemGameTraitToAmount tta in amountDefList) {
                        returnValue += (tta.GetAmount (trait) * stackSize);
                    }
                }
            } else {
                Debug.LogWarning ("Could not find trait with name " + masterTrait + " in amountdef dictionary");
            }
            return returnValue;
        }

        void AddItemToTarget (ItemData data, int amount) {
            if (amount > 0) {
                bool TryAdd = m_targetInventory.AddItemStackable (data, amount) == 0;
                if (TryAdd) {
                    Debug.Log ("<color=green>Added " + amount + " of item " + data.m_id + " to " + m_targetInventory.name + "</color>");
                    if (m_initValues.ContainsKey (data)) { // set up the 'max' init values for this init
                        m_initValues[data] += amount;
                    } else {
                        m_initValues.Add (data, amount);
                    }
                } else {
                    Debug.LogError ("<color=red> Failed to add " + data.m_id + " to " + m_targetInventory.name);
                }
            }
        }

        void RemoveItemFromTarget (ItemData data, int amount) {
            if (amount > 0) {
                bool TryRemove = m_targetInventory.DestroyItemAmount (data, amount) == 0;
                if (TryRemove) {
                    Debug.Log ("<color=yellow>Removed + " + amount + " of item " + data.m_id + " from " + m_targetInventory.name + "</color>");
                    m_initValues[data] -= amount;
                } else {
                    Debug.LogWarning ("<color=red> Failed to remove " + data.m_id + " from " + m_targetInventory.name);
                }
            }
        }

        // Update is called once per frame
        void Update () {

        }
    }
}