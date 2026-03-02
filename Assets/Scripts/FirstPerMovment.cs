using Unity.Android.Gradle;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//tag

public class FirstPerMovment : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpheight = 2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private bool shouldRotateTowardsMovement = true;

    // Camera control settings
    [SerializeField] private float lookSensitivity = 200f;
    [SerializeField] private bool invertY = false;
    [SerializeField] private bool lockCursor = true;

    // Crouch/stand settings
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 0.75f;
    [SerializeField] private LayerMask standUpBlockingMask = ~0; // default: everything

    // Climb settings
    [SerializeField] private float climbSpeed = 3f;

    // New: which layers count as climbable and how far to check in front
    [SerializeField] private LayerMask climbableMask = 0;
    [SerializeField] private float climbCheckDistance = 1f;

    private CharacterController controls;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isSprinting;
    public CapsuleCollider Collider;

    private bool isCrouching = false;

    // Look input state
    private Vector2 lookInput;
    private float cameraPitch = 0f;

    public bool Climbable = false;

    // Tracks whether climb input is currently held (pressed)
    private bool climbInputHeld = false;

    public Text text;

    void Start()
    {
        
        Collider = GetComponent<CapsuleCollider>();
        controls = GetComponent<CharacterController>();

        if (controls == null)
        {
            Debug.LogError("CharacterController component missing on player.");
            enabled = false;
            return;
        }

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerCamera != null)
        {
            cameraPitch = playerCamera.localEulerAngles.x;
            if (cameraPitch > 180f) cameraPitch -= 360f;
        }

        // Ensure initial heights match serialized settings if possible
        if (Collider != null) Collider.height = standHeight;
        controls.height = standHeight;
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        if (context.performed) isSprinting = true;
        else if (context.canceled) isSprinting = false;
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        if (context.performed)
        {
            isCrouching = true;
            if (Collider != null) Collider.height = crouchHeight;
            controls.height = crouchHeight;
            return;
        }
        else if (context.canceled)
        {
            // Attempt to uncrouch, but first check for obstructions above the player.
            float currentHeight = controls != null ? controls.height : crouchHeight;
            float requiredSpace = standHeight - currentHeight;

            // If there's no positive required space, just uncrouch.
            if (requiredSpace <= Mathf.Epsilon)
            {
                isCrouching = false;
                if (Collider != null) Collider.height = standHeight;
                controls.height = standHeight;
                return;
            }

            Bounds bounds = controls.bounds;
            float currentTopY = bounds.max.y;

            Vector3 boxCenter = new Vector3(bounds.center.x, currentTopY + requiredSpace * 0.5f, bounds.center.z);

            float radius = (Collider != null) ? Collider.radius : Mathf.Max(bounds.extents.x, bounds.extents.z);
            Vector3 halfExtents = new Vector3(radius, requiredSpace * 0.5f, radius);

            Collider[] hits = Physics.OverlapBox(boxCenter, halfExtents, transform.rotation, standUpBlockingMask, QueryTriggerInteraction.Ignore);

            bool blocked = false;
            if (hits != null && hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit == null) continue;

                    if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;

                    if (Collider != null && hit == Collider) continue;

                    blocked = true;
                    break;
                }
            }

            if (blocked)
            {
                isCrouching = true;
            }
            else
            {
                isCrouching = false;
                if (Collider != null) Collider.height = standHeight;
                controls.height = standHeight;
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        if (controls == null) return;
        if (context.performed && controls.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpheight * -2f * gravity);
        }
    }

    public void OnClimb(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        if (!Climbable)
        {
            Climbable = false;
            if (context.canceled) climbInputHeld = false;
            return;
        }

        if (context.performed)
        {
            // Key pressed -> climb up
            climbInputHeld = true;
            Climbable = true;
        }
        else if (context.canceled)
        {
            // Key released -> descend
            climbInputHeld = false;
        }
    }

   
    public void OnLook(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        lookInput = context.ReadValue<Vector2>();
    }

    public void Update()
    {
        if (controls == null) return;

        DetectClimbableInFront();

        // Update climb UI only if text reference exists
        if (text != null)
        {
            text.text = Climbable ? "Climbable" : string.Empty;
        }

        Vector3 moveDirection = playerCamera.forward * moveInput.y + playerCamera.right * moveInput.x;
        moveDirection.y = 0f;

        float usedSpeed = isSprinting ? sprintSpeed : speed;
        Vector3 horizontalVelocity = moveDirection * usedSpeed;

        if (controls.isGrounded && velocity.y < 0f)
        {
            velocity.y = -5;
        }

        if (Climbable)
        {
            velocity.y = climbInputHeld ? climbSpeed : -climbSpeed;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        Vector3 finalMotion = (horizontalVelocity + Vector3.up * velocity.y) * Time.deltaTime;
        controls.Move(finalMotion);
    }

    private void DetectClimbableInFront()
    {
        if (controls == null) return;
        if (climbableMask == 0) { Climbable = false; return; }

       
        Vector3 direction = playerCamera != null ? playerCamera.forward : transform.forward;
        direction.y = 0f;
        if (direction.sqrMagnitude <= Mathf.Epsilon) direction = transform.forward;
        direction.Normalize();

       
        Bounds bounds = controls.bounds;
        float height = controls.height;
        float radius = (Collider != null) ? Collider.radius : Mathf.Max(bounds.extents.x, bounds.extents.z);

        
        Vector3 origin = bounds.center + direction * (radius + 0.05f);

        
        float halfHeight = Mathf.Max(0.1f, height * 0.4f);
        Vector3 halfExtents = new Vector3(radius, halfHeight, radius);

        
        RaycastHit hitInfo;
        bool hit = Physics.BoxCast(origin, halfExtents, direction, out hitInfo, transform.rotation, climbCheckDistance, climbableMask, QueryTriggerInteraction.Ignore);

        if (hit)
        {
            Climbable = true;
        }
        else
        {
            Climbable = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || controls == null) return;

        float currentHeight = controls.height;
        float requiredSpace = standHeight - currentHeight;
        if (requiredSpace > Mathf.Epsilon)
        {
            Bounds bounds = controls.bounds;
            float currentTopY = bounds.max.y;
            Vector3 boxCenter = new Vector3(bounds.center.x, currentTopY + requiredSpace * 0.5f, bounds.center.z);
            float radius = (Collider != null) ? Collider.radius : Mathf.Max(bounds.extents.x, bounds.extents.z);
            Vector3 halfExtents = new Vector3(radius, requiredSpace * 0.5f, radius);

            Gizmos.color = Color.yellow;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
        }

        
        if (controls != null)
        {
         
            if (climbableMask != 0)
            {
                Bounds bounds = controls.bounds;
                float height = controls.height;
                float radius = (Collider != null) ? Collider.radius : Mathf.Max(bounds.extents.x, bounds.extents.z);
                float halfHeight = Mathf.Max(0.1f, height * 0.4f);
                Vector3 halfExtents = new Vector3(radius, halfHeight, radius);

                Vector3 direction = playerCamera != null ? playerCamera.forward : transform.forward;
                direction.y = 0f;
                if (direction.sqrMagnitude <= Mathf.Epsilon) direction = transform.forward;
                direction.Normalize();

                Vector3 origin = bounds.center + direction * (radius + 0.05f);
                Vector3 end = origin + direction * climbCheckDistance;

                Gizmos.color = Climbable ? Color.green : Color.cyan;
                // draw start box
                Gizmos.matrix = Matrix4x4.TRS(origin, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
                // draw end box
                Gizmos.color = Color.gray;
                Gizmos.matrix = Matrix4x4.TRS(end, transform.rotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2f);
                // draw line between them
                Gizmos.color = Color.white;
                Gizmos.DrawLine(origin, end);
            }
        }
    }
}
