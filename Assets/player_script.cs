//#define MANUAL_CONTROL
using UnityEngine;

public class player_script : MonoBehaviour
{
    [SerializeField] float mouseSensitivity = 3f;
    [SerializeField] float movementSpeed = 20f;
    [SerializeField] float mass = 1f;
    [SerializeField] Transform cameraTransform;

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
        updateLook();
        updateMovement();
        updateGravity();
        // Move the player forward and backward
        //float move = Input.GetAxis("Vertical") * Time.deltaTime * 5f;
        //transform.Translate(0, 0, move);

        //// Strafe left and right
        //float strafe = Input.GetAxis("Horizontal") * Time.deltaTime * 5f;
        //transform.Translate(strafe, 0, 0);

        // Jumping
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
        //}
    }

    void updateGravity()
    {
        //if (controller.isGrounded)
        //{
        //    velocity.y = 0f; // Reset vertical velocity when grounded
        //}
        //else
        //{
        //    velocity.y -= 9.81f * mass * Time.deltaTime; // Apply gravity
        //}
        //controller.Move(velocity * Time.deltaTime); // Apply the calculated velocity
        var gravity = Physics.gravity * mass * Time.deltaTime; // Get the gravity vector scaled by mass
        zVel.y = controller.isGrounded ? -1f : zVel.y + gravity.y; // Apply gravity
    }
    void updateMovement()
    {
#if MANUAL_CONTROL
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");

#else
        var x = Random.Range(-1.0f, 1.0f);
        var y = Random.Range(-1.0f, 1.0f);

#endif

        //var input = new Vector3(x, 0, y).normalized;
        var input = new Vector3();
        input += transform.right * x; // Right/Left movement
        input += transform.forward * y; // Forward/Backward movement
        input.Normalize(); // Normalize to prevent faster diagonal movement
                           //transform.Translate(input * movementSpeed * Time.deltaTime, Space.World);

        controller.Move((input * movementSpeed + zVel) * Time.deltaTime);
    }

    void updateLook()
    {
#if MANUAL_CONTROL
        look.x += Input.GetAxis("Mouse X") * mouseSensitivity;
        look.y += Input.GetAxis("Mouse Y") * mouseSensitivity;
        Mathf.Clamp(look.y, -90f, 90f); // Clamp vertical look to prevent flipping

#else
        look.x += Random.Range(-1.0f, 1.0f) * mouseSensitivity;
        look.y += Random.Range(-1.0f, 1.0f) * mouseSensitivity;
        Mathf.Clamp(look.y, -45f, 45f); // Clamp vertical look (in this case for testing purposes)

#endif
        Debug.Log(look);

        cameraTransform.localRotation = Quaternion.Euler(-look.y, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, look.x, 0f);
    }
}
