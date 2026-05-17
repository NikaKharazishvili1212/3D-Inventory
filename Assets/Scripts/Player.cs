using UnityEngine;
using static GameConstants;

public class Player : MonoBehaviour
{
    readonly int IsMoving = Animator.StringToHash("IsMoving");

    GlobalReferences global;
    [field: SerializeField] public Transform cam { get; private set; }
    [SerializeField] Rigidbody rb;
    [SerializeField] Animator animator;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] Transform cursorsParent;
    [SerializeField] GameObject[] cursors; // 0=dot, 1=hand, 2=handDrag
    int cursorState = -1; // cached to avoid redundant SetCursor calls

    public Item heldItem { get; private set; }
    float rotationX, rotationY, dragDistance;
    Vector3 flatForward, flatRight, moveDirection;


    void Awake()
    {
        global = GlobalReferences.Instance;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        CameraFollowMouse();
        Movement();
        DragItem();
        TakeItem();
        ThrowHeldItem();
        HideOrShowInventoryUI();
        UpdateCursor();
        Inventory();
        DropAllItems();
        MoveCursorInChestAndDropItemByClickingOnIt();
    }

    // Rotates camera with mouse; suppressed while rotating item or chest is open
    void CameraFollowMouse()
    {
        if (heldItem && Input.GetMouseButton(1) || global.inventory.IsChestOpen) return;

        rotationX -= Input.GetAxis("Mouse Y") * MouseSensitivity;
        rotationY += Input.GetAxis("Mouse X") * MouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -VerticalClamp, VerticalClamp);

        cam.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    // Moves player from input; stops movement while chest is open or rotating item
    void Movement()
    {
        animator.SetBool(IsMoving, rb.linearVelocity.magnitude > MovementThreshold);
        flatForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        flatRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        moveDirection = flatRight * Input.GetAxis("Horizontal") + flatForward * Input.GetAxis("Vertical");
        // Suppress player movement while chest is open or rotating item
        rb.linearVelocity = global.inventory.IsChestOpen || (heldItem && Input.GetMouseButton(1)) ? Vector3.zero : new Vector3(moveDirection.x, 0, moveDirection.z).normalized * MoveSpeed;
    }

    // Drags held item with LMB; RMB while holding rotates it instead of moving
    void DragItem()
    {
        if (global.inventory.IsChestOpen) return;

        // Drag item
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Item") && (hit.collider.transform.position - transform.position).magnitude <= ItemMaxPickupDistance)
            {
                heldItem = hit.collider.GetComponent<Item>();
                dragDistance = Mathf.Clamp(Vector3.Distance(heldItem.transform.position, cam.position), ItemDistanceMin, ItemDistanceMax);
                heldItem.SetState(isInsideHand: true, isInsideInventory: false, isKinematic: true);
            }
        }

        // Release item
        if (Input.GetMouseButtonUp(0) && heldItem)
        {
            heldItem.SetState(isInsideHand: false, isInsideInventory: false, isKinematic: false);
            heldItem = null;
        }

        // Rotate item
        if (Input.GetMouseButton(1) && heldItem)
        {
            heldItem.transform.Rotate(cam.up, -Input.GetAxis("Mouse X") * ItemRotationSensitivity, Space.World);
            heldItem.transform.Rotate(cam.right, Input.GetAxis("Mouse Y") * ItemRotationSensitivity, Space.World);
            return; // suppress position dragging while rotating
        }

        if (!heldItem) return;
        dragDistance = Mathf.Clamp(dragDistance + Input.GetAxis("Mouse ScrollWheel") * ItemScrollSensitivity, ItemDistanceMin, ItemDistanceMax);
        Vector3 targetPosition = cam.position + cam.forward * dragDistance;
        targetPosition.y = Mathf.Max(targetPosition.y, ItemMinGroundY);
        heldItem.rb.MovePosition(targetPosition);
    }

    // Takes held or aimed item into inventory on E; blocked while chest is open
    void TakeItem()
    {
        if (!Input.GetKeyDown(KeyCode.E) || global.inventory.IsChestOpen) return;

        if (heldItem)
        {
            heldItem.AddToInventory();
            global.inventory.UpdateUI();
            heldItem = null;
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Item"))
            {
                if ((hit.collider.transform.position - transform.position).magnitude > ItemMaxPickupDistance) Utils.SendLogMessage("Item is too far away".Colored("red"));
                else
                {
                    hit.collider.GetComponent<Item>().AddToInventory();
                    global.inventory.UpdateUI();
                }
            }
        }
    }

    // Throws held item forward on G; blocked while chest is open
    void ThrowHeldItem()
    {
        if (!Input.GetKeyDown(KeyCode.G) || !heldItem || global.inventory.IsChestOpen) return;
        heldItem.SetState(isInsideHand: false, isInsideInventory: false, isKinematic: false);
        heldItem.rb.AddForce(cam.forward * ItemThrowStrength, ForceMode.Impulse);
        heldItem = null;
    }

    // Shows inventory icon UI while U is held
    void HideOrShowInventoryUI()
    {
        if (Input.GetKeyDown(KeyCode.U)) global.inventory.ShowUI(true);
        if (Input.GetKeyUp(KeyCode.U)) global.inventory.ShowUI(false);
    }

    // Automatically updates cursor icon based on conditions
    void UpdateCursor()
    {
        if (global.inventory.IsChestOpen) SetCursor(0);
        else if (heldItem) SetCursor(2);
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Item") && (hit.collider.transform.position - transform.position).magnitude <= ItemMaxPickupDistance) SetCursor(1);
            else SetCursor(0);
        }
    }

    public void SetCursor(int id)
    {
        if (id == cursorState) return;
        if (id < 0 || id >= cursors.Length) throw new System.Exception($"Cursor id {id} is out of range");
        cursorState = id;
        for (int i = 0; i < cursors.Length; i++) cursors[i].SetActive(i == id);
    }

    // Toggles chest inventory on I; drops held item if picking up while chest is closed
    void Inventory()
    {
        if (!Input.GetKeyDown(KeyCode.I)) return;
        if (!global.inventory.IsChestOpen && heldItem)
        {
            heldItem.SetState(isInsideHand: false, isInsideInventory: false, isKinematic: false);
            heldItem = null;
        }
        global.inventory.OpenOrCloseInventory();
    }

    // Drops all inventory items on G (only works while chest is open)
    void DropAllItems() { if (Input.GetKeyDown(KeyCode.G)) global.inventory.DropAllItems(); }

    // Moves fake cursor to follow mouse while chest is open; clicks drop items from inventory
    void MoveCursorInChestAndDropItemByClickingOnIt()
    {
        if (!global.inventory.IsChestOpen) return;
        cursorsParent.localPosition = new Vector3((Input.mousePosition.x / Screen.width - 0.5f) * CursorMoveSpeedInChest, (Input.mousePosition.y / Screen.height - 0.5f) * CursorMoveSpeedInChest, cursorsParent.localPosition.z);

        if (!Input.GetMouseButtonDown(0)) return;
        Ray ray = new Ray(cam.position, (cursorsParent.position - cam.position).normalized);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Item")) hit.collider.GetComponent<Item>().DropFromChest();
    }

    // Resets fake cursor to center when chest closes
    public void ResetCursorPosition() => cursorsParent.localPosition = CursorLocalZ;

    // Plays footstep sound, called by walk animation event
    public void Footsteps() => audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
}