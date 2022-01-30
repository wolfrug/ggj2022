using System.Collections;
using UnityEngine;

public enum ItemTrait { // These work by positive connotation, i.e. if they don't have 'movable' then they are non-movable etc.
 NONE = 0000,
 SPLITTABLE = 1000,
 STACKABLE = 1100,
 CONSUMABLE = 3000,
 DRAGGABLE = 4000,
 HAS_TOOLTIP = 5000,
 DESTROYABLE = 6000, // means it will be destroyed when empty
 USEABLE = 7000, // means it can be consumed manually, but it won't create a context menu entry
 CAN_SPAWN_EMPTY = 8000, // means it can spawn with 0 in it
}

public enum ItemGameTrait { // specific for the game they are used in, basically
 NONE = 0000,
 ITEM = 1000, // Physical item

 CLUE = 2000, // A clue
 NEGATIVE_ADDITEM = 8000, // If they have this, that means they get a nastier prefab when added

}

[CreateAssetMenu (fileName = "Data", menuName = "Inventory Item Data", order = 1)]
public class ItemData : ScriptableObject {
    public string m_id;
    public string m_displayName;
    [TextArea]
    public string m_description;

    [Header ("{0} = description text, {1} = displayName, {2} = current stack amount {3} = max stack amount")]
    [TextArea]
    public string m_descriptionTextFormat = "{1}\n{0}\n\n\n{2}/{3}";
    [Header ("{0} = current stack, {1} = max stack (might not fit very well)")]
    public string m_stackNumberFormat = "{0}";
    [NaughtyAttributes.ShowAssetPreview]
    public Sprite m_image;
    public ItemTrait[] m_traits = { ItemTrait.DESTROYABLE, ItemTrait.STACKABLE, ItemTrait.SPLITTABLE, ItemTrait.DRAGGABLE, ItemTrait.HAS_TOOLTIP };
    [NaughtyAttributes.ReorderableList]
    public ItemGameTrait[] m_gameTraits = { ItemGameTrait.NONE };

    [Tooltip ("Which inventory types can it be added to (note: DEFAULT means it can be added to anything!")]
    public InventoryType[] m_permittedInventories = { InventoryType.DEFAULT };
    public int m_maxStackSize = 1;
    public int m_sizeInInventory = 1;

    public Vector2Int m_amountConsumedPerUse = new Vector2Int (1, 1);
    public int m_minimumNeededToConsume = 0; // Make this bigger to make the consume not trigger if it's less.
    public GameObject m_prefab;

    //public SFXClip[] m_soundClips;

    public bool HasTrait (ItemTrait trait) { // Helper function
        foreach (ItemTrait t in m_traits) {
            if (t == trait) {
                return true;
            }
        }
        return false;
    }
    public bool HasGameTrait (ItemGameTrait trait) { // Helper function
        foreach (ItemGameTrait t in m_gameTraits) {
            if (t == trait) {
                return true;
            }
        }
        return false;
    }
    public int ConsumeAmount (bool forceConsume = false) {
        if (HasTrait (ItemTrait.CONSUMABLE) || forceConsume || HasTrait (ItemTrait.USEABLE)) {
            return Mathf.Clamp (Random.Range (m_amountConsumedPerUse.x, m_amountConsumedPerUse.y), m_minimumNeededToConsume, m_amountConsumedPerUse.y);
        } else {
            return 0;
        }
    }

    public bool IsPermittedInventory (InventoryType type) { // returns 'true' for 'default'
        foreach (InventoryType checkType in m_permittedInventories) {
            if (checkType == InventoryType.DEFAULT || checkType == type) {
                return true;
            }
        }
        return false;
    }

    [NaughtyAttributes.Button]
    void SetIDToName () {
        m_id = name;
    }
}