using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Inventory {
    [System.Serializable]
    public class ItemCrafted : UnityEvent<InventoryController, ItemBlueprintData> { }

    public class InventoryCraftingController : MonoBehaviour {
        public InventoryController parentController;
        public InventoryController craftingResultsController;

        public List<ItemBlueprintType> craftableTypes = new List<ItemBlueprintType> { ItemBlueprintType.ANY };
        public CanvasGroup canvasGroup;
        public Button craftButton;
        public Inventory_BlueprintBox exampleResult;
        public GameObject m_blueprintPrefab;
        public Transform m_blueprintListParent;
        public Transform m_blueprintTooltipParent;
        public UI_ItemBox[] displayInventory;
        public bool startOff = true;
        public bool returnAllItemsOnClose = true;
        public InventoryType[] inventoriesToReturnTo = { InventoryType.PLAYER };
        private bool m_active;
        private bool m_checkingActive = true;
        public ItemBlueprintData targetBlueprint;

        public Button m_closeBlueprintListButton;
        public Button m_openBlueprintListButton;
        private Coroutine crafting;
        private static List<ItemBlueprintData> allBlueprintDatas = new List<ItemBlueprintData> { };
        private Dictionary<ItemBlueprintType, List<ItemBlueprintData>> blueprintLookupDict = new Dictionary<ItemBlueprintType, List<ItemBlueprintData>> { };
        public List<ItemBlueprintData> craftableBlueprints = new List<ItemBlueprintData> { };

        private Dictionary<ItemBlueprintData, Inventory_BlueprintBox> displayedBlueprintDict = new Dictionary<ItemBlueprintData, Inventory_BlueprintBox> { };
        public ItemCrafted itemCraftedEvent;
        private Dictionary<Item_DragAndDrop, InventoryController> allAddedItems = new Dictionary<Item_DragAndDrop, InventoryController> { };

        // Static known blueprints list!!
        private static Dictionary<ItemBlueprintData, int> m_knownBlueprints = new Dictionary<ItemBlueprintData, int> { };

        void Awake () {

            if (canvasGroup == null) {
                canvasGroup = GetComponent<CanvasGroup> ();
            }
            if (parentController == null) {
                parentController = GetComponentInParent<InventoryController> ();
            }
            if (craftingResultsController == null) { // for old functionality
                craftingResultsController = parentController;
            }
        }
        // Start is called before the first frame update
        void Start () {
            LoadKnownBlueprints (); // we do this in start since it depends on AllBlueprintDatas
            CreateExampleCopy (null, 0); // to hide it on start
            parentController.itemAddedEvent.AddListener (CheckCraftability);
            parentController.itemRemovedEvent.AddListener (RemovedItem);
            //parentController.stackManipulator.splitFinishedEvent.AddListener ((arg0, arg1) => RemovedItem (null, null));

            craftButton.onClick.AddListener (Craft);
           // m_openBlueprintListButton.onClick.AddListener (() => ClearAllItemsFromInventory ()); // clear all items when going back to blueprint list
        }

        public void Init () {
            if (startOff) {
                Active = false;
            } else {
                UpdateCraftability = true;
                Active = true;
            }
            // Create the list of possible things to craft
            LoadAllBlueprintDatas ();
            UpdateBlueprintList ();
            CheckCraftability (null, null);
            // Init-clear the display boxes
            ClearAllComponents ();
        }

        [NaughtyAttributes.Button]
        public void UpdateBlueprintList () {
            craftableBlueprints.Clear ();
            craftableBlueprints = AllowedBlueprints ();
            UpdateDisplayedBlueprints ();
        }

        public void UpdateDisplayedBlueprints (ItemBlueprintType typeToDisplay = ItemBlueprintType.ANY) {
            /*if (m_blueprintListParent != null && m_blueprintPrefab != null) {
                foreach (KeyValuePair<ItemBlueprintData, int> kvp in KnownBlueprints) { // add all known blueprints
                    if (!displayedBlueprintDict.ContainsKey (kvp.Key)) {
                        GameObject newBlueprintBox = Instantiate (m_blueprintPrefab, m_blueprintListParent);
                        newBlueprintBox.name = kvp.Key.m_id;
                        Inventory_BlueprintBox boxComponent = newBlueprintBox.GetComponent<Inventory_BlueprintBox> ();
                        boxComponent.Init (kvp.Key);
                        boxComponent.m_toolTip.tooltipCanvasParent = m_blueprintTooltipParent;
                        boxComponent.m_itembox.targetBox.tooltip.tooltipCanvasParent = exampleResult.m_tooltipBoxParent; // a little hacky lol
                        displayedBlueprintDict.Add (kvp.Key, boxComponent);
                        // For testing
                        boxComponent.m_clickable.onMouseDownEvent.AddListener ((arg0) => ClickBlueprint (boxComponent));
                    }
                }
                foreach (KeyValuePair<ItemBlueprintData, Inventory_BlueprintBox> kvp in displayedBlueprintDict) {
                    if (typeToDisplay == ItemBlueprintType.ANY || kvp.Key.m_type == typeToDisplay) // we can also show only certain types!
                    {
                        int visibility = -1;
                        KnownBlueprints.TryGetValue (kvp.Key, out visibility);
                        if (craftableBlueprints.Contains (kvp.Key) && visibility == 1) { // make craftable ones visible normally
                            kvp.Value.TooltipVisible = true;
                            kvp.Value.Visible = true;
                        } else {
                            if (kvp.Value.m_data.m_visibility == ItemBlueprintVisibility.VISIBLE_UNKNOWN) { // visible unknown! change tooltip!
                                kvp.Value.TooltipVisible = false;
                                kvp.Value.Visible = true;
                            } else {
                                kvp.Value.Visible = false;
                            };
                        }
                    } else { // hide if not of type
                        kvp.Value.Visible = false;
                    }
                }

            } else {
                Debug.LogWarning ("No parent or prefab set for the blueprint display!");
            }*/
        }

        public void CheckCraftability (InventoryController controller, Item_DragAndDrop item) { // Checks if it's possible to make anything!
            if (Active && UpdateCraftability) {
                //   Debug.Log ("Item has been moved to crafting parent controller " + parentController.name);
                targetBlueprint = null;
                foreach (ItemBlueprintData data in craftableBlueprints) {
                    //       Debug.Log ("Can craft: " + data.m_id + " = " + CanCraft (data));
                    if (CanCraft (data)) {
                        targetBlueprint = data;
                        SetCraftingButtonActive (true);
                        CreateExampleCopy (data, data.m_stackAmount);
                        if (!data.m_generic) {
                            return;
                        }
                    }
                    if (targetBlueprint != null) { // e.g. we set it to a generic
                        return;
                    }
                }
            }
            SetCraftingButtonActive (false);
            CreateExampleCopy (null, -1);
            targetBlueprint = null;
        }
        public void RemovedItem (InventoryController controller, Item_DragAndDrop item) {
            if (Active && UpdateCraftability) {
                CheckCraftability (null, null);
                //RemoveComponentFromDisplay (item.targetBox.data);
            }
        }

        void ReturnItemToPlayerInventory (Item_DragAndDrop item) {
            // Super ugly stuff incoming
            if (item == null) {
                Debug.LogError ("Tried to return null item, quitting", gameObject);
                return;
            }
            bool success = false;
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

        void CreateExampleCopy (ItemBlueprintData example, int amount) {
            if (example == null) {
                exampleResult.gameObject.SetActive (false);
            } else {
                if (example == exampleResult.m_data && exampleResult.gameObject.activeInHierarchy && exampleResult.m_itembox.targetBox.StackSize == amount) {
                    return; // since this is literally done in Update due to stupidity, we don't want to update this every frame unless something's changed
                }
                exampleResult.ClearBoxes ();
                exampleResult.gameObject.SetActive (true);
                exampleResult.m_tooltipBoxParent = m_blueprintTooltipParent;
                exampleResult.Init (example, false);
                //exampleResult.StackSize = amount;
            }
        }

        void AddComponentToDisplay (ItemData show, int amount, int index) {
            if (index >= displayInventory.Length) {
                //                Debug.LogWarning ("Blueprint longer than three items not supported!");
                return;
            }
            if (show == null) {
                displayInventory[index].gameObject.SetActive (false);
            } else {
                displayInventory[index].gameObject.SetActive (true);
                displayInventory[index].SetItemBoxData (show);
                displayInventory[index].StackSize = amount;
            }
        }

        void RemoveComponentFromDisplay (ItemData component) {
            foreach (UI_ItemBox box in displayInventory) {
                if (box.data == component) {
                    box.SetItemBoxData (null);
                    box.gameObject.SetActive (false);
                    return;
                }
            }
        }
        void ClearAllComponents () {
            foreach (UI_ItemBox box in displayInventory) {
                box.SetItemBoxData (null);
                box.gameObject.SetActive (false);
            }
        }

        public bool CanCraft (ItemBlueprintData data) {
            List<int> results = new List<int> { };
            // make a list of required results
            int index = 0;
            foreach (BlueprintComponent component in data.m_componentsNeeded) {
                int itemCount = 0;
                if (component.trait == ItemGameTrait.NONE) {
                    itemCount = parentController.CountItem (component.data);
                } else {
                    foreach (Item_DragAndDrop item in parentController.ItemsByGameTrait (component.trait)) {
                        itemCount += item.targetBox.StackSize;
                    }
                }
                if (itemCount > 0) {
                    if (itemCount >= component.amount) {
                        results.Add (1);
                        AddComponentToDisplay (component.data, component.amount, index);
                    } else {
                        results.Add (0);
                    }
                } else {
                    results.Add (0);
                }
                index++;
            }
            return !results.Contains (0);
        }

        void Craft () {
            if (crafting == null) {
                crafting = StartCoroutine (Crafting (targetBlueprint));
            } else {
                SetCraftingButtonActive (false);
            }
        }

        IEnumerator Crafting (ItemBlueprintData data) {
            bool success = false;
            foreach (BlueprintComponent component in data.m_componentsNeeded) {
                if (component.trait == ItemGameTrait.NONE) {
                    success = parentController.DestroyItemAmount (component.data, component.amount) == 0;
                } else {
                    success = parentController.DestroyItemAmountByTrait (component.trait, component.amount) == 0;
                }
            }
            if (success) {
                //ClearAllItemsFromInventory ();
                success = craftingResultsController.AddItemStackable (data.m_result, data.m_stackAmount, true) == 0;
                if (data.m_additionalResults.Length > 0) {
                    foreach (BlueprintComponent extra in data.m_additionalResults) {
                        success = craftingResultsController.AddItemStackable (extra.data, extra.amount, true) == 0;
                    }
                }
                Debug.Log ("<color=green> Successfully crafted " + data.m_result.m_id + " </color>");
                itemCraftedEvent.Invoke (parentController, data);
                ShowResultsPanel (true);
            }
            if (!success) {
                Debug.LogWarning ("<color=red> Failed to craft an object! </color>");
            }
            yield return null;
            CheckCraftability (null, null);
            crafting = null;
        }
        public void ShowResultsPanel (bool show) {
            if (show) {
                //GetComponent<Animator> ().SetTrigger ("showResult ");
            } else {
                //GetComponent<Animator> ().SetTrigger ("hideResult ");
            }
        }
        public bool Active {
            get {
                return m_active;
            }
            set {
                m_active = value;
                UpdateCraftability = value;
                if (value) {
                    UpdateBlueprintList (); // Update the list of blueprints!
                }
                SetVisible ();
                if (returnAllItemsOnClose && !value) { // we also do this on start, just in case
                    ClearAllItemsFromInventory ();
                }
                ClearAllComponents ();
                if (!value) {
                    ShowResultsPanel (false);
                }
            }
        }

        public bool UpdateCraftability {
            get {
                return m_checkingActive;
            }
            set {
                m_checkingActive = value;
            }
        }

        void SetCraftingButtonActive (bool active) {
            craftButton.interactable = active;
        }

        public void ClearAllItemsFromInventory () {
            List<Item_DragAndDrop> copyList = new List<Item_DragAndDrop> { };
            foreach (Item_DragAndDrop item in parentController.allItemBoxes) {
                copyList.Add (item);
            }
            foreach (Item_DragAndDrop item in craftingResultsController.allItemBoxes) {
                if (!copyList.Contains (item)) {
                    copyList.Add (item);
                };
            }
            foreach (Item_DragAndDrop item in copyList) {
                ReturnItemToPlayerInventory (item);
            }
        }

        public List<ItemBlueprintData> AllowedBlueprints () {
            List<ItemBlueprintData> returnList = new List<ItemBlueprintData> { };
            if (craftableTypes.Contains (ItemBlueprintType.ANY)) { // Any -> create a copy of the alldata list and send it back!
                foreach (KeyValuePair<ItemBlueprintData, int> kvp in KnownBlueprints) {
                    // Now we check the actual data of it, > 0 means it's visible and craftable
                    if (kvp.Value > 0) {
                        returnList.Add (kvp.Key);
                    }
                }
            } else {
                foreach (ItemBlueprintType type in craftableTypes) {
                    if (blueprintLookupDict.ContainsKey (type)) {
                        foreach (ItemBlueprintData data in blueprintLookupDict[type]) {
                            if (HasLoadedItem (data) > 0) {
                                returnList.Add (data);
                            }
                        }
                    };
                };
            }
            return returnList;
        }
        int HasLoadedItem (ItemBlueprintData data) {
            if (data.m_visibility != ItemBlueprintVisibility.VISIBLE_FROM_START) {
                int returnVal = -1;
                KnownBlueprints.TryGetValue (data, out returnVal);
                return returnVal;
            } else { // visible from start >> return true
                return 1;
            }

        }

        void SetVisible () { // pretty code here for active/deactivate
            canvasGroup.interactable = Active;
            canvasGroup.blocksRaycasts = Active;
            canvasGroup.alpha = Active ? 1f : 0f;
            // Null the displays..
            foreach (UI_ItemBox box in displayInventory) {
                box.SetItemBoxData (null);
            }
        }

        void LoadAllBlueprintDatas () {
            //allBlueprintDatas.Clear (); // local list

            blueprintLookupDict.Clear (); // clear previous
            // Add to lookup dictionary for quick lookup of blueprints according to type
            foreach (KeyValuePair<ItemBlueprintData, int> kvp in KnownBlueprints) { // We don't add nones or anys, to allow for easy disabling
                if (kvp.Key.m_type != ItemBlueprintType.NONE || kvp.Key.m_type != ItemBlueprintType.ANY) {
                    if (blueprintLookupDict.ContainsKey (kvp.Key.m_type)) {
                        blueprintLookupDict[kvp.Key.m_type].Add (kvp.Key);
                        //Debug.Log (" < color = blue > Added blueprint to list: " + data.m_type + " < / color > ");
                    } else {
                        List<ItemBlueprintData> newList = new List<ItemBlueprintData> { };
                        newList.Add (kvp.Key);
                        blueprintLookupDict.Add (kvp.Key.m_type, newList);
                        //Debug.Log (" < color = blue > Created new list: " + data.m_type + " < / color > ");
                    }
                }
            };
        }

        public static Dictionary<ItemBlueprintData, int> KnownBlueprints {
            get {
                return m_knownBlueprints;
            }
            set {
                m_knownBlueprints = value;
            }
        }

        public static void AddKnownBlueprint (ItemBlueprintData data, int visibility = 1, bool showpopup = true) {
            if (!KnownBlueprints.ContainsKey (data)) {
                // Debug.Log ("Adding blueprint to known blueprints: " + data.m_id);
                KnownBlueprints.Add (data, visibility);
                if (showpopup) {
                    //UIManager.NewBlueprintAddedPopup (data, visibility);
                }
            } else {
                if (showpopup && KnownBlueprints[data] != visibility) { // show a popup if the visibility actually changes
                    //UIManager.NewBlueprintAddedPopup (data, visibility);
                }
                KnownBlueprints[data] = visibility; // set it to visibility, otherwise

            }
        }

        void LoadKnownBlueprints () {
            //if (SaveManager.instance.IsNewGame) { // Clear saved blueprints if we're unsaved!
            //    ClearSavedBlueprints ();
            //}
            KnownBlueprints.Clear ();
            if (KnownBlueprints.Count == 0) {
                foreach (ItemBlueprintData data in InventoryController.AllBlueprintData) {
                    if (data.m_visibility == ItemBlueprintVisibility.VISIBLE_FROM_START) { // if it's not invisible, we can safely add it
                        AddKnownBlueprint (data, 1, false);
                    } else {
                        // Check if it -should- be visible - if it's saved, it exists
                        if (ES3.KeyExists (SaveManager.instance.CurrentSlot + "_Blueprint_" + data.m_id)) {
                            AddKnownBlueprint (data, ES3.Load<int> (SaveManager.instance.CurrentSlot + "_Blueprint_" + data.m_id), false); // add to known, but with the visibility int
                        } else if (data.m_visibility == ItemBlueprintVisibility.VISIBLE_UNKNOWN) { // add as 0 to known
                            AddKnownBlueprint (data, 0, false);
                        } else if (data.m_visibility == ItemBlueprintVisibility.CRAFTABLE_INVISIBLE) { // add as 2! v special
                            AddKnownBlueprint (data, 2, false);
                        }
                    }
                }
                Debug.Log ("Known blueprints count: " + KnownBlueprints.Count);
            }
        }

        [NaughtyAttributes.Button]
        public static void SaveKnownBlueprints () {
            foreach (KeyValuePair<ItemBlueprintData, int> kvp in KnownBlueprints) {
                ES3.Save<int> (SaveManager.instance.CurrentSlot + "_Blueprint_" + kvp.Key.m_id, kvp.Value, SaveManager.instance.settings);
                SaveManager.instance.AddSlotKey (SaveManager.instance.CurrentSlot + "_Blueprint_" + kvp.Key.m_id);
            }
        }

        [NaughtyAttributes.Button]
        void ClearSavedBlueprints () {
            foreach (KeyValuePair<ItemBlueprintData, int> kvp in KnownBlueprints) {
                if (ES3.KeyExists (SaveManager.instance.CurrentSlot + "_Blueprint_" + kvp.Key.m_id)) {
                    ES3.DeleteKey (SaveManager.instance.CurrentSlot + "_Blueprint_" + kvp.Key.m_id);
                }
            }
        }

        [NaughtyAttributes.Button]
        void DebugAddItem () {
            AddKnownBlueprint (InventoryController.GetBlueprintById ("blueprint1"));
        }

        [NaughtyAttributes.Button]
        void DebugAddItem2 () {
            AddKnownBlueprint (InventoryController.GetBlueprintById ("blueprint2"));
        }

        public void FilterDisplay (int type) { // HARDCODED BECAUSE STUPID ONCLICK IS STUPID
            switch (type) {
                case 1:
                    {
                        UpdateDisplayedBlueprints (ItemBlueprintType.GENERAL);
                        break;
                    }
                case 2:
                    {
                        UpdateDisplayedBlueprints (ItemBlueprintType.CLAYWORK);
                        break;
                    }
                case 3:
                    {
                        UpdateDisplayedBlueprints (ItemBlueprintType.SOLUTION);
                        break;
                    }
                case 4:
                    {
                        UpdateDisplayedBlueprints (ItemBlueprintType.DISMANTLE);
                        break;
                    }
                default:
                    {
                        UpdateDisplayedBlueprints ();
                        break;
                    }
            }
        }

        public void ClickBlueprint (Inventory_BlueprintBox clickedBox) {
            // Can we craft it?
            List<BlueprintComponent> missingComponents = CheckHasComponents (clickedBox.m_data);
            if (missingComponents.Count == 0) {
                foreach (BlueprintComponent comp in clickedBox.m_data.m_componentsNeeded) {
                    // Steal all necessary items from the necessary inventories!
                    InventoryController targetInventory = GameManager.instance.PlayerInventory;
                    Item_DragAndDrop targetItem = null;
                    if (comp.trait == ItemGameTrait.NONE && comp.data != null) {
                        targetInventory = GameManager.instance.GetTargetInventory (comp.data);
                        targetItem = targetInventory.ItemsByData (comp.data) [0]; // should be safe, since we checked it!
                        Debug.Log ("<color=purple>Clicked blueprint " + clickedBox.m_data.m_id + " and retrieved item " + targetItem + " from inventory " + targetInventory + "(data)</color>");
                    } else {
                        targetInventory = GameManager.instance.GetTargetInventory (comp.trait);
                        targetItem = targetInventory.ItemsByGameTrait (comp.trait) [0];
                        Debug.Log ("<color=purple>Clicked blueprint " + clickedBox.m_data.m_id + " and retrieved item " + targetItem + " from inventory " + targetInventory + "(trait)</color>");
                    }
                    parentController.TryTakeItemFromInventory (targetItem, null);
                }
                m_closeBlueprintListButton.onClick.Invoke (); // switch!
            } else {
                // Flash some fancy red stuff on the components that are missing, and let it go
            }
        }

        // Checks if we have the necessary components, and returns a list of those that are -missing- (also if there aren't enough)
        public List<BlueprintComponent> CheckHasComponents (ItemBlueprintData data) {
            List<BlueprintComponent> missingComponentList = new List<BlueprintComponent> { };
            foreach (BlueprintComponent comp in data.m_componentsNeeded) {
                InventoryController targetInventory = GameManager.instance.PlayerInventory;
                int itemCount = 0;
                if (comp.trait == ItemGameTrait.NONE && comp.data != null) {
                    targetInventory = GameManager.instance.GetTargetInventory (comp.data);
                    itemCount = targetInventory.CountItem (comp.data);
                } else {
                    targetInventory = GameManager.instance.GetTargetInventory (comp.trait);
                    itemCount = targetInventory.ItemsByGameTrait (comp.trait).Count;
                }
                if (itemCount <= 0 || itemCount < comp.amount) {
                    missingComponentList.Add (comp);
                }
            }
            return missingComponentList;
        }

        // Update is called once per frame
        void Update () {
            if (Active && UpdateCraftability) {
                CheckCraftability (null, null);
            }
        }
    }
}