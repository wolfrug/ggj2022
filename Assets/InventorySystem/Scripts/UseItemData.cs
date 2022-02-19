using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UseItemType {
    NONE = 0000,
    USETARGET = 1000,
}

[System.Serializable]
public class UseItemDefinition {
    public ItemData[] m_requiredDatas;
    [Tooltip ("Any item with this game trait (unless it already exists as a specific item!)")]
    public ItemGameTrait[] m_gameTraits;
    [Tooltip ("Any trait = true -> any of these traits are good, false -> you need ALL of these traits!")]
    public bool m_anyTrait = false;
    [TextArea]
    public string m_description = "Use item";
    [Tooltip ("How many pips it counts up or down per nr of item used")]
    public Vector2Int m_effectPerUseVector = new Vector2Int (1, 1);
    [Tooltip ("Used specifically by the consequence system to determine their likelihood of being picked")]
    public float m_weight = 1f;

    [Tooltip ("Using this item will result in these items (maybe) being added to your inventory instead")]
    public RandomizedInventoryItem[] m_useItemResult;
    public int m_effectPerUse {
        get {
            return Random.Range (m_effectPerUseVector.x, m_effectPerUseVector.y);
        }
    }
    public RandomizedInventoryItem GetRandomResult () {
        if (m_useItemResult.Length > 0) {
            List<RandomizedInventoryItem> randomList = Inventory.InventoryController.GenerateRandomInventoryContent (m_useItemResult, new Vector2Int (1, 1));
            return randomList[Random.Range (0, randomList.Count)];
        } else {
            return null;
        }
    }
}

[CreateAssetMenu (fileName = "Data", menuName = "Use Item Data", order = 1)]
public class UseItemData : ScriptableObject {
    public string m_id;
    public UseItemType m_type = UseItemType.NONE;
    public string m_displayName = "Use Item";
    [TextArea]
    public string m_description = "";
    public Vector2 m_poolMinMax = new Vector2 (0f, 1f);
    public bool m_poolIsInteger = true;
    public int m_poolValueStart = 0;
    [Tooltip ("By default, 0 = current pool, 1 = max pool, 2 = min pool, 3 = default min, 4 = default max")]
    public string m_poolValueFormat = "{0}/{4}";
    [Tooltip ("By default, 0 = change value, 1 = optional + if positive, 2 = current pool")]
    public string m_poolChangeValueFormat = "{1}{0}";
    public Vector3 m_effectThresholds = new Vector3 (0.1f, 0.2f, 0.25f);
    public UseItemDefinition[] m_definitions;

    public UseItemDefinition GetDefinition (ItemData data) {
        foreach (UseItemDefinition def in m_definitions) {
            if (HasData (data, def)) {
                //Debug.Log ("Found valid definition directly based on itemdata");
                return def;
            } else if (def.m_gameTraits.Length > 0) { // requires actual entries to work
                if (DataHasAllTraits (data, def)) {
                    return def;
                }
                //Debug.Log ("Found valid definition based on the trait " + def.m_gameTrait);
            } else {
                //Debug.Log ("Did not find trait " + def.m_gameTrait + " in data " + data.m_id);
            }
        }
        return null;
    }

    public bool HasData (ItemData data, UseItemDefinition def) {
        foreach (ItemData checkData in def.m_requiredDatas) {
            if (checkData == data) {
                return true;
            }
        }
        return false;
    }
    public bool HasTrait (ItemGameTrait trait, UseItemDefinition def) {
        foreach (ItemGameTrait checkTrait in def.m_gameTraits) {
            if (checkTrait == trait) {
                return true;
            }
        }
        return false;
    }
    public bool DataHasAllTraits (ItemData data, UseItemDefinition def) { // checks if the given data has all the traits or not
        if (def.m_anyTrait) { // any trait is good - return as soon as one is found
            foreach (ItemGameTrait trait in data.m_gameTraits) {
                if (HasTrait (trait, def)) {
                    return true;
                }
            }
        } else { // if we -don't- find one of these traits, we immediately return null!
            foreach (ItemGameTrait trait in def.m_gameTraits) {
                if (!data.HasGameTrait (trait)) {
                    return false;
                }
            }
        }
        return true; // success!
    }

    [NaughtyAttributes.Button]
    void SetIdToName () {
        m_id = name;
    }

    [NaughtyAttributes.Button]
    void AddAllItemsToData () {
        List<ItemData> allItemDatas = new List<ItemData> { };
        List<UseItemDefinition> newUseItemDefs = new List<UseItemDefinition> { };
        Object[] loadedDatas = Resources.LoadAll ("Data/Items", typeof (ItemData));
        foreach (Object obj in loadedDatas) {
            allItemDatas.Add (obj as ItemData);
        }
        for (int i = 0; i < allItemDatas.Count; i++) {
            UseItemDefinition newDef = new UseItemDefinition ();
            newDef.m_requiredDatas = new ItemData[1];
            newDef.m_requiredDatas[0] = allItemDatas[i];
            newDef.m_description = "INVALID"; // default
            newUseItemDefs.Add (newDef);
        }
        m_definitions = newUseItemDefs.ToArray ();
    }

}