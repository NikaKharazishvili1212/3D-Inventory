using UnityEngine;
using static GameConstants;

/// <summary> Represents a physical item in the world — handles dragging, dropping, and inventory interaction. </summary>
public class Item : MonoBehaviour
{
    GlobalReferences global;
    [field: SerializeField] public ScriptableItem ScriptableItem { get; private set; }
    [field: SerializeField] public Rigidbody rb { get; private set; }
    [SerializeField] AudioSource audioSource;
    bool isInsideHand, isInsideInventory;

    void Awake() => global = GlobalReferences.Instance;

    // Applies custom gravity — skipped when item is in hand or inventory
    void FixedUpdate() { if (!isInsideHand) rb.AddForce(Vector3.down * 15 * ScriptableItem.weight, ForceMode.Acceleration); }

    // Plays impact sound when collision is strong enough
    void OnCollisionEnter(Collision collision) { if (collision.relativeVelocity.magnitude > ItemCollisionSoundThreshold) audioSource.PlayOneShot(ScriptableItem.sound); }

    // Called by player, inventory and this script to set state of the item
    public void SetState(bool isInsideHand, bool isInsideInventory, bool isKinematic)
    {
        this.isInsideHand = isInsideHand;
        this.isInsideInventory = isInsideInventory;
        rb.isKinematic = isKinematic;
    }

    // Called by player to add this item to inventory
    public void AddToInventory()
    {
        if (GlobalReferences.Instance.inventory.UsedSlots >= TotalInventorySlots)
        {
            Utils.SendLogMessage("Your inventory is already full".Colored("red"));
            return;
        }
        global.inventory.AddItem(this);
        SetState(isInsideHand: false, isInsideInventory: true, isKinematic: true);
    }

    // Called by player script to drop this item from inventory (only works while chest is open)
    public void DropFromChest()
    {
        if (!isInsideInventory) return;
        global.inventory.RemoveItem(this);
        SetState(isInsideHand: false, isInsideInventory: false, isKinematic: false);
    }
}