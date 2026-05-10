using UnityEngine;
using static GameConstants;

/// <summary> Represents a physical item in the world — handles dragging, dropping, and inventory interaction. </summary>
public class Item : MonoBehaviour
{
    GlobalReferences global;
    [field: SerializeField] public ScriptableItem ScriptableItem { get; private set; }
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider collid;
    [SerializeField] bool isInsideInventory, isInsideHand, isRejected; // Disables gravity when the item is inside the inventory or in hand
    public static bool AnyItemInHand { get; private set; }
    float dragDistance; // Tracks distance between item and camera during drag
    Vector3 lastMousePosition;

    void Awake() => global = GlobalReferences.Instance;

    void Update()
    {
        if (!isInsideInventory && !isInsideHand) rb.AddForce(Vector3.down * ScriptableItem.weight, ForceMode.Acceleration);
        lastMousePosition = Input.mousePosition;

        ThrowOnHotkey();
    }

    // Plays a sound when the item hits something with sufficient force
    void OnCollisionEnter(Collision collision) { if (collision.relativeVelocity.magnitude > ItemCollisionSoundThreshold) audioSource.PlayOneShot(ScriptableItem.sound); }

    // Adds the item to the chest when the held item is moved into it
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chest") && isInsideHand)
        {
            if (!isInsideHand) return; // CancelDrag was called, item was rejected

            if (GlobalReferences.Instance.inventory.UsedSlots >= TotalInventorySlots)
            {
                Utils.SendLogMessage("Your inventory is already full".Colored("red"));
                isRejected = true;
                ResetState();
                return;
            }

            global.inventory.AddItem(this);
            InsideInventoryState();
        }
    }

    // Drag, rotate, and disable gravity for the item
    void OnMouseDrag()
    {
        if (isInsideInventory || isRejected) return;

        rb.isKinematic = true; // Stop item's move or rotation while we are dragging or rotating it

        // Right click while dragging = rotate mode
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            transform.Rotate(global.cam.transform.up, -delta.x * ItemRotateSensitivity, Space.World);
            transform.Rotate(global.cam.transform.right, delta.y * ItemRotateSensitivity, Space.World);
            return;
        }

        // Otherwise, move the item normally
        if (!isInsideHand) dragDistance = Vector3.Distance(transform.position, global.cam.transform.position);
        if (dragDistance > ItemMaxPickupDistance) return;

        isInsideHand = true;
        AnyItemInHand = true;
        dragDistance = Mathf.Clamp(dragDistance + Input.GetAxis("Mouse ScrollWheel") * ItemScrollSensitivity, ItemDistanceMin, ItemDistanceMax);
        Vector3 point = global.cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragDistance));
        point.y = Mathf.Max(point.y, ItemMinGroundY); // Prevent moving the item below the ground
        rb.MovePosition(point);
    }

    // Stop dragging and re-enable gravity for the item on mouse release
    void OnMouseUp()
    {
        if (isInsideInventory) return;
        AnyItemInHand = false;
        isInsideHand = false;
        isRejected = false;
        rb.isKinematic = false;
    }

    // Resets physics and drag state
    public void ResetState()
    {
        isInsideHand = false;
        isInsideInventory = false;
        rb.isKinematic = false;
        collid.isTrigger = false;
    }

    void InsideInventoryState()
    {
        isInsideHand = false;
        isInsideInventory = true;
        rb.isKinematic = true;
        collid.isTrigger = true;
    }

    // Drops the item from the inventory, called by Player on mouse release
    public void Drop()
    {
        if (isInsideInventory)
        {
            global.inventory.PlaySound(3);
            global.inventory.RemoveItem(this);
            ResetState();
        }
    }

    void ThrowOnHotkey()
    {
        if (isInsideHand && Input.GetKeyDown(KeyCode.G) && !global.inventory.IsChestOpen)
        {
            isRejected = true;
            ResetState();
            rb.AddForce(global.playerCamera.forward * ItemThrowStrength, ForceMode.Impulse);
        }
    }
}