using UnityEngine;

/// <summary> Represents a physical item in the world — handles dragging, dropping, and inventory interaction. </summary>
public class Item : MonoBehaviour
{
    static readonly int DropHash = Animator.StringToHash("Drop");

    GlobalReferences global;
    [field: SerializeField] public ScriptableItem ScriptableItem { get; private set; }
    [SerializeField] AudioSource audioSource;
    [SerializeField] Rigidbody rb;
    [SerializeField] Collider collid;
    [SerializeField] bool isInsideInventory, isInsideHand; // Disables gravity when the item is inside the inventory or in hand
    float cooldown; // Prevents dropping spam due to animation timing
    float dragDistance; // Tracks distance between item and camera during drag
    Vector3 lastMousePosition;

    void Awake() => global = GlobalReferences.Instance;

    void Update()
    {
        if (cooldown > 0) cooldown -= Time.deltaTime;
        if (!isInsideInventory && !isInsideHand) rb.AddForce(Vector3.down * ScriptableItem.weight, ForceMode.Acceleration);
        lastMousePosition = Input.mousePosition;
    }

    // Plays a sound when the item hits something with sufficient force
    void OnCollisionEnter(Collision collision) { if (collision.relativeVelocity.magnitude > GameConstants.ItemCollisionSoundThreshold) audioSource.PlayOneShot(ScriptableItem.sound); }

    // Adds the item to the chest when the held item is moved into it
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chest") && isInsideHand)
        {
            if (!isInsideHand) return; // CancelDrag was called, item was rejected

            if (GlobalReferences.Instance.inventory.UsedSlots >= GameConstants.TotalInventorySlots)
            {
                Utils.SendLogMessage("Your inventory is already full".Colored("red"));
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
        if (isInsideInventory) return;

        rb.isKinematic = true; // Stop item's move or rotation while we are dragging or rotating it

        // Right click while dragging = rotate mode
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            transform.Rotate(global.cam.transform.up, -delta.x * GameConstants.ItemRotateSensitivity, Space.World);
            transform.Rotate(global.cam.transform.right, delta.y * GameConstants.ItemRotateSensitivity, Space.World);
            return;
        }

        // Otherwise, move the item normally
        if (!isInsideHand) dragDistance = Vector3.Distance(transform.position, global.cam.transform.position);
        if (dragDistance > GameConstants.ItemMaxPickupDistance) return;

        isInsideHand = true;
        dragDistance = Mathf.Clamp(dragDistance + Input.GetAxis("Mouse ScrollWheel") * GameConstants.ItemScrollSensitivity, GameConstants.ItemDistanceMin, GameConstants.ItemDistanceMax);
        Vector3 point = global.cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragDistance));
        point.y = Mathf.Max(point.y, GameConstants.ItemMinGroundY); // Prevent moving the item below the ground
        rb.MovePosition(point);
    }

    // Stop dragging and re-enable gravity for the item on mouse release
    void OnMouseUp()
    {
        if (isInsideInventory) return;
        isInsideHand = false;
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
        if (cooldown > 0) return; // Prevents spamming
        cooldown = GameConstants.ItemDropCooldown;

        if (isInsideInventory)
        {
            GetComponentInParent<Animator>().Play(DropHash);
            global.inventory.PlaySound(3);
            this.Wait(GameConstants.ItemDropDelay, () =>
            {
                global.inventory.RemoveItem(this);
                ResetState();
            });
        }
    }
}