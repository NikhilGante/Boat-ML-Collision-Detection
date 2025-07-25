//#define MANUAL_CONTROL
using UnityEngine;

public class player_script : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 3f;
    [SerializeField] float movementSpeed = 20f;
    [SerializeField] float mass = 1f;
    [SerializeField] Transform cameraTransform;

    // For random movement when not in manual control mode
    [SerializeField] float directionChangeInterval = 2f;    // How often to change direction in seconds
    [SerializeField] float lookVerticalBound = 15f;    // How far up/down to look in random movement mode

    float timeSinceChange = 0f;
    Vector2 currentMoveDir;
    Vector2 currentLookDelta;

    CharacterController controller;
    Vector3 zVel;
    Vector2 look;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Awake()    // Awake is called when the script instance is being loaded
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
#if MANUAL_CONTROL
        currentMoveDir = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        currentLookDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

#else
        timeSinceChange += Time.deltaTime;
        if (timeSinceChange > directionChangeInterval)
        {
            currentMoveDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            currentLookDelta = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            //currentLookDelta = new Vector2(Random.Range(-1f, 1f), Random.Range(-lookVerticalBound, lookVerticalBound));
            timeSinceChange = 0f;
        }
#endif

        updateLook(currentLookDelta);
        updateMovement(currentMoveDir);
        updateGravity();
    }

    void updateGravity()
    {
        var gravity = Physics.gravity * mass * Time.deltaTime; // Get the gravity vector scaled by mass
        zVel.y = controller.isGrounded ? -1f : zVel.y + gravity.y; // Apply gravity
    }
    void updateLook(Vector2 delta)
    {
        look.x += delta.x * mouseSensitivity;
        look.y += delta.y * mouseSensitivity;

#if MANUAL_CONTROL
        look.y = Mathf.Clamp(look.y, -90f, 90f);    // Clamp vertical look to prevent flipping
#else
        // For random movement, we set the vertical look to a fixed value
        //look.y = delta.y;    // Clamp vertical look (in this case for testing purposes)
        look.y = Mathf.Clamp(look.y, -lookVerticalBound, lookVerticalBound);    // Clamp vertical look to prevent flipping
        Debug.Log($"Look Y: {look.y}");    // Log the vertical look value for debugging
#endif

        cameraTransform.localRotation = Quaternion.Euler(-look.y, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, look.x, 0f);
    }
    void updateMovement(Vector2 moveInput)
    {
        var input = transform.right * moveInput.x + transform.forward * moveInput.y;
        input.Normalize();
        controller.Move((input * movementSpeed + zVel) * Time.deltaTime);
    }
}
