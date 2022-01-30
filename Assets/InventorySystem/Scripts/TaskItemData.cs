using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Data", menuName = "Task Item Data", order = 1)]
public class TaskItemData : ItemData {
    public float m_defaultWeight = 1f; // for when picked for randomnesses
    public BlueprintComponent[] m_requirements;
    public InventoryData m_resultsWinInventory;
    public RandomizedInventoryItem[] m_resultsWin;
    public Vector2Int m_minMaxRandomSpawnedWin = new Vector2Int (1, 1);
    public InventoryData m_resultsLoseInventory;
    public RandomizedInventoryItem[] m_resultsLoss;
    public Vector2Int m_minMaxRandomSpawnedLose = new Vector2Int (1, 1);

    public int TotalRequiredComponentCount () {
        int returnval = 0;
        foreach (BlueprintComponent comp in m_requirements) {
            returnval += comp.amount;
        }
        return returnval;
    }
    public BlueprintComponent GetRequirementByTrait (ItemGameTrait trait) {
        foreach (BlueprintComponent comp in m_requirements) {
            if (comp.trait == trait) {
                return comp;
            }
        }
        return null; // no such component
    }
    public BlueprintComponent GetRequirementByData (ItemData data) {
        foreach (BlueprintComponent comp in m_requirements) {
            if (comp.data == data) {
                return comp;
            }
        }
        return null; // no such component
    }

    public void DebugComponentCount () {
        if (TotalRequiredComponentCount () > 0 && TotalRequiredComponentCount () != m_maxStackSize) {
            Debug.LogWarning ("<color=yellow>A Task has a different max stack size than the total component requirement! Task id: " + m_id + "</color>");
        }
    }

    [NaughtyAttributes.Button]
    void SetIDtoName () {
        m_id = name;
    }
}