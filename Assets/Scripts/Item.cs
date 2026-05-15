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
    [SerializeField] bool isInsideInventory, isInsideHand;

    float dragDistance;
    Vector3 lastMousePosition;

    void Awake() => global = GlobalReferences.Instance;

    void Update()
    {
        Gravity();
        lastMousePosition = Input.mousePosition;
    }

    void Gravity() { if (!isInsideInventory && !isInsideHand) rb.AddForce(Vector3.down * ScriptableItem.weight, ForceMode.Acceleration); }

    // Crashing sound
    void OnCollisionEnter(Collision collision) { if (collision.relativeVelocity.magnitude > ItemCollisionSoundThreshold) audioSource.PlayOneShot(ScriptableItem.sound); }

    public void Take()
    {
        if (GlobalReferences.Instance.inventory.UsedSlots >= TotalInventorySlots)
        {
            Utils.SendLogMessage("Your inventory is already full".Colored("red"));
            ResetState();
            return;
        }

        global.inventory.AddItem(this);
        isInsideHand = false;
        isInsideInventory = true;
        rb.isKinematic = true;
        collid.isTrigger = true;
    }

    void OnMouseDrag()
    {
        if (isInsideInventory) return;

        rb.isKinematic = true;

        // Rotation with right mouse button
        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            rb.MoveRotation(Quaternion.AngleAxis(-delta.x * ItemRotateSensitivity, Vector3.up) *
                            Quaternion.AngleAxis(delta.y * ItemRotateSensitivity, Vector3.right) *
                            rb.rotation);
        }

        // Initiate drag on first frame if not already held
        if (!isInsideHand)
        {
            float distToItem = Vector3.Distance(transform.position, global.player.cam.position);
            if (distToItem > ItemMaxPickupDistance) return;
            dragDistance = Mathf.Clamp(distToItem, ItemDistanceMin, ItemDistanceMax);
            isInsideHand = true;
        }

        // Adjust distance with scroll wheel
        dragDistance = Mathf.Clamp(dragDistance + Input.GetAxis("Mouse ScrollWheel") * ItemScrollSensitivity, ItemDistanceMin, ItemDistanceMax);

        // Lock item to center of camera view
        Vector3 targetPosition = global.player.cam.position + global.player.cam.forward * dragDistance;
        targetPosition.y = Mathf.Max(targetPosition.y, ItemMinGroundY);
        rb.MovePosition(targetPosition);
    }

    void OnMouseEnter() => global.player.SetCursor(1);
    void OnMouseExit() => global.player.SetCursor(0);

    void OnMouseUp()
    {
        if (isInsideInventory) return;
        isInsideHand = false;
        rb.isKinematic = false;
    }

    public void ResetState()
    {
        isInsideHand = false;
        isInsideInventory = false;
        rb.isKinematic = false;
        collid.isTrigger = false;
        dragDistance = 0f;
    }

    // Called by player to drop the item from inventory
    public void Drop()
    {
        if (!isInsideInventory) return;
        global.inventory.PlaySound(3);
        global.inventory.RemoveItem(this);
        ResetState();
    }

    // Called by player to throw the item from hand
    public void Throw(Vector3 pos)
    {
        if (!isInsideHand || !Input.GetKeyDown(KeyCode.G) || global.inventory.IsChestOpen) return;
        ResetState();
        rb.AddForce(pos, ForceMode.Impulse);
    }
}