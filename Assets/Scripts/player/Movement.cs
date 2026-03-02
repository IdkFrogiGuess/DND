using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpheight = 2f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private bool shouldRotateTowardsMovement = true;

    private CharacterController controls;
    private Vector3 moveInput;
    private Vector3 velocity;
    private bool isSprinting;

    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 0.75f;
    [SerializeField] private LayerMask standUpBlockingMask = ~0; // default: everything

    public CapsuleCollider Collider;

    public bool isCrouching = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controls = GetComponent<CharacterController>();
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


    public void OnMove(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        moveInput = context.ReadValue<Vector2>();
        Debug.Log($"Move input: {moveInput}");
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        if (context.performed)
        {
            isSprinting = true;
        }
        else if (context.canceled)
        {
            isSprinting = false;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (enabled == false)
            return;

        Debug.Log($"Jumping {context.performed} - Is Grounded: {controls.isGrounded}");
        if (context.performed && controls.isGrounded)
        {
            Debug.Log("Jump executed");
            velocity.y = Mathf.Sqrt(jumpheight * -2f * gravity);
        }
    }

    // Update is called once per frame
    void Update()
    {

        // Camera direction
        Vector3 forward = playerCamera.forward;
        Vector3 right = playerCamera.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * moveInput.y + right * moveInput.x;

        float usedSpeed = isSprinting ? sprintSpeed : speed;
        controls.Move(moveDirection * usedSpeed * Time.deltaTime);

        if (shouldRotateTowardsMovement && moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        // Jump and Gravity
        velocity.y += gravity * Time.deltaTime;
        controls.Move(velocity * Time.deltaTime);
    }

}

