using System.Collections;
using System.Collections.Generic;
using Doozy.Engine;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Inventory {

    [System.Serializable]
    public class InventoryOpened : UnityEvent<InventoryController> { }

    [System.Serializable]
    public class InventoryClosed : UnityEvent<InventoryController> { }

    [System.Serializable]
    public class InventoryItemAdded : UnityEvent<InventoryController, Item_DragAndDrop> { }

    [System.Serializable]
    public class InventoryItemRemoved : UnityEvent<InventoryController, Item_DragAndDrop> { }

    public class InventoryController : MonoBehaviour {

        // Static list of all inventories (in lieu of managers etc)
        public static List<InventoryController> allInventories = new List<InventoryController> { };

        public InventoryData data;
        public InventoryType type = InventoryType.DEFAULT;
        public List<InventoryType> permittedItemSources = new List<InventoryType> { InventoryType.DEFAULT };
        public Canvas mainCanvas;
        public CanvasGroup canvasGroup;
        public TextMeshProUGUI inventoryName;
        public TextMeshProUGUI inventorySpaceLeft;
        private int defaultSortOrder = 0;
        public GameObject itemBoxPrefab;
        public Transform inventoryParent;
        public Inventory_StackManipulator stackManipulator;
        public InventoryContextMenuController contextMenuController;
        public InventoryCraftingController craftingController;
        public Inventory_ConsumeWatcher consumeWatcher;
        public GameObject takeAllButton; // probably only necessary to submit for player inventories
        public GameObject tooltipCanvas;
        public Item_DragTarget mainDragTarget;
        public int maxSlots = 99;
        public bool isDragging = false;
        public float timeUntilDragStarted = 0.5f;
        public List<Item_DragAndDrop> allItemBoxes = new List<Item_DragAndDrop> { };
        public bool clearOnStart = false;
        public bool hideOnInit = true;

        public bool spawnItemBoxSeparately = true;
        private bool m_isActive = false;
        private bool usesDoozyUI = false;
        private Doozy.Engine.UI.UIView doozyView;

        private static List<ItemData> allItemDatas = new List<ItemData> { };
        private static List<InventoryData> allInventoryDatas = new List<InventoryData> { };
        private static List<ItemBlueprintData> allBlueprintDatas = new List<ItemBlueprintData> { };

        private static List<UseItemData> allUseItemDatas = new List<UseItemData> { };
        private Dictionary<string, string> savedInventory = new Dictionary<string, string> { };

        public InventoryOpened inventoryOpenedEvent;
        public InventoryClosed inventoryClosedEvent;

        public InventoryItemAdded itemAddedEvent;
        public InventoryItemRemoved itemRemovedEvent;

        public DragStarted itemDragStartedEvent;
        public DragEnded itemDragEndedEvent;
        public DragCompleted itemDragCompletedEvent;

        /*void OnEnabled () {
            if (!allInventories.Contains (this)) {
                allInventories.Add (this);
            }
        }*/

        void OnDestroy () {
            if (allInventories.Contains (this)) {
                allInventories.Remove (this);
            }
            inventoryClosedEvent.RemoveAllListeners ();
            inventoryOpenedEvent.RemoveAllListeners ();
            itemAddedEvent.RemoveAllListeners ();
            itemRemovedEvent.RemoveAllListeners ();
        }

        void Awake () {
            // Set up inventory at start (and clears existing inventory)
            //InitInventory (data, clearOnStart);
            if (!allInventories.Contains (this)) {
                allInventories.Add (this);
            }
            LoadAllItemDatas ();
            LoadAllInventoryDatas ();
            LoadAllBlueprintDatas ();
            LoadAllUseItemDatas ();
            doozyView = GetComponent<Doozy.Engine.UI.UIView> ();
            usesDoozyUI = doozyView != null;
        }

        [NaughtyAttributes.Button]
        void SetUniqueName () {
            gameObject.name = gameObject.name + IndexOfInventoryID (data.m_id, this);
        }

        // Start is called before the first frame update
        void Start () {

            if (mainCanvas == null) {
                mainCanvas = GetComponent<Canvas> ();
            }
            if (mainCanvas == null) {
                canvasGroup = GetComponent<CanvasGroup> ();
            }
            defaultSortOrder = mainCanvas.sortingOrder;
            // This is the 'backup' dragtarget, letting you just drag and drop into the inventory
            if (mainDragTarget != null) {
                mainDragTarget.dragCompletedEvent.AddListener (OnDragCompleted);
            }
            // Add listeners to stack manipulator onfinished
            stackManipulator.combineFinishedEvent.AddListener (FinishCombineStackManipulation);
            stackManipulator.splitFinishedEvent.AddListener (FinishSplitStackManipulation);
            itemAddedEvent.AddListener (PlayItemAddedSound); // for the soundddd

        }

        void LoadAllItemDatas () {
            // Load all item datas!
            if (allItemDatas.Count == 0) {
                Object[] loadedDatas = Resources.LoadAll ("Data/Items", typeof (ItemData));
                foreach (Object obj in loadedDatas) {
                    allItemDatas.Add (obj as ItemData);
                    if (obj as TaskItemData != null) { // debug component counts for task items, just in case
                        (obj as TaskItemData).DebugComponentCount ();
                    }
                }
            }
        }
        void LoadAllInventoryDatas () {
            // Load all inventory datas!
            if (allInventoryDatas.Count == 0) {
                Object[] loadedDatas = Resources.LoadAll ("Data/Inventories", typeof (InventoryData));
                foreach (Object obj in loadedDatas) {
                    allInventoryDatas.Add (obj as InventoryData);
                }
                // Debug.Log ("Inventories loaded: " + AllInventoryData.Count);
            }
        }

        void LoadAllBlueprintDatas () {
            // Load all blueprint datas!
            if (allBlueprintDatas.Count == 0) {
                Object[] loadedDatas = Resources.LoadAll ("Data/Blueprints", typeof (ItemBlueprintData));
                foreach (Object obj in loadedDatas) {
                    allBlueprintDatas.Add (obj as ItemBlueprintData);
                }
            }
        }

        void LoadAllUseItemDatas () {
            // Load all inventory datas!
            if (allUseItemDatas.Count == 0) {
                Object[] loadedDatas = Resources.LoadAll ("Data/UseItems", typeof (UseItemData));
                foreach (Object obj in loadedDatas) {
                    allUseItemDatas.Add (obj as UseItemData);
                }
                // Debug.Log ("Inventories loaded: " + AllInventoryData.Count);
            }
        }

        [NaughtyAttributes.Button]
        void ClearAndReinitInventory () {
            InitInventory (data, true);
        }

        void PlayItemAddedSound (InventoryController ctrl, Item_DragAndDrop item) {
            if (item.targetBox.data.HasGameTrait (ItemGameTrait.ITEM)) {
                AudioManager.instance.PlaySFX ("UI_inventoryAction");
            };
            //if (item.audioSource != null) {
            //    item.audioSource.PlayRandomType (SFXType.UI_DRAGCOMPLETE);
            //};
        }

        public void InitInventory (InventoryData newData, bool clearPrevious = false) {

            if (clearPrevious) {
                // Add all item boxes to the allItemBoxes list and add listeners to them
                allItemBoxes.Clear ();
                if (inventoryParent.childCount > 0) {
                    for (int i = 0; i < inventoryParent.childCount; i++) {
                        Transform child = inventoryParent.GetChild (i);
                        Item_DragAndDrop tryGetBox = child.GetComponentInChildren<Item_DragAndDrop> ();
                        if (tryGetBox != null) {
                            AddItemBox (tryGetBox);
                        }
                    }
                };
                ClearInventory ();
            }
            if (newData != null) {
                data = newData;
            }
            if (data != null) {
                maxSlots = data.m_defaultMaxSlots;
                UpdateInventoryUI ();
                if (data.m_inventoryItemPrefab != null) {
                    itemBoxPrefab = data.m_inventoryItemPrefab;
                }
                type = data.m_type;
                List<RandomizedInventoryItem> itemsToAdd = new List<RandomizedInventoryItem> { };
                if (data.m_defaultContent.Length > 0) {
                    itemsToAdd = GenerateRandomInventoryContent (data.m_defaultContent, data.m_minMaxRandomItemsSpawned);
                    // and finally actually add them, with randomized stack sizes!
                    foreach (RandomizedInventoryItem item in itemsToAdd) {
                        if (item.data != null) {
                            int randomStackSize = Mathf.Clamp (Random.Range (item.randomStackSize.x, item.randomStackSize.y), 0, item.data.m_maxStackSize);
                            if ((randomStackSize > 0 || item.data.HasTrait (ItemTrait.CAN_SPAWN_EMPTY)) && item.data != null) { // This setup allows for null datas, which can empty out slots!
                                AddItem (item.data, randomStackSize, true); // we FORCE ADD these, because goddammit
                            };
                        };
                    }

                }
                if (data.m_allowContentFrom.Length > 0) {
                    foreach (InventoryType type in data.m_allowContentFrom) {
                        if (!permittedItemSources.Contains (type)) {
                            permittedItemSources.Add (type);
                        }
                    }
                }
                // INIT ENGINE
                if (consumeWatcher != null) {
                    consumeWatcher.InitEngine ();
                }
                // INIT CRAFTING
                if (craftingController != null) {
                    craftingController.Init ();
                }

                // Hide/unhide the inventory!
                if (hideOnInit) {
                    HideInventory (true, false);
                } else {
                    ShowInventory (true, false);
                }
                Debug.Log ("<color=green>Finished initializing inventory " + gameObject.name + " with data " + data.m_id + ", spawning " + itemsToAdd.Count + " new items!</color>");
            }
        }

        public List<RandomizedInventoryItem> GenerateRandomInventoryContent (InventoryData data) {
            // Create dictionary of weighted data
            Dictionary<RandomizedInventoryItem, float> randomWeightedDictionary = new Dictionary<RandomizedInventoryItem, float> ();
            // These are the items to add - we add guaranteed items to it right away
            List<RandomizedInventoryItem> itemsToAdd = new List<RandomizedInventoryItem> { };
            foreach (RandomizedInventoryItem rdata in data.m_defaultContent) {
                if (rdata.guaranteed) {
                    itemsToAdd.Add (rdata);
                } else {
                    randomWeightedDictionary.Add (rdata, rdata.weight);
                };
            }
            // Sets the number of iterations we'll do
            int itemsToSpawn = data.m_defaultContent.Length - itemsToAdd.Count;
            if (data.m_minMaxRandomItemsSpawned.x > -1 && data.m_minMaxRandomItemsSpawned.y > 0) {
                itemsToSpawn = Random.Range (data.m_minMaxRandomItemsSpawned.x, data.m_minMaxRandomItemsSpawned.y);
            }
            // iterate through the list and add as many as randomly determined to it...
            if (itemsToSpawn > 0 && randomWeightedDictionary.Count > 0) {
                for (int i = 0; i < itemsToSpawn; i++) {
                    RandomizedInventoryItem randomItem = randomWeightedDictionary.RandomElementByWeight (e => e.Value).Key;
                    if (randomItem.random_unique) {
                        randomWeightedDictionary.Remove (randomItem);
                    }
                    itemsToAdd.Add (randomItem);
                };
            };
            return itemsToAdd;
        }

        void ClearInventory () { // -destroys- the inventory, omg
            foreach (Item_DragAndDrop box in allItemBoxes) {
                GameObject.Destroy (box.gameObject);
            }
            allItemBoxes.Clear ();
        }
        void UpdateInventoryUI () {
            if (inventoryName != null) {
                inventoryName.text = data.m_displayName;
            }
            if (inventorySpaceLeft != null) {
                inventorySpaceLeft.text = string.Format ("{0}/{1}", maxSlots - SpaceLeft, maxSlots);
            }
        }

        public int SpaceLeft {
            get {
                int usedSlots = 0;
                foreach (Item_DragAndDrop item in allItemBoxes) {
                    if (item.targetBox.data != null) {
                        usedSlots += item.targetBox.data.m_sizeInInventory;
                    } else { // Default if it doesn't have data for whatever reason - make it 0 or 1
                        usedSlots += 0;
                    }
                }
                return maxSlots - usedSlots;
            }
        }
        public void SetTakeAllButtonActive (bool active) {
            if (takeAllButton != null) {
                takeAllButton.SetActive (active);
            }
        }
        void SetTakeAllButtonActiveLate () { // to let everything else load in first, call with invoke
            //Debug.Log ("Testing take all button for active state");
            if (GetPermittedInventoriesForType (type, this).Count > 0) {
                //  Debug.Log ("Found " + GetPermittedInventoriesForType (type, this).Count + " permitted inventories that are active");
                SetTakeAllButtonActive (true);
            } else {
                SetTakeAllButtonActive (false);
            }
        }

        public void TryAddItem (ItemData itemData) {
            AddItem (itemData);
        }
        public bool AddItem (ItemData itemdata, int stackAmount = 1, bool forceAdd = false) { // add a box based on data, must spawn
            if ((SpaceLeft >= itemdata.m_sizeInInventory && itemdata.IsPermittedInventory (type)) || forceAdd) {
                Item_DragAndDrop newBox = SpawnBox (itemdata);
                newBox.targetBox.SetItemBoxData (itemdata);
                newBox.targetBox.StackSize = stackAmount;
                newBox.gameObject.name = itemdata.m_displayName + "_InventoryItem";
                return AddItemBox (newBox);
            } else {
                if (SpaceLeft < itemdata.m_sizeInInventory) {
                    Debug.LogWarning ("Could not add item: not enough space left in inventory! Space left: " + SpaceLeft + "/" + itemdata.m_sizeInInventory);
                } else if (!itemdata.IsPermittedInventory (type)) {
                    Debug.LogWarning ("Could not add item because it is not permitted in this inventory (" + type.ToString () + ")");
                }
                return false;
            }
        }

        public bool AddItemBox (Item_DragAndDrop newItemBox = null) { // Set to null to have it spawn one, otherwise it'll make one

            if (SpaceLeft > 0) {
                if (newItemBox == null) { // We spawn a new box if the item is null, though this is a bit risky...
                    Debug.LogWarning ("Attempted to add empty item! Use AddItem to spawn from data!");
                    return false;
                } else {
                    if (newItemBox.transform.parent != inventoryParent) { // if we're adding from outside the inventory
                        newItemBox.transform.SetParent (inventoryParent);
                    }
                }
                if (!allItemBoxes.Contains (newItemBox)) { // Add to list and also begin listening to the dragtarget
                    allItemBoxes.Add (newItemBox);
                    if (newItemBox.targetBox.dragTarget != null) {
                        newItemBox.targetBox.dragTarget.dragCompletedEvent.AddListener (OnDragCompleted);
                        newItemBox.targetBox.dragTarget.pointerDownEvent.AddListener (OnClickedItemBox);
                    };
                    newItemBox.dragEnded.AddListener (OnDragEnd);
                    newItemBox.dragStarted.AddListener (OnDragStart);
                    if (tooltipCanvas != null) {
                        newItemBox.targetBox.tooltip.tooltipCanvasParent = tooltipCanvas.transform;
                        newItemBox.targetBox.tooltip.ResetTooltip ("Tooltip: " + newItemBox.targetBox.data.m_id, true);
                        // SUPER HACKY LOL
                    }
                    newItemBox.UpdateInteractability ();
                }
                UpdateInventoryUI ();
                itemAddedEvent.Invoke (this, newItemBox);
                return true;
            } else {
                Debug.Log ("Not enough space!");
                return false;
            }
        }

        public bool RemoveItemBox (Item_DragAndDrop targetItem) { // Mainly to remove all the listeners - does -not- change the object's parent!
            if (allItemBoxes.Contains (targetItem)) {
                allItemBoxes.Remove (targetItem);
                if (targetItem.targetBox.dragTarget != null) {
                    targetItem.targetBox.dragTarget.dragCompletedEvent.RemoveListener (OnDragCompleted);
                    targetItem.targetBox.dragTarget.pointerDownEvent.RemoveListener (OnClickedItemBox);
                };

                targetItem.dragEnded.RemoveListener (OnDragEnd);
                targetItem.dragStarted.RemoveListener (OnDragStart);
                UpdateInventoryUI ();
                itemRemovedEvent.Invoke (this, targetItem);
                return true;
            } else {
                return false;
            }
        }

        Item_DragAndDrop SpawnBox (ItemData data) {
            GameObject spawnedObj = Instantiate (itemBoxPrefab, inventoryParent);
            Item_DragAndDrop spawnedItem = spawnedObj.GetComponent<Item_DragAndDrop> ();
            if (spawnItemBoxSeparately) { // we assume the item box -isn't- included in the prefab
                GameObject itemBox = Instantiate (data.m_prefab, spawnedObj.transform);
                UI_ItemBox targetBox = itemBox.GetComponent<UI_ItemBox> ();
                targetBox.data = data;
                spawnedItem.targetBox = targetBox;
                spawnedItem.targetTransform = itemBox.transform;
                //spawnedItem.audioSource.UpdateSoundDictionary (data.m_soundClips); // Update the box with the appropriate sound clip data!
            }
            return spawnedItem;
        }
        public void DestroyBox (Item_DragAndDrop target) { // can also be used to destroy 'foreign' boxes
            if (target != null) {
                if (target.targetBox.data.HasTrait (ItemTrait.DESTROYABLE)) {
                    if (allItemBoxes.Contains (target)) { // local box, np
                        RemoveItemBox (target);
                        Destroy (target.gameObject);
                    } else {
                        InventoryController targetObjectParent = target.GetComponentInParent<InventoryController> ();
                        if (targetObjectParent == null) { // No parent? Floating magical lonely boy. Oh well, kill then!
                            Destroy (target.gameObject);
                        } else { // send it to them to destroy
                            //   Debug.Log ("Target object parent is: " + targetObjectParent, targetObjectParent.gameObject);
                            //   Debug.Log ("Target is: " + target);
                            if (targetObjectParent == this) {
                                Debug.LogError ("Somehow trying to destroy a box that belongs to this controller without being in the item boxes! (" + target + ")", gameObject);
                                Destroy (target.gameObject);
                            } else {
                                targetObjectParent.DestroyBox (target);
                            };
                        }
                    }
                } else {
                    Debug.Log ("Could not destroy item " + target.gameObject.name + " because it is not flagged as destroyable!", target.gameObject);
                }
            } else {
                Debug.LogWarning ("Tried to DestroyBox an already null box! Not good.");
            }
        }

        int clicked = 0;
        float clicktime = 0;
        float clickdelay = 0.5f;
        void OnClickedItemBox (Item_DragTarget target, PointerEventData coll) { // Context menu!
            //if (GameManager.instance.GameState != GameStates.NARRATIVE) {
            if (coll.button == PointerEventData.InputButton.Right) {
                Debug.Log ("Right-clicked Clicked " + target.gameObject.name);
                if (contextMenuController != null) {
                    contextMenuController.SelectItem (target.gameObject.GetComponent<UI_ItemBox> ()); // no let's not do this
                }
            } else if (coll.button == PointerEventData.InputButton.Left) {
                clicked++;
                if (clicked == 1) clicktime = Time.time;
                if (clicked > 1 && Time.time - clicktime < clickdelay) {
                    clicked = 0;
                    clicktime = 0;
                    contextMenuController.ForceSelectOption (ContextMenuEntryType.UI_DROP, target.gameObject.GetComponent<UI_ItemBox> ());
                } else if (clicked > 2 || Time.time - clicktime > 1) { clicked = 0; };
                if (contextMenuController != null) {
                    contextMenuController.SelectItem (target.gameObject.GetComponent<UI_ItemBox> ()); // no let's not do this
                }
            }
            //}
        }

        void OnDragStart (Item_DragAndDrop target) {
            isDragging = true;
            // Debug.Log ("Started drag");
            target.transform.SetAsLastSibling ();
            contextMenuController.Cancel ();
            mainCanvas.sortingOrder = 999;
            itemDragStartedEvent.Invoke (target);
        }
        void OnDragEnd (Item_DragAndDrop target) {
            isDragging = false;
            //  Debug.Log ("Ended drag");
            target.ResetDragTargetPosition ();
            mainCanvas.sortingOrder = defaultSortOrder;
            itemDragEndedEvent.Invoke (target);

        }
        void OnDragCompleted (Item_DragAndDrop dragAndDropItem, Item_DragTarget target) {
            //if (GameManager.instance.GameState != GameStates.NARRATIVE) {
            Debug.Log (name + " completed drag of " + dragAndDropItem.name + " on " + target.name, target.gameObject);
            TryTakeItem (dragAndDropItem, target);
            //}
            itemDragCompletedEvent.Invoke (dragAndDropItem, target);
        }

        public void TryTakeItem (Item_DragAndDrop dragAndDropItem, Item_DragTarget target) {
            if (allItemBoxes.Contains (dragAndDropItem)) {
                //   Debug.Log ("Move inside own inventory!");
                if (!TryCombineInternal (dragAndDropItem, target.GetComponentInParent<Item_DragAndDrop> ())) { // try to combine, if not, switch places
                    SwitchPlacesInsideInventory (dragAndDropItem, target.GetComponentInParent<Item_DragAndDrop> ());
                };
            } else {
                //    Debug.Log ("Moved from different inventory! (report from: " + name + ")", gameObject);
                if (!TryCombineOrSplitExternal (dragAndDropItem, target.GetComponentInParent<Item_DragAndDrop> ())) {
                    TryTakeItemFromInventory (dragAndDropItem, target.GetComponentInParent<Item_DragAndDrop> ());
                };
            }
        }

        public void TryDropAll () { // same as what happens in the inventory crafting controller!
            List<Item_DragAndDrop> copyList = new List<Item_DragAndDrop> { };
            foreach (Item_DragAndDrop item in allItemBoxes) {
                copyList.Add (item);
            }
            foreach (Item_DragAndDrop item in copyList) {
                ReturnItemToPlayerInventory (item);
            }
        }

        void ReturnItemToPlayerInventory (Item_DragAndDrop item) {
            // Super ugly stuff incoming
            if (item == null) {
                Debug.LogError ("Tried to return null item, quitting", gameObject);
                return;
            }
            bool success = false;
            List<InventoryType> inventoriesToReturnTo = new List<InventoryType> { };
            foreach (InventoryType type in item.targetBox.data.m_permittedInventories) {
                if (type != data.m_type) { // don't add your -own- type
                    inventoriesToReturnTo.Add (type);
                };
            }
            foreach (InventoryType type in inventoriesToReturnTo) {
                Debug.Log ("Attempting to return item " + item.targetBox.data.m_id + " to inventory " + type);
                if (InventoryController.GetInventoryOfType (type, null, false) != null) {
                    success = InventoryController.GetInventoryOfType (type, null, false).TryTakeItemFromInventory (item, null);
                };
                if (success) { // success!
                    Debug.Log ("<color=green>Successfully returned item " + item.targetBox.data.m_id + " to inventory " + type + "</color>");
                    return;
                } else {
                    Debug.Log ("<color=red>Failed to return item " + item.targetBox.data.m_id + " to inventory " + type + "</color>");
                }
            }
        }

        public void TryTakeAll (InventoryController targetController = null) { // simple attempt to take all, ignoring splitting etc

            Debug.Log ("Trying to take all");
            if (targetController == null || targetController == this) { // if null, try to find the first inventory active that lets us steal their shit
                List<InventoryController> permittedInventories = InventoryController.GetPermittedInventoriesForType (type, this);
                Debug.Log ("Found " + permittedInventories.Count + " permitted inventories.");
                if (permittedInventories.Count > 0) {
                    foreach (InventoryController inv in permittedInventories) {
                        if (inv.allItemBoxes.Count > 0 && inv != this) {
                            Debug.Log ("Trying to take all from " + inv.type, inv.gameObject);
                            List<Item_DragAndDrop> copyList = new List<Item_DragAndDrop> (inv.allItemBoxes);
                            Debug.Log ("Attempting to iterate through list of length " + copyList.Count);
                            foreach (Item_DragAndDrop item in copyList) {
                                TryTakeItemFromInventory (item, null);
                            }
                        }
                    }
                }
                //targetController = InventoryController.GetPermittedInventoryForType (type, this);
            }
        }

        bool TryCombineInternal (Item_DragAndDrop draggedItem, Item_DragAndDrop targetItem) {
            // Both items are internal -> see if they are compatible and try to make a stack
            //   Debug.Log ("Trying to do an internal combination!");
            if (targetItem != null) {
                if (draggedItem.targetBox.data == targetItem.targetBox.data) {
                    if (targetItem.targetBox.StackSize < targetItem.targetBox.data.m_maxStackSize) { // there's space!
                        if (stackManipulator.CheckCompatibility (draggedItem)) {
                            stackManipulator.StartManipulator (draggedItem, targetItem, true); // we auto-combine
                            return true;
                        };
                    }
                } else {
                    //      Debug.Log ("Internal combination failed: incompatible data");
                }
            } else {
                //   Debug.Log ("Internal combination failed: targetItem null");
            }
            return false;
        }
        bool TryCombineOrSplitExternal (Item_DragAndDrop draggedItem, Item_DragAndDrop targetItem) {
            //   Debug.Log ("Trying to do an external combination!");
            // Check if there's space, to prevent splitting letting you go over max inventory size
            if (SpaceLeft >= draggedItem.targetBox.data.m_sizeInInventory || targetItem != null) {
                // Dragged item is external; if it has a stack size, let's let the player pick how much is dropped
                if (draggedItem.targetBox.StackSize > 1) {
                    if (stackManipulator.CheckCompatibility (draggedItem) && draggedItem.targetBox.data.IsPermittedInventory (type)) {
                        stackManipulator.StartManipulator (draggedItem, targetItem);
                        return true;
                    }
                }
            }
            return false;
        }

        void SwitchPlacesInsideInventory (Item_DragAndDrop switcher, Item_DragAndDrop switchee) { // switches places internally, using sibling index
            if (switchee != null) {
                int targetIndex = switchee.transform.GetSiblingIndex ();
                if (Input.mousePosition.x > switchee.transform.position.x) { // mouse is to the right of it, place it in front
                    if (targetIndex < inventoryParent.childCount - 1) {
                        switcher.transform.SetSiblingIndex (targetIndex + 1);
                    } else {
                        switcher.transform.SetAsLastSibling ();
                    }
                } else { // mouse is to the left of it, place it behind
                    switcher.transform.SetSiblingIndex (targetIndex);
                }
            }
        }

        public bool TryTakeItemFromInventory (Item_DragAndDrop item, Item_DragAndDrop targetSlot) { // if allowed - assumption is if inventories are visible, they'll take stuff
            InventoryController targetObjectParent = item.GetComponentInParent<InventoryController> ();
            if (targetObjectParent == null && item.targetBox.data.IsPermittedInventory (type)) { // No parent? Floating magical lonely boy. Oh well, yoink.
                return AddItemBox (item);
            } else {
                if (permittedItemSources.Contains (targetObjectParent.type)) { // permitted inventory type!
                    if (item.targetBox.draggable && item.targetBox.data.IsPermittedInventory (type)) { // can it be taken?
                        int attemptTake = AddItemStackable (item.targetBox.data, item.targetBox.StackSize);
                        // USE STACKABLE ADD INSTEAD
                        //    bool attemptTake = AddItemBox (item);
                        if (attemptTake == 0) { // success! remove the box from the other inventory
                            //targetObjectParent.RemoveItemBox (item);
                            targetObjectParent.DestroyBox (item);
                            //SwitchPlacesInsideInventory (item, targetSlot);
                            return true;
                            //Debug.Log (name + " stole item " + item.name + " successfully from " + targetObjectParent.name);
                        } else { // else we fail and just let it do whatever
                            item.targetBox.StackSize = attemptTake; // set the remainder to the remainder and let god sort it ou
                            //Debug.Log (name + " attempted to steal item " + item.name + " from " + targetObjectParent.name + " but failed (no space)");
                        }
                    }
                } else {
                    //Debug.Log (name + " attempted to steal item " + item.name + " from " + targetObjectParent.name + " but failed (not permitted)");
                }
            }
            return false;
        }

        void FinishCombineStackManipulation (Item_DragAndDrop selectedBox, Item_DragAndDrop combiner) {
            //Debug.Log ("Finished Combine Action. Result: ");
            if (combiner == null) { // basically a 'failed' combine, can just ignore
                //Debug.Log ("failed (combiner null)");
                return;
            }
            if (selectedBox.targetBox.StackSize < 1) { //the dragged box is empty! Destroy it
                //Debug.Log ("success (targetBox empty)");
                DestroyBox (selectedBox);
            } else if (combiner.targetBox.StackSize < 1) { // erm, other box is empty? destroy it?
                //Debug.Log ("success (combiner box empty)");
                DestroyBox (combiner);
            }
        }
        void FinishSplitStackManipulation (Item_DragAndDrop selectedBox, int stackAmount) {
            //Debug.Log ("Finished Split Action. Result: ");
            if (stackAmount == 0) { // Failed split, ignore
                //  Debug.Log ("failed (0 stack)");
                return;
            }

            if (stackAmount > 0) { // create a new copy of the box and add it to ourselves
                //  Debug.Log ("success: created new stacked box");
                int success = AddItemStackable (selectedBox.targetBox.data, stackAmount);
                if (success != 0) {
                    Debug.LogWarning ("Finish Split Stack Manipulation failed to add item stackable, amount left: " + success);
                    selectedBox.targetBox.StackSize += success; // re-add to the box...
                } else {
                    if (selectedBox.targetBox.StackSize < 1) { // split all from it somehow? Destroy
                        //  Debug.Log ("success: dragged box emptied");
                        DestroyBox (selectedBox);
                    }
                }
            }
        }

        public List<Item_DragAndDrop> ItemsByTrait (ItemTrait trait) {
            List<Item_DragAndDrop> returnList = new List<Item_DragAndDrop> { };
            foreach (Item_DragAndDrop item in allItemBoxes) {
                if (item.targetBox.data.HasTrait (trait)) {
                    returnList.Add (item);
                }
            }
            return returnList;
        }
        public List<Item_DragAndDrop> ItemsByGameTrait (ItemGameTrait trait) {
            List<Item_DragAndDrop> returnList = new List<Item_DragAndDrop> { };
            foreach (Item_DragAndDrop item in allItemBoxes) {
                if (item.targetBox.data.HasGameTrait (trait)) {
                    returnList.Add (item);
                }
            }
            return returnList;
        }
        public List<Item_DragAndDrop> ItemsByData (ItemData data) {
            List<Item_DragAndDrop> returnList = new List<Item_DragAndDrop> { };
            foreach (Item_DragAndDrop item in allItemBoxes) {
                if (item.targetBox.data == data) {
                    returnList.Add (item);
                }
            }
            return returnList;
        }

        public int CountItem (ItemData itemData) {
            int returnValue = -1;
            foreach (Item_DragAndDrop item in allItemBoxes) {
                if (item.targetBox.data == itemData) {
                    if (returnValue == -1) { // return 0 if it exists at least once!
                        returnValue = 0;
                    }
                    returnValue += item.targetBox.StackSize;
                }
            }
            return returnValue;
        }
        public int DestroyItemAmountByTrait (ItemGameTrait trait, int amount) {
            if (trait != ItemGameTrait.NONE) {
                foreach (Item_DragAndDrop item in ItemsByGameTrait (trait)) {
                    int amountDestroyable = CountItem (item.targetBox.data);
                    int success = DestroyItemAmount (item.targetBox.data, amount);
                    if (success == 0) {
                        return 0;
                    } else {
                        amount -= amountDestroyable;
                    }
                }
            } else {
                Debug.LogWarning ("Cannot destroy by gametrait NONE!");
            }
            return amount;
        }
        public int DestroyItemAmount (ItemData itemData, int amount) { // returns 0 if successful or the amount left undestroyed if unsuccessful
            if (CountItem (itemData) < 0) { // we don't have enough, return
                Debug.LogWarning ("Attempted to destroy " + amount + " of " + itemData.m_id + " but we did not have enough (" + CountItem (itemData) + ")");
                return amount;
            }
            int amountLeft = amount;
            foreach (Item_DragAndDrop item in allItemBoxes) { // iterate through all and destroy until we reach the end
                if (item.targetBox.data == itemData) {
                    if (item.targetBox.StackSize >= amountLeft) {
                        item.targetBox.StackSize -= amountLeft;
                        if (item.targetBox.StackSize <= 0) {
                            if (item.targetBox.StackSize == 0 && itemData.HasTrait (ItemTrait.CAN_SPAWN_EMPTY)) {
                                Debug.Log ("Reached 0 for item that is allowed to be at 0 when destroying!");
                            } else {
                                DestroyBox (item);
                            };
                        } else {
                            itemRemovedEvent.Invoke (this, item);
                        }
                        return 0;
                    } else {
                        amountLeft -= item.targetBox.StackSize;
                        DestroyBox (item);
                        return DestroyItemAmount (itemData, amountLeft);
                    }
                }
            }
            return 0;
        }
        public int AddItemStackable (ItemData itemData, int amount, bool forceAdd = false) { // Add items to stacks - a bit smarter, in short. Returns 0 if all was added
            if (!itemData.HasTrait (ItemTrait.STACKABLE)) {
                Debug.LogWarning ("Trying to add-stackable item that cannot be stacked! Attempting normal AddItem");
                return AddItem (itemData, amount, forceAdd) ? amount : 0; // add back amount if failed, 0 if successful
            }
            if (CountItem (itemData) < 0) { // We don't actually have it :O
                if (itemData.m_maxStackSize >= amount) { // stack size is within limits tho!
                    if (AddItem (itemData, amount, forceAdd)) { // successfully added item! return 0
                        return 0;
                    } else {
                        return amount; // if we can't add even one, and we don't already have it, there's no space anyway -> quit
                    }
                } // else: the amount is larger than what we can accomodate, so we'll need to add multiples
            }
            int totalPossibleSpace = 0;
            foreach (Item_DragAndDrop targetItem in ItemsByData (itemData)) {
                totalPossibleSpace += targetItem.targetBox.StackLeft;
            }
            //  Debug.Log ("Attempting to add an item stackably, with total possible space being " + totalPossibleSpace + " with a needed amount of " + amount);
            // uh oh - we will need to add multiples no matter what.
            while (totalPossibleSpace < amount) { // we add items until we have enough possible space to accomodate
                bool success = AddItem (itemData, 0, forceAdd); // we add -empty- items
                if (!success) { // we could not add any more items, oh no
                    //Debug.LogWarning ("Could not add enough items to satisfy the demand! Cancelling. Amount left: " + amount);
                    break;
                }
                totalPossibleSpace += itemData.m_maxStackSize; // we add the full possible spaceleft of the item just added
            }

            if (itemData.HasTrait (ItemTrait.STACKABLE)) { // We only do it if it's actually stackable, ofc
                foreach (Item_DragAndDrop item in ItemsByData (itemData)) { // iterate through all and add until we reach the end
                    int currentItemStackSize = item.targetBox.StackSize;
                    if (itemData.m_maxStackSize >= currentItemStackSize + amount) {
                        item.targetBox.StackSize += amount; // instant success!
                        itemAddedEvent.Invoke (this, item);
                        return 0;
                    } else {
                        int canBeAdded = item.targetBox.StackLeft;
                        if (canBeAdded > 0) {
                            item.targetBox.StackSize += canBeAdded;
                            itemAddedEvent.Invoke (this, item);
                            amount -= canBeAdded;
                        }
                    }
                }
            } else {
                if (AddItem (itemData, amount, forceAdd)) { // if the item is not stackable, we still try to add one more separately
                    return 0;
                } else {
                    return amount; // if we can't add even one, and we don't already have it, there's no space anyway -> quit
                }
            }
            // Somehow, we still have stuff left
            Debug.LogWarning ("Could not add enough items to satisfy the demand! Cancelling. Amount left: " + amount);
            return amount; // failed to add all of it, or just failed
        }

        public bool Visible {
            get {
                return m_isActive;
            }
            set {
                if (value) {
                    ShowInventory ();
                } else {
                    HideInventory ();
                }
            }
        }

        public void ToggleVisible () {
            Visible = !Visible;
        }

        public void ShowInventory (bool force = false, bool sendEvent = true) {
            // Fancy code for showing inventory, maybe some doozy control graphs
            if (!Visible || force) { // Only want to invoke event if actually hidden
                if (!usesDoozyUI) {
                    canvasGroup.interactable = true;
                    canvasGroup.alpha = 1f;
                    canvasGroup.blocksRaycasts = true;
                } else {
                    doozyView.Show ();
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                //  Debug.Log ("Showing inventory.", gameObject);
                switch (type) {
                    /*case InventoryType.PLAYER:
                        {
                            if (sendEvent) { GameEventMessage.SendEvent ("ShowPlayerInventory"); };
                            break;
                        }
                    case InventoryType.LOOTABLE:
                        {
                            //GameEventMessage.SendEvent ("ShowLootingInventory");
                            canvasGroup.interactable = true;
                            canvasGroup.alpha = 1f;
                            canvasGroup.blocksRaycasts = true;
                            break;
                        }*/
                    case InventoryType.CRAFTING:
                        {
                            //if (sendEvent) { GameEventMessage.SendEvent ("ShowCraftingInventory"); };
                            if (craftingController != null) {
                                craftingController.Active = true;
                            } else {
                                Debug.LogWarning ("Inventory set to 'crafting' has null crafting-controller!");
                            };
                            break;
                        }
                    default:
                        {
                            //if (sendEvent) { GameEventMessage.SendEvent ("ShowPlayerInventory"); };
                            break;
                        }

                }
                // For setting the take all button on/off - note that other inventories should not have it set!
                Invoke (nameof (SetTakeAllButtonActiveLate), 0.01f);
                m_isActive = true;
                if (sendEvent) {
                    inventoryOpenedEvent.Invoke (this);
                };
                // SET STATE!
                //GameLogic.GameStateLogic.Instance.State = GameLogic.GameStateLogic.StateEnum.Inventory;
            } else {
                Debug.Log ("NOT Showing inventory because " + Visible, gameObject);
            };
        }
        public void HideInventory (bool force = false, bool sendEvent = true) {
            // Debug.Log ("Hiding inventory " + gameObject.name, gameObject);
            // Fancy code for hiding inventory, maybe some doozy control graphs
            if (Visible || force) { // we don't want to invoke the event for this unless we're actually hidden
                if (!usesDoozyUI) {
                    canvasGroup.interactable = false;
                    canvasGroup.alpha = 0f;
                    canvasGroup.blocksRaycasts = false;
                } else {
                    doozyView.Hide ();
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                };
                /* if (type == InventoryType.LOOTABLE) {
                     canvasGroup.interactable = false;
                     canvasGroup.alpha = 0f;
                     canvasGroup.blocksRaycasts = false;
                 } else {
                     if (sendEvent) { GameEventMessage.SendEvent ("CloseInventory"); };
                 }*/
                m_isActive = false;
                stackManipulator.Active = false;
                if (craftingController != null) {
                    craftingController.Active = false;
                };
                if (sendEvent) {
                    inventoryClosedEvent.Invoke (this);
                };
                // ARE WE THE LAST INVENTORY? THEN SET STATE
                if (InventoryController.GetAllInventories ().Count == 0) {
                    //GameLogic.GameStateLogic.Instance.InvokeNextState ();
                }
            };
        }
        // HELPER FUNCTIONS

        public static List<InventoryController> GetAllInventories (InventoryType type = InventoryType.NONE, InventoryController excluded = null, bool onlyActive = true) { // returns list of all inventories of type, optionally excluding one (typically 'self')
            List<InventoryController> returnList = new List<InventoryController> ();
            foreach (InventoryController inv in allInventories) {
                if ((onlyActive && inv.Visible) || !onlyActive) {
                    if (inv.type == type || type == InventoryType.NONE) {
                        if (excluded != null) {
                            if (inv != excluded) {
                                returnList.Add (inv);
                            }
                        } else {
                            returnList.Add (inv);
                        }
                    }
                }
            };
            return returnList;
        }
        public static InventoryController GetInventoryOfType (InventoryType type, InventoryController excluded = null, bool onlyActive = true) { // returns the first inventory of type it finds, optionally excluding one (typically 'self')
            foreach (InventoryController inv in allInventories) {
                if ((onlyActive && inv.Visible) || !onlyActive) {
                    if (inv.type == type) {
                        if (excluded != null) {
                            if (inv != excluded) {
                                return inv;
                            }
                        } else {
                            return inv;
                        }
                    }
                }
            }
            return null;
        }

        public static InventoryController GetPermittedInventoryForType (InventoryType type, InventoryController excluded = null, bool onlyActive = true) {
            foreach (InventoryController inv in allInventories) {
                if ((onlyActive && inv.Visible) || !onlyActive) {
                    if (inv.data.AllowsContentFrom (type)) {
                        if (excluded != null) {
                            if (inv != excluded) {
                                return inv;
                            }
                        } else {
                            return inv;
                        }
                    }
                }
            }
            return null;
        }
        public static List<InventoryController> GetPermittedInventoriesForType (InventoryType type, InventoryController excluded = null, bool onlyActive = true) {
            List<InventoryController> returnList = new List<InventoryController> ();
            foreach (InventoryController inv in allInventories) {
                if ((onlyActive && inv.Visible) || !onlyActive) {
                    if (inv.data.AllowsContentFrom (type)) {
                        if (excluded != null) {
                            if (inv != excluded) {
                                returnList.Add (inv);
                            }
                        } else {
                            returnList.Add (inv);
                        }
                    }
                }
            }
            return returnList;
        }

        public static InventoryController SpawnInventory (InventoryData data, bool visibleOnStart = true) { // spawns a new inventory with data
            InventoryController newController = null;
            if (data != null) {
                if (data.m_inventoryCanvasPrefab != null) {
                    newController = Instantiate (data.m_inventoryCanvasPrefab).GetComponent<InventoryController> ();
                    if (newController != null) {
                        newController.data = data;
                        newController.hideOnInit = !visibleOnStart;
                    }
                }
            }
            return newController;
        }

        public static void CloseAllInventories (InventoryType ofType = InventoryType.NONE) { // closes all inventories/all inventories of type
            foreach (InventoryController inv in allInventories) {
                if (ofType == InventoryType.NONE) {
                    inv.Visible = false;
                } else {
                    if (inv.type == ofType) {
                        inv.Visible = false;
                    }
                }
            }
        }
        public static void OpenAllInventories (InventoryType ofType = InventoryType.PLAYER) { // opens all inventories / all inventories of type
            foreach (InventoryController inv in allInventories) {
                if (ofType == InventoryType.NONE) {
                    inv.Visible = true;
                } else {
                    if (inv.type == ofType) {
                        inv.Visible = true;
                    }
                }
            }
        }

        public static ItemData GetDataByID (string id) {
            foreach (ItemData data in allItemDatas) {
                if (data.m_id == id) {
                    return data;
                }
            }
            return null;
        }
        public static InventoryData GetInventoryDataByID (string id) {
            foreach (InventoryData data in allInventoryDatas) {
                if (data.m_id == id) {
                    return data;
                }
            }
            return null;
        }

        public static ItemBlueprintData GetBlueprintById (string id) {
            foreach (ItemBlueprintData data in allBlueprintDatas) {
                if (data.m_id == id) {
                    return data;
                }
            }
            return null;
        }
        public static List<InventoryData> AllInventoryData {
            get {
                return allInventoryDatas;
            }
        }

        public static List<ItemBlueprintData> AllBlueprintData {
            get {
                return allBlueprintDatas;
            }
        }

        public static List<ItemData> AllItemData {
            get {
                return allItemDatas;
            }
        }

        public static int IndexOfInventoryID (string dataID, InventoryController controller) {
            // returns -1 if it is the only controller with that data.m_id, otherwise returns their index in allInventories
            bool duplicatesFound = false;
            int indexOf = -1;
            for (int i = 0; i < allInventories.Count; i++) {
                InventoryController inv = allInventories[i];
                if (inv == controller) {
                    indexOf = i;
                } else {
                    if (inv.data.m_id == dataID) { // multiples of same id, use their index instead
                        duplicatesFound = true;
                    }
                }
            }
            if (duplicatesFound) {
                return indexOf;
            } else {
                return -1;
            }
        }

        public static List<RandomizedInventoryItem> GenerateRandomInventoryContent (RandomizedInventoryItem[] itemArray, Vector2Int minMaxRandomItemsSpawned) {
            // Create dictionary of weighted data
            Dictionary<RandomizedInventoryItem, float> randomWeightedDictionary = new Dictionary<RandomizedInventoryItem, float> ();
            // These are the items to add - we add guaranteed items to it right away
            List<RandomizedInventoryItem> itemsToAdd = new List<RandomizedInventoryItem> { };
            foreach (RandomizedInventoryItem rdata in itemArray) {
                if (rdata.guaranteed) {
                    itemsToAdd.Add (rdata);
                } else {
                    randomWeightedDictionary.Add (rdata, rdata.weight);
                };
            }
            // Sets the number of iterations we'll do
            int itemsToSpawn = itemArray.Length - itemsToAdd.Count;
            if (minMaxRandomItemsSpawned.x > -1 && minMaxRandomItemsSpawned.y > 0) {
                itemsToSpawn = Random.Range (minMaxRandomItemsSpawned.x, minMaxRandomItemsSpawned.y);
            }
            // iterate through the list and add as many as randomly determined to it...
            if (itemsToSpawn > 0 && randomWeightedDictionary.Count > 0) {
                for (int i = 0; i < itemsToSpawn; i++) {
                    RandomizedInventoryItem randomItem = randomWeightedDictionary.RandomElementByWeight (e => e.Value).Key;
                    if (randomItem.random_unique) {
                        randomWeightedDictionary.Remove (randomItem);
                    }
                    itemsToAdd.Add (randomItem);
                };
            };
            return itemsToAdd;
        }

        [NaughtyAttributes.Button]
        void DebugSaveInventory () {
            SaveInventory ();
        }

        [NaughtyAttributes.Button]
        void DebugLoadInventory () {
            LoadInventory ();
        }

        private List<string> savedStrings = new List<string> { };
        public void SaveInventory () {
            int index = IndexOfInventoryID (data.m_id, this);
            savedInventory.Clear ();
            for (int i = 0; i < allItemBoxes.Count; i++) {
                // We add each item separately by their index + stack amount and their ID
                string itemId = allItemBoxes[i].targetBox.data.m_id;
                string indexWithStack = string.Format ("{0}_{1}", i, allItemBoxes[i].targetBox.StackSize);
                savedInventory.Add (indexWithStack, itemId);
            }
            // Saves based on game object name
            string inventorySaveName = string.Format ("{0}({1})", data.m_id.ToString (), gameObject.name);
            if (savedStrings.Contains (inventorySaveName)) {
                Debug.LogWarning ("Warning: two inventories cannot have the same game object name and inventory m_id, they will override one another! (" + inventorySaveName + ")", gameObject);
            }
            ES3.Save<Dictionary<string, string>> (SaveManager.instance.CurrentSlot + "_SavedInventory_" + inventorySaveName, savedInventory, SaveManager.instance.settings);
            SaveManager.instance.AddSlotKey (SaveManager.instance.CurrentSlot + "_SavedInventory_" + inventorySaveName);
            Debug.Log ("Saved inventory: " + inventorySaveName);
        }

        public void LoadInventory () {
            int index = IndexOfInventoryID (data.m_id, this);
            string inventorySaveName = string.Format ("{0}({1})", data.m_id.ToString (), gameObject.name);
            if (ES3.KeyExists (SaveManager.instance.CurrentSlot + "_SavedInventory_" + inventorySaveName)) {
                savedInventory = ES3.Load<Dictionary<string, string>> (SaveManager.instance.CurrentSlot + "_SavedInventory_" + inventorySaveName, savedInventory);
                ClearInventory ();
                LoadAllItemDatas ();
                foreach (KeyValuePair<string, string> kvp in savedInventory) {
                    ItemData data = GetDataByID (kvp.Value);
                    int itemIndex = int.Parse (kvp.Key.Split ('_') [0]); // we don't actually use the index thoughhhh
                    int itemStack = int.Parse (kvp.Key.Split ('_') [1]); // stack size!
                    AddItem (data, itemStack);
                }
                Debug.Log ("Loaded inventory: " + inventorySaveName);
            } else {
                Debug.Log ("Did not load inventory " + inventorySaveName + " because it was empty.");
            }
        }

        public void ClearSavedInventory () {
            int index = IndexOfInventoryID (data.m_id, this);
            string inventorySaveName = string.Format ("{0}({1})", data.m_id.ToString (), index);
            ES3.DeleteKey (SaveManager.instance.CurrentSlot + "_SavedInventory_" + inventorySaveName);
        }

        // Update is called once per frame
        void Update () {

        }
    }
}