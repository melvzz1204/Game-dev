using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject CurrentPlayer;
    private CharacterController controller;
    public Animation animationComponent;
    public Transform cameraTransform; // 🎯 Reference to main camera

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float rotationSpeed = 10f;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isPickingUp = false;

    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string runButton = "Fire3";
    public KeyCode jumpKey = KeyCode.Space;
    public bool isThrowing = false;

    [Header("Joystick Input")]
    public bool useJoystick = false;
    private Vector2 joystickInput = Vector2.zero;

    // 📱 Mobile input flags
    private bool isMobileRunning = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (CurrentPlayer != null)
            animationComponent = CurrentPlayer.GetComponent<Animation>();

        if (animationComponent == null)
            Debug.LogError("No Animation component found on CurrentPlayer!");

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform; // Auto-assign main camera
    }

    void Update()
    {
        if (!isPickingUp)
        {
            HandleMovement();
            HandleJump();
        }
    }

    // 🚀 Called by joystick UI
    public void OnJoystickInput(Vector2 input)
    {
        joystickInput = input;
    }

    // 📱 Called by UI Button to toggle running (pressed = toggle)
    public void MobileRunInput()
    {
        if (!isPickingUp)
            isMobileRunning = !isMobileRunning;
    }

    void HandleMovement()
    {
        if (animationComponent == null) return;
        if (isPickingUp || isThrowing) return;

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float moveX, moveZ;

        if (useJoystick)
        {
            moveX = joystickInput.x;
            moveZ = joystickInput.y;
        }
        else
        {
            moveX = Input.GetAxis(horizontalAxis);
            moveZ = Input.GetAxis(verticalAxis);
        }

        bool isRunning = Input.GetButton(runButton) || isMobileRunning;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // 🧭 Movement direction relative to camera (optional but better for 3D games)
        Vector3 inputDir = new Vector3(moveX, 0, moveZ);
        Vector3 moveDir = inputDir;

        if (inputDir.sqrMagnitude > 0.01f)
        {
            // ✅ Convert input to camera-relative direction
            if (Camera.main != null)
            {
                Vector3 camForward = Camera.main.transform.forward;
                Vector3 camRight = Camera.main.transform.right;
                camForward.y = 0;
                camRight.y = 0;
                camForward.Normalize();
                camRight.Normalize();

                moveDir = (camForward * moveZ + camRight * moveX).normalized;
            }

            // Rotate toward direction
            HandleRotation(moveDir);
        }

        // ✅ Move forward based on facing direction
        float moveAmount = Mathf.Clamp01(inputDir.magnitude);
        controller.Move(moveDir * moveAmount * currentSpeed * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Animation
        HandleAnimation(moveX, moveZ, isRunning);
    }

    void HandleRotation(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }


    void HandleJump()
    {
        if (isGrounded && Input.GetKeyDown(jumpKey))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animationComponent != null)
                animationComponent.CrossFade("Jump");
        }
    }

    void HandleAnimation(float moveX, float moveZ, bool isRunning)
    {
        if (animationComponent == null || isPickingUp) return;

        if (Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f)
        {
            animationComponent.CrossFade(isRunning ? "RunningAnimation" : "Walking");
        }
        else
        {
            animationComponent.CrossFade("Idle");
        }
    }

    public IEnumerator PlayPickAnimation()
    {
        if (animationComponent == null)
        {
            Debug.LogError("No Animation component found on CurrentPlayer!");
            yield break;
        }

        isPickingUp = true;
        animationComponent.Stop();
        animationComponent.CrossFade("Pick Object", 0.1f);

        if (animationComponent["Pick Object"] == null)
        {
            Debug.LogError("Animation clip 'Pick Object' not found on CurrentPlayer!");
            isPickingUp = false;
            yield break;
        }

        float animLength = animationComponent["Pick Object"].length;
        yield return new WaitForSeconds(animLength);
        isPickingUp = false;
        animationComponent.CrossFade("Idle", 1.0f);
    }
}
