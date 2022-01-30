using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Inventory {
    public class Inventory_BlueprintBox : MonoBehaviour {
        public ItemBlueprintData m_data;
        public TextMeshProUGUI m_displayName;
        public GenericTooltip m_toolTip;
        public GameObject m_tooltipBox;
        public Transform m_tooltipBoxParent;
        public Item_DragAndDrop m_itembox;
        public GenericClickable m_clickable;
        public List<Inventory_BlueprintBox> m_tooltipBoxes = new List<Inventory_BlueprintBox> { };
        [SerializeField]
        private bool m_active = true;
        private bool m_tooltipActive = true;

        public void Init (ItemBlueprintData data, bool showRequirements = true) {
            m_data = data;
            CreateTargetBox ();
            m_itembox.targetBox.SetItemBoxData (m_data.m_result);
            m_itembox.targetBox.StackSize = m_data.m_stackAmount;
            m_itembox.interactable = false;
            m_itembox.targetBox.draggable = false;
            m_itembox.targetBox.tooltip.tooltipCanvasParent = m_tooltipBoxParent;
            m_displayName.text = m_data.m_displayName;
            if (gameObject.activeInHierarchy) {
                StartCoroutine (SetupTooltip (showRequirements));
            };
        }

        void CreateTargetBox () {
            if (m_itembox.targetBox == null) {
                m_itembox.targetBox = Instantiate (m_data.m_result.m_prefab, m_itembox.transform).GetComponent<UI_ItemBox> ();
            } else {
                if (m_itembox.targetBox.data != m_data.m_result) {
                    //  Debug.LogWarning ("Initializing new box for blueprintbox - is this desired?", gameObject);
                    Destroy (m_itembox.targetBox.gameObject);
                    m_itembox.targetBox = Instantiate (m_data.m_result.m_prefab, m_itembox.transform).GetComponent<UI_ItemBox> ();
                }
            }
        }
        public void ClearBoxes () {
            foreach (Inventory_BlueprintBox box in m_tooltipBoxes) {
                Destroy (box.gameObject);
            }
            m_tooltipBoxes.Clear ();
        }

        IEnumerator SetupTooltip (bool showRequirements) {
            yield return new WaitUntil (() => m_toolTip.spawnedTooltip != null);
            m_tooltipBoxParent = m_toolTip.spawnedTooltip.transform.Find (m_toolTip.tooltiptextPath).parent;

            if (showRequirements) { // go through all requirements and show them
                for (int i = 0; i < m_data.m_componentsNeeded.Length; i++) {
                    GameObject newBox = Instantiate (m_tooltipBox, m_tooltipBoxParent);
                    Inventory_BlueprintBox boxComponent = newBox.GetComponent<Inventory_BlueprintBox> ();
                    m_tooltipBoxes.Add (boxComponent); // add to list
                    boxComponent.InitAsTooltip (m_data, i);
                }
            } else { //go through all results and show them
                // do one separate for the 'result' result
                // destroy all previous results lol
                ClearBoxes ();
                GameObject newBox = Instantiate (m_tooltipBox, m_tooltipBoxParent);
                Inventory_BlueprintBox boxComponent = newBox.GetComponent<Inventory_BlueprintBox> ();
                m_tooltipBoxes.Add (boxComponent); // add to list
                boxComponent.InitAsTooltipResults (m_data, m_data.m_stackAmount, m_data.m_result);
                for (int i = 0; i < m_data.m_additionalResults.Length; i++) {
                    newBox = Instantiate (m_tooltipBox, m_tooltipBoxParent);
                    boxComponent = newBox.GetComponent<Inventory_BlueprintBox> ();
                    m_tooltipBoxes.Add (boxComponent); // add to list
                    boxComponent.InitAsTooltipResults (m_data, i, m_data.m_additionalResults[i].data);
                }
            }
            // And then setup to hide/show it...
            HideTooltipComponents (!TooltipVisible, showRequirements);
        }

        public void InitAsTooltip (ItemBlueprintData data, int index) { // index == index number for required component
            m_data = data;
            CreateTargetBox ();
            if (data.m_componentsNeeded[index].data != null) { // set the data only if it exists, of course
                m_itembox.targetBox.SetItemBoxData (m_data.m_componentsNeeded[index].data);
                m_itembox.targetBox.StackSize = m_data.m_componentsNeeded[index].amount;
            }
            m_itembox.interactable = false;
            m_itembox.targetBox.draggable = false;
            if (m_data.m_componentsNeeded[index].trait == ItemGameTrait.NONE) { // If the data exists, use the display name of the item
                m_displayName.text = m_data.m_componentsNeeded[index].data.m_displayName;
            } else { // else use the alternate display name
                m_displayName.text = m_data.m_componentsNeeded[index].alternateDisplayName;
            }
            m_toolTip.IsActive = false;
            m_toolTip.enabled = false;
        }
        public void InitAsTooltipResults (ItemBlueprintData data, int amount, ItemData itemData) {
            m_data = data;
            CreateTargetBox ();
            if (itemData != null) { // set the data only if it exists, of course
                m_itembox.targetBox.SetItemBoxData (itemData);
                m_itembox.targetBox.StackSize = amount;
            }
            m_itembox.interactable = false;
            m_itembox.targetBox.draggable = false;
            m_displayName.text = itemData.m_displayName;
            m_toolTip.IsActive = true;
            m_toolTip.enabled = true;
        }

        void HideTooltipComponents (bool hide = true, bool showRequirements = true) {
            if (m_tooltipBoxes.Count > 0) {
                foreach (Inventory_BlueprintBox box in m_tooltipBoxes) {
                    box.Visible = !hide;
                }
                if (hide && showRequirements) {
                    m_toolTip.tooltiptext = "Components unknown";
                } else if (!hide && showRequirements) {
                    m_toolTip.tooltiptext = "Required components";
                } else {
                    m_toolTip.tooltiptext = "Crafting results";
                }
                if (m_toolTip.spawnedTooltip != null) {
                    m_toolTip.SetTooltipText (m_toolTip.tooltiptext);
                }
            } else {
                //   Debug.LogWarning ("This blueprintBox has no spawned tooltip boxes!", gameObject);
            }
        }

        public bool Visible {
            get {
                return m_active;
            }
            set {
                m_active = value;
                gameObject.SetActive (value);
            }
        }
        public bool TooltipVisible {
            get {
                return m_tooltipActive;
            }
            set {
                m_tooltipActive = value;
                HideTooltipComponents (!value);
            }
        }

    }
}