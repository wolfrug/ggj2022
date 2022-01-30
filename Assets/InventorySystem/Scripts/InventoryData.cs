using System.Collections;
using UnityEngine;

public enum InventoryType {
    DEFAULT = 0000,
    NONE = 0100,
    PLAYER = 1000,
    PLAYER_CLUES = 1001,
    NPC = 1100,
    LOOTABLE = 2000,
    LOOTABLE_SINGLETON = 2100, // for when you have just the one inventory 
    CRAFTING = 3000,
    CRAFTING_RESULTS = 3300,
    ACTIONBAR = 4000,
    TASKS = 5000,

}

public enum InventoryGameModifier { // various game-specific modifiers, e.g. inventory strenght or whatever
 NONE,
 SMALL,
 MEDIUM,
 LARGE,

 FOOD,
 CLAY,
 TRIBESMAN,
 WARRIOR,
 CLAYWORKER,
 SHAMAN,
 OTHER,
 SPECIAL_GRASS,
 SPECIAL_RUINS,
 HUNT_ANIMAL,
 HUNT_BOT,
}

[System.Serializable]
public class RandomizedInventoryItem {
    public ItemData data;
    public float weight = 1f;
    public bool guaranteed = true;
    public bool random_unique = true;
    public Vector2Int randomStackSize = new Vector2Int (1, 1);
}

[CreateAssetMenu (fileName = "Data", menuName = "Inventory Data", order = 1)]
public class InventoryData : ScriptableObject {
    public string m_id = "defaultInventory";
    public string m_displayName = "Inventory";
    public bool m_clearOnInit = true;
    public int m_defaultMaxSlots = 99;
    public GameObject m_inventoryItemPrefab;
    public InventoryType m_type = InventoryType.DEFAULT;
    public InventoryGameModifier[] m_modifiers = { InventoryGameModifier.NONE };
    public InventoryType[] m_allowContentFrom = { InventoryType.DEFAULT };

    [Header ("Uncheck 'guaranteed' to decrease the chance of the item spawning")]
    public RandomizedInventoryItem[] m_defaultContent;
    [Header ("Set to min and max number of randomly weighed items to spawn ('unique' items only ever spawn once)")]
    public Vector2Int m_minMaxRandomItemsSpawned = new Vector2Int (-1, -1);
    public GameObject m_inventoryCanvasPrefab;

    public bool AllowsContentFrom (InventoryType checkType) {
        foreach (InventoryType type in m_allowContentFrom) {
            if (checkType == type) {
                return true;
            }
        }
        return false;
    }
    public bool HasModifier (InventoryGameModifier checkType) {
        foreach (InventoryGameModifier type in m_modifiers) {
            if (checkType == type) {
                return true;
            }
        }
        return false;
    }

    [NaughtyAttributes.Button]
    void SetIdToName () {
        m_id = name;
    }
}