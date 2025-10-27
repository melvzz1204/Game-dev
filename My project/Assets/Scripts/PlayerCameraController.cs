using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform target;       // Follow target (e.g., player head pivot)
    public Transform playerBody;   // Player root transform

    [Header("View Settings")]
    public bool isFirstPerson = false;
    public KeyCode toggleViewKey = KeyCode.V;

    [Header("Camera Settings")]
    public float mouseSensitivity = 3f;
    public float joystickSensitivity = 120f; // sensitivity for joystick camera
    public float smoothTime = 0.05f;
    public float minPitch = -40f;
    public float maxPitch = 75f;
    public float cameraHeight = 1.8f;
    public float cameraDistance = 4f;
    public float minDistance = 2f;
    public float maxDistance = 6f;
    public float zoomSpeed = 2f;
    public float sideOffset = 0.5f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;

    [Header("Raycast Settings")]
    public LayerMask aimLayers;
    public float rayDistance = 100f;
    public Color hitColor = Color.red;
    public Color missColor = Color.white;

    [Header("Input Mode")]
    public bool useJoystickCamera = false;  // ✅ Switch between mouse or joystick camera
    private Vector2 cameraJoystickInput = Vector2.zero;

    private float yaw;
    private float pitch;
    private float desiredDistance;
    private float smoothedYaw;
    private float smoothedPitch;
    private float yawVelocity;
    private float pitchVelocity;

    private Camera cam;
    private Transform hitTarget;
    private Vector3 hitPoint;

    void Start()
    {
        cam = Camera.main;
        desiredDistance = cameraDistance;

        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

#if UNITY_EDITOR || UNITY_STANDALONE
        // Only lock cursor if we are using mouse control
        if (!useJoystickCamera)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
#else
// Always keep cursor visible on mobile/touch
Cursor.lockState = CursorLockMode.None;
Cursor.visible = true;
#endif

    }

    void Update()
    {
        HandleInput();
        HandleRaycast();
    }

    void LateUpdate()
    {
        UpdateCameraPosition();
        HandleCharacterRotation();
    }

    // 🎮 Called by UI Virtual Joystick (camera stick)
    public void OnCameraJoystickInput(Vector2 input)
    {
        cameraJoystickInput = input;
    }

    public bool TryGetAimPoint(out Vector3 point)
    {
        point = hitPoint;
        return hitTarget != null;
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(toggleViewKey))
        {
            isFirstPerson = !isFirstPerson;

            if (isFirstPerson && playerBody != null)
            {
                yaw = playerBody.eulerAngles.y;
                smoothedYaw = yaw;
            }
        }

        if (useJoystickCamera)
        {
            // 📱 Camera control via virtual joystick
            yaw += cameraJoystickInput.x * joystickSensitivity * Time.deltaTime;
            pitch -= cameraJoystickInput.y * joystickSensitivity * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            // 🖱️ Camera control via mouse
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // Scroll to zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                desiredDistance -= scroll * zoomSpeed;
                desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
            }
        }
    }

    void UpdateCameraPosition()
    {
        float effectiveSmoothTime = Mathf.Max(0.01f, smoothTime);
        smoothedYaw = Mathf.SmoothDampAngle(smoothedYaw, yaw, ref yawVelocity, effectiveSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        smoothedPitch = Mathf.SmoothDampAngle(smoothedPitch, pitch, ref pitchVelocity, effectiveSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        Quaternion rotation = Quaternion.Euler(smoothedPitch, smoothedYaw, 0f);

        if (isFirstPerson)
        {
            Vector3 headOffset = new Vector3(0, 2.4f, 0);
            Vector3 fpPos = playerBody.position + headOffset;
            transform.position = Vector3.Lerp(transform.position, fpPos, 1f - Mathf.Exp(-20f * Time.unscaledDeltaTime));
            transform.rotation = rotation;
        }
        else
        {
            Vector3 offset = rotation * new Vector3(sideOffset, cameraHeight, -desiredDistance);
            Vector3 desiredPos = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime));
            transform.rotation = rotation;
        }
    }

    void HandleCharacterRotation()
    {
        if (!playerBody) return;

        if (isFirstPerson)
        {
            Quaternion targetRot = Quaternion.Euler(0, yaw, 0);
            playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
        else
        {
            Quaternion targetRot = Quaternion.Euler(0, smoothedYaw, 0);
            playerBody.rotation = Quaternion.Slerp(playerBody.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    void HandleRaycast()
    {
        if (!cam) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, rayDistance, aimLayers);

        if (hitSomething)
        {
            hitTarget = hit.transform;
            hitPoint = hit.point;
            Debug.DrawLine(ray.origin, hit.point, hitColor);
        }
        else
        {
            hitTarget = null;
            hitPoint = ray.origin + ray.direction * rayDistance;
            Debug.DrawLine(ray.origin, hitPoint, missColor);
        }
    }
}
