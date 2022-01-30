using System.Collections;
using UnityEngine;

public enum ItemBlueprintType {
    NONE = 0000,
    ANY = 1000,
    GENERAL = 2000,
    CLAYWORK = 3000,
    SOLUTION = 4000,
    DISMANTLE = 5000,
}
public enum ItemBlueprintVisibility {
    VISIBLE_FROM_START = 0000,
    VISIBLE_UNKNOWN = 1000,
    INVISIBLE = 2000,
    VISIBLE = 3000,
    CRAFTABLE_INVISIBLE = 4000,
}

[System.Serializable]
public class BlueprintComponent {
    public ItemData data;
    [Tooltip ("Only used in cases where there is no itemdata set at all")]
    public string alternateDisplayName = "<ERROR>";
    public ItemGameTrait trait = ItemGameTrait.NONE;
    [Tooltip ("Set to 0 to leave the item untouched after crafting")]
    public int amount = 1;
}

[CreateAssetMenu (fileName = "Data", menuName = "Item Blueprint", order = 1)]
public class ItemBlueprintData : ScriptableObject {
    public string m_id;
    public string m_displayName = "Craft Item";
    public ItemBlueprintVisibility m_visibility = ItemBlueprintVisibility.VISIBLE_FROM_START;
    public ItemBlueprintType m_type = ItemBlueprintType.ANY;
    [Tooltip ("Set to true if you want this blueprint to only be picked -after- any other non-generic blueprint (e.g. not trait-based)")]
    public bool m_generic = false;
    [Header ("Add data for a specific data, or use another trait than NONE to allow any itemData with that trait (but preference for what is in data)")]
    public BlueprintComponent[] m_componentsNeeded;
    [Header ("The resulting itemData from the successful combination, as well as an optional stack amount (1 = is global default)")]
    public ItemData m_result;
    public int m_stackAmount = 1;
    [Header ("Additional items that will be spawned as a result (but not necessarily displayed)")]
    public BlueprintComponent[] m_additionalResults;

    public int CompatibleData (ItemData data) { // returns -1 if it is not compatible, otherwise return the needed amount
        foreach (BlueprintComponent component in m_componentsNeeded) {
            if (component.data == data) {
                return component.amount;
            }
            if (component.trait != ItemGameTrait.NONE) {
                if (data.HasGameTrait (component.trait)) {
                    return component.amount;
                }
            }
        }
        return -1;
    }

    [NaughtyAttributes.Button]
    void SetIDToName () {
        m_id = name;
    }

}