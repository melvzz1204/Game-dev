using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterControllerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject CurrentPlayer; // model/rig with Animation component

    private CharacterController controller;
    public Animation animationComponent;

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    private Vector3 velocity;
    private bool isGrounded;
    private bool isPickingUp = false;

    [Header("Input Settings")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public string runButton = "Fire3"; // Left Shift
    public KeyCode jumpKey = KeyCode.Space;
    public bool isThrowing = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (CurrentPlayer != null)
            animationComponent = CurrentPlayer.GetComponent<Animation>();

        if (animationComponent == null)
            Debug.LogError("No Animation component found on CurrentPlayer!");
    }

    void Update()
    {
        if (!isPickingUp)
        {
            HandleMovement();
            HandleJump();
        }
    }

    void HandleMovement()
    {
        if (animationComponent == null) return;

        if (isPickingUp || isThrowing)
            return; // ❌ skip movement animation while busy

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float moveX = Input.GetAxis(horizontalAxis);
        float moveZ = Input.GetAxis(verticalAxis);
        bool isRunning = Input.GetButton(runButton);

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Animation logic
        if (animationComponent != null && !isPickingUp)
        {
            if (moveZ > 0.1f)
            {
                // Forward movement
                animationComponent.CrossFade(isRunning ? "RunningAnimation" : "Walking");
            }
            else if (moveZ < -0.1f)
            {
                // Backward movement
                animationComponent.CrossFade("WalkingBackward");
            }
            else if (move.sqrMagnitude > 0.1f)
            {
                // Strafing left/right (optional: use Walking)
                animationComponent.CrossFade("Walking");
            }
            else
            {
                // Idle
                animationComponent.CrossFade("Idle");
            }
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

    public IEnumerator PlayPickAnimation()
    {
        if (animationComponent == null)
        {
            Debug.LogError("No Animation component found on CurrentPlayer!");
            yield break;
        }

        Debug.Log("Playing Pick Object animation...");

        isPickingUp = true;

        // Force stop other animations before playing
        animationComponent.Stop();
        animationComponent.CrossFade("Pick Object", 0.1f);

        // If the animation doesn’t exist, warn
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
