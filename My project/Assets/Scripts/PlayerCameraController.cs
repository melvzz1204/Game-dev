using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform target;       // The follow target (e.g. player head pivot)
    public Transform playerBody;   // The main player root

    [Header("View Settings")]
    public bool isFirstPerson = false;
    public KeyCode toggleViewKey = KeyCode.V;

    [Header("Camera Settings")]
    public float mouseSensitivity = 3f;
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
    public KeyCode rotateKey = KeyCode.Mouse0;  // Hold this to rotate player
    public float rotationSpeed = 10f;

    [Header("Raycast Settings")]
    public LayerMask aimLayers;
    public float rayDistance = 100f;
    public Color hitColor = Color.red;
    public Color missColor = Color.white;

    private float yaw;
    private float pitch;
    private float desiredDistance;
    // Smoothed rotation state (use per-axis smoothing to avoid flicker)
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

    // Public accessor for other scripts to obtain the current aim point.
    // Returns true if raycast hit a target in aimLayers, false otherwise.
    public bool TryGetAimPoint(out Vector3 point)
    {
        point = hitPoint;
        return hitTarget != null;
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(toggleViewKey))
            isFirstPerson = !isFirstPerson;

        // Orbit control
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            desiredDistance -= scroll * zoomSpeed;
            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        }
    }

    void UpdateCameraPosition()
    {
        // Improved smoothing: use SmoothDampAngle for yaw/pitch, but clamp smoothTime to avoid overshoot/flicker
        float effectiveSmoothTime = Mathf.Max(0.01f, smoothTime);
        smoothedYaw = Mathf.SmoothDampAngle(smoothedYaw, yaw, ref yawVelocity, effectiveSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        smoothedPitch = Mathf.SmoothDampAngle(smoothedPitch, pitch, ref pitchVelocity, effectiveSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        Quaternion rotation = Quaternion.Euler(smoothedPitch, smoothedYaw, 0f);

        if (isFirstPerson)
        {
            Vector3 fpPos = target.position + new Vector3(0, 0.2f, 0);
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
        // ✅ Character rotates ONLY when rotateKey is held
        if (!playerBody) return;
        if (Input.GetKey(rotateKey))
        {
            // Try rotating toward hit point if available
            if (hitTarget)
            {
                Vector3 dir = (hitPoint - playerBody.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(dir);
                    playerBody.rotation = Quaternion.Slerp(playerBody.rotation, lookRot, rotationSpeed * Time.deltaTime);
                    return;
                }
            }

            // Otherwise, rotate toward camera yaw
            Quaternion targetRot = Quaternion.Euler(0, yaw, 0);
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
