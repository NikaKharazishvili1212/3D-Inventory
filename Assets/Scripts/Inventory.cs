using UnityEngine;
using UnityEngine.UI;

/// <summary> Manages the 3D chest inventory — slots, items, weight, and UI state. </summary>
public class Inventory : MonoBehaviour
{
    static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

    GlobalReferences global;
    public bool IsChestOpen { get; private set; }
    public int UsedSlots { get; private set; }
    [SerializeField] Text itemCountText, weightCountText;
    [SerializeField] Transform[] slots;
    [SerializeField] Image[] icons;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip chestOpen, chestClose, itemPickup, itemDrop;
    [SerializeField] GameObject tooltip;
    float weightCount;
    bool isOnCd;

    void Awake()
    {
        global = GlobalReferences.Instance;
        UsedSlots = 0;
    }

    void Start() => GridLayout3D();
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) OpenOrCloseInventory();
        if (Input.GetKeyDown(KeyCode.G) && IsChestOpen) DropAllItems();
    }

    // Called when pressing a hotkey or clicking the 3D inventory
    public void OpenOrCloseInventory()
    {
        if (isOnCd) return;
        isOnCd = true;
        this.Wait(1f, () => isOnCd = false);
        IsChestOpen = !IsChestOpen;
        animator.SetBool(IsOpenHash, IsChestOpen);
    }

    // Plays a sound by index: 0 chestOpen, 1 chestClose, 2 itemPickup, 3 itemDrop
    public void PlaySound(int index)
    {
        AudioClip[] sounds = { chestOpen, chestClose, itemPickup, itemDrop };
        audioSource.PlayOneShot(sounds[index]);
    }

    // Positions slots in a 3D grid layout on start
    void GridLayout3D()
    {
        Vector3 spacing = new Vector3(1f, -1f, 1f); // Spacing between slots (X, Y, Z)
        int columns = GameConstants.InventoryColumns;
        for (int i = 0; i < slots.Length; i++)
        {
            int row = i / columns;
            int column = i % columns;
            slots[i].localPosition = new Vector3(column * spacing.x, row * spacing.y, 0); // Assign position to each slot
        }
    }

    // Adds item to the first available slot and updates UI
    public void AddItem(Item item)
    {
        PlaySound(2);
        UsedSlots++;
        weightCount += item.ScriptableItem.weight;
        itemCountText.text = Utils.FormatCounter(UsedSlots, GameConstants.TotalInventorySlots);
        weightCountText.text = weightCount.ToString();

        foreach (Transform slot in slots)
            if (slot.childCount == 0)
            {
                item.transform.SetParent(slot, false);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.Euler(270, 0, 0);
                break;
            }

        Utils.SendLogMessage(item.ScriptableItem.name.Colored("green") + " has been added into inventory");
        if (UsedSlots == GameConstants.TotalInventorySlots) Utils.SendLogMessage("Your inventory is full".Colored("yellow"));
    }

    // Removes item from inventory, drops it into the scene, and resorts slots
    public void RemoveItem(Item item)
    {
        UsedSlots--;
        weightCount -= item.ScriptableItem.weight;
        itemCountText.text = Utils.FormatCounter(UsedSlots, GameConstants.TotalInventorySlots);
        weightCountText.text = weightCount.ToString();

        item.transform.SetParent(global.itemSpawner.transform, false);
        item.transform.position = transform.position + Vector3.up + Vector3.forward; // Position the dropped item slightly above and forward
        SortSlots(); // Reorganize slots to fill empty spaces
        Utils.SendLogMessage(item.ScriptableItem.name.Colored("red") + " has been removed from inventory");
    }

    // Drops all items from inventory at once and updates UI
    void DropAllItems()
    {
        if (UsedSlots == 0) return;

        foreach (Transform slot in slots)
        {
            if (slot.childCount == 0) break;
            Item item = slot.GetChild(0).GetComponent<Item>();
            item.transform.SetParent(global.itemSpawner.transform, false);
            item.transform.position = transform.position + Vector3.up + Vector3.forward;
            item.ResetState();
        }

        UsedSlots = 0;
        weightCount = 0;
        itemCountText.text = Utils.FormatCounter(UsedSlots, GameConstants.TotalInventorySlots);
        weightCountText.text = weightCount.ToString();
        PlaySound(3);
        Utils.SendLogMessage("All items removed from inventory".Colored("red"));
    }

    // Shifts items forward to fill any empty slots
    void SortSlots()
    {
        for (int i = 0; i < slots.Length - 1; i++)
            if (slots[i].childCount == 0)
                for (int j = i + 1; j < slots.Length; j++)
                    if (slots[j].childCount > 0)
                    {
                        Transform child = slots[j].GetChild(0);
                        child.SetParent(slots[i], false);
                        child.localPosition = Vector3.zero;
                        break;
                    }
    }

    // Shows or hides the canvas UI with icons for currently held items
    public void ShowUI(bool trueOrFalse)
    {
        tooltip.SetActive(trueOrFalse);
        if (trueOrFalse)
        {
            foreach (Image x in icons) x.gameObject.SetActive(false);
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].childCount > 0)
                {
                    icons[i].gameObject.SetActive(true);
                    icons[i].sprite = global.spriteAtlas.GetSprite(slots[i].GetChild(0).name.Replace("(Clone)", "").Trim());
                }
                else break;
            }
        }
    }
}