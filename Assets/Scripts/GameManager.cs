using System.Collections;
using System.Collections.Generic;
using Doozy.Engine.UI;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameStateEvent : UnityEvent<GameState> { }

public enum GameStates {
    NONE = 0000,
    INIT = 1000,
    LATE_INIT = 1100,
    GAME = 2000,
    NARRATIVE = 3000,
    INVENTORY = 4000,
    DEFEAT = 5000,
    WIN = 6000,
    PAUSE = 7000,

}

[System.Serializable]
public class GameState {
    public GameStates state;
    public GameStates nextState;
    public GameStateEvent evtStart;

}

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public bool initOnStart = true;
    [NaughtyAttributes.ReorderableList]
    public GameState[] gameStates;
    [SerializeField]
    private GameState currentState;
    public float lateInitWait = 0.1f;

    public GameObject masterInventory;
    private Dictionary<GameStates, GameState> gameStateDict = new Dictionary<GameStates, GameState> { };
    private BasicAgent player;
    private InventoryController m_playerInventory;
    private InventoryController m_playerClueInventory;

    public TypeWriterQueue m_thoughtWriter;
    public InkStringtableManager m_inkStringtableManager;

    void Awake () {
        if (instance == null) {
            instance = this;
        } else {
            Destroy (gameObject);
        }
        foreach (GameState states in gameStates) {
            gameStateDict.Add (states.state, states);
        }
    }
    void Start () {
        if (initOnStart) {
            Invoke ("Init", 1f); // uncomment if not going via mainmenu
        };
        //AudioManager.instance.PlayMusic ("MusicBG");
    }

    [NaughtyAttributes.Button]
    public void Init () {
        SetState (GameStates.INIT);
        //Invoke ("FixTerribleBug", 5f);
        //NextState ();
    }

    public void FixTerribleBug () {
        Invoke (nameof (StartStoryLate), 0.2f);
    }
    void StartStoryLate () {
        InkWriter.main.StartStory ();
    }

    void Late_Init () {
        currentState = gameStateDict[GameStates.LATE_INIT];
        Debug.Log ("Invoking late init");
        currentState.evtStart.Invoke (currentState);
        if (currentState.nextState != GameStates.NONE) {
            NextState ();
        }
    }
    public void NextState () {
        if (currentState.nextState != GameStates.NONE) {
            if (gameStateDict[currentState.state].nextState == GameStates.LATE_INIT) { // late init inits a bit late and only works thru nextstate
                Invoke ("Late_Init", lateInitWait);
                // Debug.Log ("Invoking late init");
                return;
            } else {
                Debug.Log ("Invoking Next State " + "(" + gameStateDict[currentState.state].nextState.ToString () + ")");
                SetState (gameStateDict[currentState.state].nextState);
            };
        }
    }
    public void SetState (GameStates state) {
        if (state != GameStates.NONE) {
            GameState = state;
        };
    }
    public GameState GetState (GameStates state) {
        foreach (GameState getState in gameStates) {
            if (getState.state == state) {
                return getState;
            }
        }
        return null;
    }
    public GameStates GameState {
        get {
            if (currentState != null) {
                return currentState.state;
            } else {
                return GameStates.NONE;
            }
        }
        set {
            Debug.Log ("Changing state to " + value);
            currentState = gameStateDict[value];
            currentState.evtStart.Invoke (currentState);
            if (currentState.nextState != GameStates.NONE) {
                NextState ();
            };
        }
    }

    public void WinGame () {
        GameState = GameStates.WIN;
        Debug.Log ("Victory!!");
        SceneManager.LoadScene ("endscene");
    }
    public void Defeat () {

        currentState = gameStateDict[GameStates.DEFEAT];
        currentState.evtStart.Invoke (currentState);
    }

    public void Restart () {
        Time.timeScale = 1f;
        SceneManager.LoadScene (SceneManager.GetActiveScene ().name, LoadSceneMode.Single);
    }

    public void DualLoadScenes () {
        SceneManager.LoadScene ("ManagersScene", LoadSceneMode.Additive);
        SceneManager.LoadScene ("SA_Demo", LoadSceneMode.Additive);
    }

    [NaughtyAttributes.Button]
    public void BackToMenu () {
        Time.timeScale = 1f;
        SceneManager.LoadScene ("mainmenu");
    }

    [NaughtyAttributes.Button]
    public void SaveGame () {
        Debug.Log ("Saving game");
        SaveManager.instance.IsNewGame = false;
        InkWriter.main.SaveStory ();
        foreach (InventoryController ctrl in InventoryController.allInventories) {
            ctrl.SaveInventory ();
        }
        AudioManager.instance.SaveVolume ();
        SceneController.instance.SaveScene ();
        SaveManager.instance.SaveCache ();
    }

    [NaughtyAttributes.Button]
    public void LoadGame () {
        Debug.Log ("Loading game");
        Restart ();
    }
    public void Pause () {
        GameState oldState = currentState;
        GameState pauseState = gameStateDict[GameStates.PAUSE];
        GameState = GameStates.PAUSE;
        pauseState.evtStart.Invoke (gameStateDict[GameStates.PAUSE]);
        StartCoroutine (PauseWaiter (oldState.state));
        Time.timeScale = 0f;
    }
    public void UnPause () {
        GameState = GameStates.NONE;
        Time.timeScale = 1f;
    }
    IEnumerator PauseWaiter (GameStates continueState) {
        yield return new WaitUntil (() => GameState != GameStates.PAUSE);
        GameState = continueState;
    }

    public void InitUseItemListeners () { // Inits a listener that listens whenever an item is successfully used
        Inventory.Item_UseItemSpot[] allUseItemSpots = FindObjectsOfType<Item_UseItemSpot> ();
        foreach (Item_UseItemSpot item in allUseItemSpots) {
            item.m_usedItemEvent.AddListener (UsedItem);
        }
    }

    void UsedItem (ItemData item, int amount) {
        // Sets a variable
        if (item != null) {
            Debug.Log ("Used item " + item.m_id + ", setting lastUsedItem to correct string & closing inventory");
            InkWriter.main.story.variablesState["lastUsedItem"] = item.m_id;
            CloseOwnInventory ();
        }
    }

    public void InitInventoryEvents () {
        Debug.Log ("Attempting to init inventory events");
        PlayerInventory = InventoryController.GetInventoryOfType (InventoryType.PLAYER, null, false);
        foreach (InventoryController lootableInventory in InventoryController.GetAllInventories (InventoryType.NONE, null, false)) {
            Debug.Log ("Adding events to " + InventoryController.GetAllInventories (InventoryType.NONE, null, false).Count + " inventories");
            Debug.Log ("Next inventory is: " + lootableInventory.name);
            lootableInventory.InitInventory (lootableInventory.data, lootableInventory.clearOnStart);
            lootableInventory.inventoryOpenedEvent.AddListener (OpenInventory);
            lootableInventory.inventoryClosedEvent.AddListener (CloseInventory);
        }
        // And then load if necessary
        if (!SaveManager.instance.IsNewGame) {
            foreach (InventoryController lootableInventory in InventoryController.GetAllInventories (InventoryType.NONE, null, false)) {
                lootableInventory.LoadInventory ();
            }
        }

    }
    public void InitDoozy () {
        Debug.Log ("Starting Doozy");
        Doozy.Engine.GameEventMessage.SendEvent ("InitDoozy");
    }
    void OpenInventory (InventoryController otherInventory) {
        SetState (GameStates.INVENTORY);
        Debug.Log ("Inventory opened " + otherInventory.gameObject);
        /*if (otherInventory.type == InventoryType.LOOTABLE || otherInventory.type == InventoryType.CRAFTING) { // auto-open player inventory when opening lootable container
            OpenOwnInventory ();
        }*/
        if (otherInventory.type == InventoryType.CRAFTING) {
            InventoryController.GetInventoryOfType (InventoryType.CRAFTING_RESULTS, null, false).Visible = true;
        }
    }
    void CloseInventory (InventoryController otherInventory) {

        /*//  Debug.Log ("Inventory closed " + otherInventory.gameObject);
        if (otherInventory.type == InventoryType.LOOTABLE) {
            PlayerInventory.Visible = false;
        }
        if (otherInventory.type == InventoryType.PLAYER) {
            InventoryController.CloseAllInventories (InventoryType.LOOTABLE);
            InventoryController.CloseAllInventories (InventoryType.CRAFTING);
            InventoryController.CloseAllInventories (InventoryType.PLAYER_CLUES);
            InventoryController.CloseAllInventories (InventoryType.CRAFTING_RESULTS);
        }
        */
        if (InventoryController.GetAllInventories ().Count == 0 && GameState == GameStates.INVENTORY) {
            SetState (GameStates.GAME);
        }
    }

    public void OpenJournal () {
        if (GameState == GameStates.GAME) {
            InkWriter.main.GoToKnot ("OpenJournalExt");
        }
    }
    public void OpenOwnInventory () {
        PlayerInventory.Visible = true;
        InventoryController.GetInventoryOfType (InventoryType.CRAFTING, null, false).Visible = true;
        InventoryController.GetInventoryOfType (InventoryType.CRAFTING_RESULTS, null, false).Visible = true;
        //PlayerClueInventory.Visible = true;
        masterInventory.SetActive (true);
        AudioManager.instance.PlaySFX ("UI_inventoryOpen");
    }
    public void CloseOwnInventory () {
        PlayerInventory.Visible = false;
        PlayerClueInventory.Visible = false;
        InventoryController.CloseAllInventories (InventoryType.LOOTABLE);
        InventoryController.CloseAllInventories (InventoryType.CRAFTING);
        InventoryController.CloseAllInventories (InventoryType.CRAFTING_RESULTS);
        masterInventory.SetActive (false);
        AudioManager.instance.PlaySFX ("UI_inventoryOpen");
    }

    public void TogglePlayerInventory () {
        if (PlayerInventory.Visible || PlayerClueInventory.Visible) {
            CloseOwnInventory ();
        } else {
            OpenOwnInventory ();
        }
    }
    public void OpenCraftingInventory () {
        if (GameState == GameStates.INVENTORY) {
            InventoryController.GetInventoryOfType (InventoryType.CRAFTING, null, false).Visible = true;
            InventoryController.GetInventoryOfType (InventoryType.CRAFTING_RESULTS, null, false).Visible = true;
        } else if (GameState != GameStates.GAME) {
            return;
        } else {
            InventoryController.GetInventoryOfType (InventoryType.CRAFTING, null, false).Visible = true;
            InventoryController.GetInventoryOfType (InventoryType.CRAFTING_RESULTS, null, false).Visible = true;
            //AudioManager.instance.PlaySFX ("ClickButton");
        }
    }

    public List<int> m_usedAddItemCountsThisSession = new List<int> { };

    public void Ink_CheckHasItem (object[] inputVariables) {
        // variable 0 -> m_id of item looked for
        // variable 1 -> ink variable name to set value to (-1 does not have, 0+ has with count)

        string inkVariableName = (string) inputVariables[1];
        string m_id = (string) inputVariables[0];
        ItemData data = InventoryController.GetDataByID (m_id);
        if (data == null) {
            Debug.LogWarning ("No such item with ID" + m_id);
            InkWriter.main.story.variablesState[(inkVariableName)] = -1;
            return;
        }
        InventoryController targetInventory = GetTargetInventory (data);
        int returnVariable = targetInventory.CountItem (data);
        InkWriter.main.story.variablesState[(inkVariableName)] = returnVariable;
    }
    public void Ink_ConsumeItem (object[] inputVariables) {
        //variable 0 -> m_id of item looked for
        //variable 1 -> integer of number of items to consume
        // note -> it's up to the inkist to check that there is enough before committing, there is no confirmation!
        int amount = (int) inputVariables[1];
        string m_id = (string) inputVariables[0];
        int currentAddItemPoint = (int) inputVariables[2];
        if (!m_usedAddItemCountsThisSession.Contains (currentAddItemPoint)) {
            m_usedAddItemCountsThisSession.Add (currentAddItemPoint);
            ItemData data = InventoryController.GetDataByID (m_id);
            if (data == null) {
                Debug.LogWarning ("No such item with ID" + m_id);
                return;
            }
            InventoryController targetInventory = GetTargetInventory (data);
            int returnVariable = targetInventory.CountItem (data);
            if (targetInventory.DestroyItemAmount (data, amount) < amount) {
                Debug.LogWarning ("Failed to destroy the required amount of item " + m_id + "(" + m_id + ")");
            }
        } else {
            Debug.LogWarning ("Tried to remove item " + m_id + " more than once, but it was stopped!");
        }
    }
    public void Ink_AddItem (object[] inputVariables) {
        //variable 0 -> m_id of item looked for
        //variable 1 -> integer of number of items to add
        // note -> it's up to the inkist to check that there is enough before committing, there is no confirmation!
        int amount = (int) inputVariables[1];
        string m_id = (string) inputVariables[0];
        int currentAddItemPoint = (int) inputVariables[2];
        if (!m_usedAddItemCountsThisSession.Contains (currentAddItemPoint)) {
            m_usedAddItemCountsThisSession.Add (currentAddItemPoint);
            ItemData data = InventoryController.GetDataByID (m_id);
            if (data == null) {
                Debug.LogWarning ("No such item with ID" + m_id);
                return;
            }
            InventoryController targetInventory = GetTargetInventory (data);
            if (!targetInventory.AddItem (data, amount)) {
                Debug.LogWarning ("Failed to add the required amount of item " + m_id + "(" + m_id + ")");
            } else {
                NewItemPopup (data, amount);
            }
        } else {
            Debug.LogWarning ("Tried to add item " + m_id + " more than once, but it was stopped!");
        }
    }

    public void Ink_ThoughtBubble (object[] inputVariables) {
        // can take up to 3 strings
        string string1 = (string) inputVariables[0];
        string string2 = (string) inputVariables[1];
        string string3 = (string) inputVariables[2];
        List<string> allStrings = new List<string> { string1, string2, string3 };
        allStrings.RemoveAll ((x) => x == "");
        int currentAddItemPoint = (int) inputVariables[3];
        if (!m_usedAddItemCountsThisSession.Contains (currentAddItemPoint)) {
            m_usedAddItemCountsThisSession.Add (currentAddItemPoint);
            PlayWriterQueue (allStrings.ToArray ());
        } else {
            Debug.LogWarning ("Tried to play a thoughtbubble more than once, but it was stopped!");
        }
    }

    public void AddItem (ItemData item) {
        Doozy.Engine.GameEventMessage.SendEvent ("ShowPlayerInventory");
        InventoryController targetInventory = GetTargetInventory (item);
        if (targetInventory.AddItem (item, 1)) {
            NewItemPopup (item, 1);
        }
    }

    public BasicAgent Player {
        get {
            if (player == null) {
                player = GameObject.FindGameObjectWithTag ("Player").GetComponent<BasicAgent> ();
                //mover.targetAgent = player.navMeshAgent;
            };
            return player;
        }
    }
    public InventoryController PlayerInventory {
        get {
            if (m_playerInventory == null) {
                m_playerInventory = InventoryController.GetInventoryOfType (InventoryType.PLAYER, null, false);
            }
            return m_playerInventory;
        }
        set {
            m_playerInventory = value;
        }
    }

    public InventoryController PlayerClueInventory {
        get {
            if (m_playerClueInventory == null) {
                m_playerClueInventory = InventoryController.GetInventoryOfType (InventoryType.PLAYER_CLUES, null, false);
            }
            return m_playerClueInventory;
        }
        set {
            m_playerClueInventory = value;
        }
    }

    public InventoryController GetTargetInventory (ItemData data) {
        InventoryController targetInventory = PlayerInventory;
        if (data.HasGameTrait (ItemGameTrait.CLUE)) {
            targetInventory = PlayerClueInventory;
        }
        return targetInventory;
    }
    public InventoryController GetTargetInventory (ItemGameTrait trait) {
        InventoryController targetInventory = PlayerInventory;
        if (trait == ItemGameTrait.CLUE) {
            targetInventory = PlayerClueInventory;
        }
        return targetInventory;
    }

    public void PlayVoiceOver (object[] inputVariables) {
        string id = inputVariables[0] as string;
        int currentAddItemPoint = (int) inputVariables[1];
        if (!m_usedAddItemCountsThisSession.Contains (currentAddItemPoint)) {
            m_usedAddItemCountsThisSession.Add (currentAddItemPoint);
            AudioManager.instance.PlaySFX (id);
        } else {
            Debug.LogWarning ("Tried to play audioclip " + id + " twice, but was stopped!");
        }
    }

    public void PlayWriterQueueFromKnot (string targetKnot) {
        // First we create a list of strings from the knot
        string[] knotStrings = m_inkStringtableManager.CreateStringArray (targetKnot);
        // Then we set it to play on the typewriter
        if (knotStrings.Length > 0) {
            PlayWriterQueue (knotStrings);
        } else {
            Debug.LogWarning ("Could not play writer queue from knot - no strings found! (" + targetKnot + ")");
        }
    }
    public void PlayWriterQueue (string[] targetStrings) {
        WriterAction[] newQueue = TypeWriterQueue.CreateTypeWriterQueue (targetStrings);
        m_thoughtWriter.SetQueue (newQueue);
        m_thoughtWriter.StartQueue (0);
        Debug.Log ("Starting new writer queue of length " + targetStrings.Length + " with contents starting with " + targetStrings[0]);
    }

    public static UIPopup NewItemPopup (ItemData data, int amount) {
        UIPopup popup;
        if (data.HasGameTrait (ItemGameTrait.CLUE)) {
            popup = UIPopup.GetPopup ("newClueAdded");
        } else {
            popup = UIPopup.GetPopup ("newItemAdded");
        };
        if (popup != null) {
            TextMeshProUGUI mainText = popup.transform.Find ("Container/ItemTextMain")?.GetComponent<TextMeshProUGUI> ();
            if (mainText != null) { // set the data name
                if (data.HasGameTrait (ItemGameTrait.CLUE)) {
                    mainText.text = "New Clue Found";
                } else if (data.HasGameTrait (ItemGameTrait.ITEM)) {
                    mainText.text = "New Item Added";
                }
            }
            Inventory.Item_DragAndDrop parent = popup.GetComponentInChildren<Inventory.Item_DragAndDrop> (true);
            Inventory.UI_ItemBox modelBox = parent.targetBox;
            modelBox.SetDraggable (false);
            Inventory.UI_ItemBox itemBox = Instantiate (data.m_prefab, parent.transform).GetComponent<Inventory.UI_ItemBox> ();
            itemBox.SetItemBoxData (data);
            itemBox.StackSize = amount;
            popup.Data.SetLabelsTexts (data.m_displayName);
            itemBox.highlight = false;
            // TURN OFF TOOL
            itemBox.tooltip.IsActive = false;
            itemBox.SetDraggable (false);
            parent.UpdateInteractability ();
            // Fix fix fix!
            itemBox.tooltip.spawnedTooltip = modelBox.tooltip.spawnedTooltip;
            //itemBox.nameText.transform.position = modelBox.nameText.transform.position;
            //itemBox.nameText.transform.localScale = modelBox.nameText.transform.localScale;
            itemBox.nameText.fontSize = modelBox.nameText.fontSize;
            modelBox.gameObject.SetActive (false);
            //worp!
            UIPopupManager.ShowPopup (popup, popup.AddToPopupQueue, false);
            return popup;
        } else {
            return null;
        }
    }
}