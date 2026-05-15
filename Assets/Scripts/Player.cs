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
    [SerializeField] GameObject cursorDot, cursorHand;

    float rotationX, rotationY;
    Vector3 flatForward, flatRight, moveDirection;
    public Item heldItem { get; private set; }

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
        TakeItem();
        TrackHeldItem();
        ThrowHeldItem();
        HideOrShowInventoryUI();
    }

    void CameraFollowMouse()
    {
        if (global.inventory.IsChestOpen) return;

        rotationX -= Input.GetAxis("Mouse Y") * MouseSensitivity;
        rotationY += Input.GetAxis("Mouse X") * MouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -VerticalClamp, VerticalClamp);

        cam.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation = Quaternion.Euler(0, rotationY, 0);
    }

    void Movement()
    {
        animator.SetBool(IsMoving, rb.linearVelocity.magnitude > MovementThreshold);
        flatForward = new Vector3(cam.forward.x, 0, cam.forward.z).normalized;
        flatRight = new Vector3(cam.right.x, 0, cam.right.z).normalized;
        moveDirection = flatRight * Input.GetAxis("Horizontal") + flatForward * Input.GetAxis("Vertical");
        rb.linearVelocity = global.inventory.IsChestOpen ? Vector3.zero : new Vector3(moveDirection.x, 0, moveDirection.z).normalized * MoveSpeed;
    }

    void TakeItem()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem)
            {
                heldItem.Take();
                global.inventory.UpdateUI();
                SetCursor(0);
            }
            else
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Item") && (hit.collider.transform.position - transform.position).magnitude <= ItemMaxPickupDistance)
                {
                    hit.collider.GetComponent<Item>().Take();
                    global.inventory.UpdateUI();
                    SetCursor(0);
                }
            }
        }
    }

    // Detects when player starts or stops holding an item via raycast + mouse state
    void TrackHeldItem()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Item"))
            {
                Cursor.lockState = CursorLockMode.None;
                heldItem = hit.collider.GetComponent<Item>();
            }
        }

        if (Input.GetMouseButtonUp(0) && heldItem)
        {
            Cursor.lockState = CursorLockMode.Locked;
            heldItem.Drop();
            heldItem = null;
        }
    }

    void ThrowHeldItem()
    {
        if (!Input.GetKeyDown(KeyCode.G) || !heldItem || global.inventory.IsChestOpen) return;
        heldItem.Throw(cam.forward * ItemThrowStrength);
    }

    void HideOrShowInventoryUI()
    {
        if (Input.GetKeyDown(KeyCode.U)) global.inventory.ShowUI(true);
        if (Input.GetKeyUp(KeyCode.U)) global.inventory.ShowUI(false);
    }

    public void SetCursor(int id) // 0 = dot, 1 = hand
    {
        if (heldItem) return;
        if (id > 1 || id < 0) throw new System.Exception("Id can be either 0 or 1");
        cursorDot.SetActive(id == 0);
        cursorHand.SetActive(id == 1);
    }

    public void Footsteps() => audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
}