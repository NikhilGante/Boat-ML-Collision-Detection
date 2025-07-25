//#define MANUAL_CONTROL
using UnityEngine;

public class player_script : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 3f;
    [SerializeField] float movementSpeed = 20f;
    [SerializeField] float mass = 1f;
    [SerializeField] Transform cameraTransform;

    // For random movement when not in manual control mode
    [SerializeField] float directionChangeInterval = 5f;    // How often to change direction in seconds
    [SerializeField] float lookVerticalBound = 10f;    // How far up/down to look in random movement mode

    float timeSinceChange = 0f;    // Timer for changing direction
    Vector2 currentMoveDir;
    Vector2 currentLookDelta;
    Vector2 currentLookTarget;

    CharacterController controller;
    Vector3 zVel;
    Vector2 look;

    // Constants
    Vector3 lakeCentre = new Vector3(490f, 0f, 490f);
    [SerializeField] float lakeRadius = 150f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        timeSinceChange = directionChangeInterval;    // Initialize the timer to trigger a change upon first update   
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
        if (timeSinceChange >= directionChangeInterval)
        {
            currentMoveDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

            // Fixed vertical look (target) to a random value within bounds
            currentLookTarget = new Vector2(look.x + Random.Range(-180f, 180f), Random.Range(-lookVerticalBound, lookVerticalBound));

            // Calculate the minimum change rate based on the target and time interval
            Vector2 minlookChangeRate = new Vector2(currentLookTarget.x - look.x, currentLookTarget.y - look.y);
            minlookChangeRate *= Time.deltaTime / directionChangeInterval;    
            
            // Look delta is a rand value within the range of minlookChangeRate to 2x the rate so the player moves straight for some time
            currentLookDelta = new Vector2(Random.Range(1f, 2f) * minlookChangeRate.x, Random.Range(1f, 2f) * minlookChangeRate.y);
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
#if MANUAL_CONTROL
        look.x += delta.x * mouseSensitivity;
        look.y += delta.y * mouseSensitivity;
        look.y = Mathf.Clamp(look.y, -90f, 90f);    // Clamp vertical look to prevent flipping
#else
        look.x = Mathf.MoveTowards(look.x, currentLookTarget.x, Mathf.Abs(currentLookDelta.x));
        look.y = Mathf.MoveTowards(look.y, currentLookTarget.y, Mathf.Abs(currentLookDelta.y));
        //Debug.Log($"Look X: {look.x}, Y: {look.y}, target: {currentLookTarget}, time: {timeSinceChange}");    // Log the vertical look value for debugging
#endif

        cameraTransform.localRotation = Quaternion.Euler(-look.y, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, look.x, 0f);
    }
    void updateMovement(Vector2 moveInput)
    {
        var input = transform.right * moveInput.x + transform.forward * moveInput.y;
        input.Normalize();

        float distFromCentre = Vector3.Distance(transform.position, lakeCentre);
        if(distFromCentre > lakeRadius) // If outside the lake radius, move towards the lake centre
        {
            Debug.Log($"Hit radius! time: {timeSinceChange}");    // Log the vertical look value for debugging

            Vector3 directionToLakeCentre = (lakeCentre - transform.position).normalized;
            //input = directionToLakeCentre * moveInput.magnitude; // Adjust input to move towards the lake centre
            // Smoothly adjust the input towards the lake centre
            input = Vector3.Lerp(input, directionToLakeCentre * moveInput.magnitude, 0.5f);

        }
        controller.Move((input * movementSpeed + zVel) * Time.deltaTime);
    }
}
