using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Inventory {

    [System.Serializable]
    public class UseItemComplete : UnityEvent<Item_UseItemSpot> { }

    [System.Serializable]
    public class UseItemItemUsed : UnityEvent<ItemData, int> { }

    // A spot where you can drag an item and then 'use' it directly, if there is a 'blueprint' for it
    public class Item_UseItemSpot : MonoBehaviour {

        [Tooltip ("Definitions that this particular spot accepts: note that they all count down towards the same end goal!")]
        public UseItemData m_data;
        public GenericUIPool m_pool;
        public Item_DragTarget m_dragTarget;
        public GenericTooltip m_tooltip;
        public Image m_displayImage;
        public TextMeshProUGUI m_displayNameText;
        public Inventory_StackManipulator m_stackManipulator;
        // For ink integration
        //public InkVariableSetter m_inkVariableSetter;

        [Tooltip ("Count up (false) or down (true)")]
        public bool m_countDown = true;
        [SerializeField]
        private bool m_active = true;
        [SerializeField]
        private bool m_completed = false;

        //private Dictionary<ItemData, UseItemDefinition> m_definitionsDict = new Dictionary<ItemData, UseItemDefinition> { };
        public UseItemComplete m_eventCompleteEvent;
        public UseItemComplete m_eventUncompleteEvent;
        public UseItemItemUsed m_usedItemEvent;
        public OnGenericPoolChanged m_changedEvent;
        public OnGenericPoolChanged m_currentValueEvent;
        private Doozy.Engine.UI.UIView m_doozyView;
        private CanvasGroup m_canvasGroup;

        [Tooltip ("Format text, hardcoded currently. {0} = effect per item used (can also be a range), {1} = description text for thing {2} = color")]
        [SerializeField]
        private string m_tooltipFormat = "Effect: {2}{0}</color> per item.<br>{1}";
        // Colors for various effect thresholds
        private string m_goodEffect = "<color=green>";
        private string m_mediumEffect = "<color=yellow>";
        private string m_badEffect = "<color=red>";
        // percentages at which the effect is considered bad/neutral/good. y is actually not used at all lol
        private Vector3 m_effectThresholds = new Vector3 (0.1f, 0.2f, 0.25f);

        // Start is called before the first frame update
        void Start () {
            if (m_dragTarget == null) {
                m_dragTarget = GetComponentInChildren<Item_DragTarget> ();
            }
            if (m_pool == null) {
                m_pool = GetComponentInChildren<GenericUIPool> ();
            }
            // Try get doozy view!
            m_doozyView = GetComponent<Doozy.Engine.UI.UIView> ();
            m_canvasGroup = GetComponent<CanvasGroup> ();
            // Add some listeners
            m_dragTarget.dragCompletedEvent.AddListener (CompleteDrag);
            m_dragTarget.dragEnteredEvent.AddListener (EnterDrag);
            m_dragTarget.dragExitedEvent.AddListener (ExitDrag);
            m_pool.m_currentValueEvent.AddListener (PoolValueChanged);
            // Copies of the changed etc events of the pool, for simplicity's sake
            m_pool.m_changedEvent.AddListener ((arg0) => m_changedEvent.Invoke (arg0));
            m_pool.m_currentValueEvent.AddListener ((arg0) => m_currentValueEvent.Invoke (arg0));
            if (m_stackManipulator != null) {
                m_stackManipulator.splitFinishedEvent.AddListener (FinishSplit);
            }
            if (Active && m_data != null) {
                Init (m_data);
            }
        }

        public void Init (UseItemData data, bool activateOnInit = true) {
            if (data != null) {
                m_data = data;
                /*m_definitionsDict.Clear ();
                foreach (UseItemDefinition def in data.m_definitions) {
                    if (def.m_requiredDatas.Length > 0) {
                        foreach (ItemData itemData in def.m_requiredDatas) {
                            m_definitionsDict.Add (itemData, def);
                        };
                    }
                }*/
                m_pool.SetMinMax (data.m_poolMinMax);
                m_pool.SetTextFormat (data.m_poolValueFormat);
                m_pool.SetChangeTextFormat (data.m_poolChangeValueFormat);
                m_pool.SetPool (data.m_poolValueStart);
                m_pool.m_isInteger = data.m_poolIsInteger;
                m_pool.UpdateDefaultsToCurrent ();

                if (m_displayNameText != null) {
                    m_displayNameText.text = data.m_displayName;
                }
                if (Completed) {
                    m_eventUncompleteEvent.Invoke (this);
                    m_completed = false;
                }
                if (activateOnInit) {
                    Active = true;
                }
                ExitDrag (null, null); // Set up the default exit drag text
                m_effectThresholds = data.m_effectThresholds;
            } else {
                Debug.LogError ("Tried to initialize UseSetItem with null data!", gameObject);
                m_pool.Reset ();
                if (m_displayNameText != null) {
                    m_displayNameText.text = "[ERROR]";
                }
                Active = false;
            }
        }

        public bool Active {
            get {
                return m_active;
            }
            set {
                m_active = value;
                if (!m_active) {
                    Hide ();
                } else {
                    Show ();
                }
            }
        }

        void Hide () {
            if (m_doozyView != null) {
                m_doozyView.Hide ();
            } else if (m_canvasGroup != null) {
                m_canvasGroup.alpha = 0f;
                m_canvasGroup.interactable = false;
                m_canvasGroup.blocksRaycasts = false;
            } else {
                gameObject.SetActive (false);
            }
        }
        void Show () {
            if (m_doozyView != null) {
                m_doozyView.Show ();
            } else if (m_canvasGroup != null) {
                m_canvasGroup.alpha = 1f;
                m_canvasGroup.interactable = true;
                m_canvasGroup.blocksRaycasts = true;
            } else {
                gameObject.SetActive (true);
            }
        }
        public bool Completed {
            get {
                return m_completed;
            }
        }
        void CompleteDrag (Item_DragAndDrop item, Item_DragTarget dragTarget) {
            if (dragTarget == m_dragTarget && Active) { // slightly pointless check :D
                if (IsDataValid (item.targetBox.data)) { // is this a targetable data?
                    if (item.targetBox.data.HasTrait (ItemTrait.USEABLE)) { // is it useable?
                        if ((m_countDown && m_pool.Value > m_pool.m_poolMinMax.x) || (!m_countDown && m_pool.Value < m_pool.m_poolMinMax.y)) {
                            if (m_stackManipulator != null && item.targetBox.data.HasTrait (ItemTrait.SPLITTABLE)) { // use stack manipulator?
                                Debug.Log ("Consuming " + item.targetBox.data.m_displayName + " in use item " + m_data.m_displayName + "using stack manipulator");
                                TryStackManipulator (item);
                            } else { // note: will only consume -one- item in this case
                                Debug.Log ("Consuming " + item.targetBox.data.m_displayName + " in use item " + m_data.m_displayName + " not using stack manipulator");
                                ConsumeItem (item, GetConsumeAmount (item.targetBox.data));
                            };
                        };
                    } else {
                        Debug.LogWarning ("Item " + item.targetBox.data.m_id + " does not have USEABLE trait!");
                    }
                }
            }
        }

        void EnterDrag (Item_DragAndDrop item, Item_DragTarget dragTarget) { // Update the tooltip with stuff
            if (dragTarget == m_dragTarget && Active) { // slightly pointless check :D
                if (IsDataValid (item.targetBox.data)) { // is this a targetable data?
                    if (item.targetBox.data.HasTrait (ItemTrait.USEABLE)) { // is it useable?
                        if ((m_countDown && m_pool.Value > m_pool.m_poolMinMax.x) || (!m_countDown && m_pool.Value < m_pool.m_poolMinMax.y)) {
                            UpdateTooltip (item);
                            return;
                        }
                    }
                }
            }
            m_tooltip.SetTooltipText (m_data.m_description + "<br>" + "No effect!");
        }
        void ExitDrag (Item_DragAndDrop item, Item_DragTarget dragTarget) {
            m_tooltip.SetTooltipText (m_data.m_description);
        }

        public bool IsDataValid (ItemData data) {
            if (m_data.GetDefinition (data) != null) {
                //  Debug.Log ("Found valid definition");
                return true;
            } else {
                // Debug.Log ("No valid definition found");
                return false;
            }
        }
        public UseItemDefinition GetDefinition (ItemData data) {
            return m_data.GetDefinition (data);
        }

        void UpdateTooltip (Item_DragAndDrop item) {
            ItemData data = item.targetBox.data;
            Vector2Int changePerAmount = GetDefinition (data).m_effectPerUseVector;
            Vector2Int consumedPerUse = data.m_amountConsumedPerUse;
            if (consumedPerUse.x < 1) { // if 0, we still count it as 1
                consumedPerUse.x = 1;
            }
            Vector2Int minMaxChange = new Vector2Int (changePerAmount.x * consumedPerUse.x, changePerAmount.y * consumedPerUse.y);
            string effect = minMaxChange.x.ToString () + "-" + minMaxChange.y.ToString ();
            if (minMaxChange.x == minMaxChange.y) { // if they're the same, just use one
                effect = minMaxChange.x.ToString ();
            }

            // Get color
            string color = "<color=black>";
            float averageTotalEffect = (minMaxChange.y / m_data.m_poolMinMax.y); // how large a percentage of the total can we affect?
            if (averageTotalEffect <= m_effectThresholds.x) {
                color = m_badEffect;
            } else if (averageTotalEffect <= m_effectThresholds.z) {
                color = m_mediumEffect;
            } else {
                color = m_goodEffect;
            }

            // Update tooltip
            m_tooltip.SetTooltipText (string.Format (m_tooltipFormat, effect, GetDefinition (data).m_description, color));

        }
        void PoolValueChanged (float newValue) {
            if (m_pool.Value >= m_pool.m_poolMinMax.y) {
                if (!Completed) {
                    m_eventCompleteEvent.Invoke (this);
                    m_completed = true;
                }
            } else if (Completed) {
                m_completed = false;
                m_eventUncompleteEvent.Invoke (this);
            }
        }
        void TryStackManipulator (Item_DragAndDrop item) {
            m_stackManipulator.StartManipulator (item, null);
        }
        void FinishSplit (Item_DragAndDrop item, int amount) { // Listen to split event
            // First we -return the full sum- since we're using the stack manipulator in a different way
            item.targetBox.StackSize += amount;
            int consumeAmount = 0;
            for (int i = 0; i < amount; i++) {
                consumeAmount += GetConsumeAmount (item.targetBox.data);
            }
            ConsumeItem (item, consumeAmount);
        }

        int GetConsumeAmount (ItemData data) {
            int outValue = 0;
            UseItemDefinition definition = GetDefinition (data);
            if (definition != null) {
                outValue = data.ConsumeAmount (); // handle the randomization here -> remember that this does not actually consume anything!
            }
            return outValue;
        }

        void ConsumeItem (Item_DragAndDrop item, int amount) {
            if ((m_countDown && m_pool.Value > m_pool.m_poolMinMax.x) || (!m_countDown && m_pool.Value < m_pool.m_poolMinMax.y)) {
                Debug.Log ("Consume item of " + item + " starting with amount " + amount);
                ItemData targetData = item.targetBox.data;
                int consumeAmount = amount;
                int changeLoops = 0;
                if (consumeAmount > 0) { // we can use items without consuming them
                    //if (consumeAmount == 0) { consumeAmount = 1; }; // Default to 1
                    int maxLeft = (int) m_pool.Value;
                    if (!m_countDown) {
                        maxLeft = (int) (m_pool.m_poolMinMax.y - m_pool.Value);
                    };
                    Debug.Log ("Max Left " + maxLeft);
                    int changePerAmount = GetDefinition (targetData).m_effectPerUse;
                    int targetChangedAmount = 0;
                    Debug.Log ("Change loops: " + changeLoops + " targetchangeamount: " + targetChangedAmount);
                    for (int i = 0; i < consumeAmount; i++) {
                        Debug.Log ("Looping: maxLeft: " + maxLeft + " changeLoops: " + changeLoops + "Total amount: " + amount);
                        if (item.targetBox.StackSize > 0 && maxLeft > 0) {
                            maxLeft -= changePerAmount;
                            if (amount > 0) {
                                item.targetBox.StackSize--;
                            };
                            changeLoops++;
                            targetChangedAmount += changePerAmount;
                            changePerAmount = GetDefinition (targetData).m_effectPerUse;
                        } else {
                            break;
                        }
                    }
                    int changedAmount = 0;
                    if (m_countDown) {
                        targetChangedAmount *= -1;
                        changedAmount = (int) m_pool.ChangePoolR (targetChangedAmount);
                    } else {
                        changedAmount = (int) m_pool.ChangePoolR (targetChangedAmount);
                    }
                    // Sound!
                    //item.audioSource.PlayRandomType (SFXType.UI_INV_USE);

                    if (item.targetBox.StackSize == 0 && targetData.HasTrait (ItemTrait.DESTROYABLE)) {
                        Debug.Log ("Destroying item in useitemspot " + item.targetBox.data.m_displayName);
                        DestroyBox (item);
                    }
                    ItemRemovedEvent (item);
                }
                // Spawn items, omg
                SpawnItems (GetDefinition (targetData), changeLoops);
                m_usedItemEvent.Invoke (targetData, changeLoops);

                ExitDrag (null, null); // to reset the tooltip
            } else {
                Debug.LogError ("Somehow the double-double check failed and this happened. No idea how.");
            }
        }
        void DestroyBox (Item_DragAndDrop target) {
            InventoryController targetObjectParent = target.GetComponentInParent<InventoryController> ();
            if (targetObjectParent == null) { // No parent? Floating magical lonely boy. Oh well, kill then!
                Destroy (target.gameObject);
            } else { // send it to them to destroy
                //   Debug.Log ("Target object parent is: " + targetObjectParent, targetObjectParent.gameObject);
                //   Debug.Log ("Target is: " + target);
                targetObjectParent.DestroyBox (target);
            }
        }

        void SpawnItems (UseItemDefinition itemDefinition, int amount) {
            if (amount < 1) { amount = 1; }; // we always spawn at least one item, right?
            if (itemDefinition.m_useItemResult.Length > 0) {
                List<RandomizedInventoryItem> itemsToSpawn = InventoryController.GenerateRandomInventoryContent (itemDefinition.m_useItemResult, new Vector2Int (amount, amount));
                foreach (RandomizedInventoryItem item in itemsToSpawn) {
                    if (item.data != null) {
                        int randomStackSize = Mathf.Clamp (UnityEngine.Random.Range (item.randomStackSize.x, item.randomStackSize.y), 0, item.data.m_maxStackSize);
                        if ((randomStackSize > 0 || item.data.HasTrait (ItemTrait.CAN_SPAWN_EMPTY)) && item.data != null) { // This setup allows for null datas, which can empty out slots!
                            InventoryController targetInventory = GameManager.instance.GetTargetInventory (item.data);
                            int removedLeft = targetInventory.AddItemStackable (item.data, randomStackSize);
                            if (removedLeft != 0) {
                                Debug.LogWarning ("Failed to add the required amount of item " + item.data.m_id + " (" + randomStackSize + ")");
                            } else {
                                Debug.Log ("<color=green>Added " + randomStackSize + " of item " + item.data.m_id + "!</color>");
                            }
                            GameManager.NewItemPopup (item.data, randomStackSize - removedLeft);
                            /*if (item.data.HasGameTrait (ItemGameTrait.TASK)) {
                                UIManager.TaskAdded (item.data, randomStackSize - removedLeft);
                            } else {
                                UIManager.NewItemPopup (item.data, randomStackSize - removedLeft);
                            }*/
                        };
                    };
                }
            }
        }

        void ItemRemovedEvent (Item_DragAndDrop item) {
            InventoryController targetObjectParent = item.GetComponentInParent<InventoryController> ();
            if (targetObjectParent != null) { // found parent!
                targetObjectParent.itemRemovedEvent.Invoke (targetObjectParent, item);
            }
        }

        public void UpdateImage (ItemData item, int amount) { // amount is just here to let you do this from the event!
            if (item != null) {
                SetImage (item.m_image);
            } else {
                SetImage (null);
            }
        }

        public void SetImage (Sprite newImage) {
            if (m_displayImage != null) {
                if (newImage != null) {
                    m_displayImage.gameObject.SetActive (true);
                    m_displayImage.sprite = newImage;
                } else { // hide ze image
                    m_displayImage.gameObject.SetActive (false);
                }
            }
        }

        [NaughtyAttributes.Button]
        public void Reset () {
            Init (m_data);
        }
    }
}