using UnityEngine;
using static GameConstants;

/// <summary> Handles player movement, rotation, and mouse interactions with items and the inventory. </summary>
public class Player : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("IsMoving"); // Optimized for performance

    GlobalReferences global;
    [SerializeField] Transform cam;
    [SerializeField] Rigidbody rb;
    [SerializeField] Animator animator; // Handles animations for footstep sounds
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] float moveSpeed = 5f;
    bool isUiDisplayed;

    void Awake() => global = GlobalReferences.Instance;

    void Update()
    {
        MoveAndRotation();
        DropItemsByMouseUp();
        DisplayInventoryByMouseDown();
    }

    // Called by animation to play a random footstep sound when moving
    public void Footsteps() => audioSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);

    // Handles movement input and player rotation, blocked while inventory is open
    void MoveAndRotation()
    {
        animator.SetBool(IsMoving, rb.linearVelocity.magnitude > MovementThreshold);
        Vector3 moveDirection = cam.right * Input.GetAxis("Horizontal") + cam.forward * Input.GetAxis("Vertical");
        rb.linearVelocity = global.inventory.IsChestOpen ? Vector3.zero : new Vector3(moveDirection.x, 0, moveDirection.z).normalized * moveSpeed;
    }

    // Raycasts on mouse release to drop a held inventory item
    void DropItemsByMouseUp()
    {
        if (!Input.GetMouseButtonUp(0)) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Item")) hit.collider.GetComponent<Item>()?.Drop();
    }

    // Raycasts on mouse press to show inventory UI, hides it on release
    void DisplayInventoryByMouseDown()
    {
        if (Input.GetMouseButtonDown(0) && !isUiDisplayed)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Chest"))
            {
                isUiDisplayed = true;
                global.inventory.ShowUI(true);
            }
        }

        if (Input.GetMouseButtonUp(0) && isUiDisplayed)
        {
            isUiDisplayed = false;
            global.inventory.ShowUI(false);
        }
    }
}