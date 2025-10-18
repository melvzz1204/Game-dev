using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Slipper
{
    public string name;
    public GameObject slipperObject;
    public float mass = 1.0f;
    public float accuracy = 0.1f;
}

public class PickandThrow : MonoBehaviour
{
    [Header("References")]
    public CharacterControllerMovement movementScript;
    public Transform startPointThrow;
    public Transform holdPoint; // child under hand
    public PlayerCameraController cameraController; // <-- added: camera reference for aim raycast

    [Header("Slipper Settings")]
    public List<Slipper> slippers = new List<Slipper>();
    public float throwForce = 20f;
    public float pickupCooldown = 0.5f;

    [Header("Input Settings")]
    public KeyCode throwKey = KeyCode.Mouse0; // Left Mouse
    public KeyCode pickKey = KeyCode.E;       // Pickup key

    private float lastThrowTime = -1f;
    private Slipper heldSlipper = null;

    void Update()
    {
        HandleInput();

    }

    void HandleInput()
    {
        if (heldSlipper != null && Input.GetKeyDown(throwKey))
        {
            StartCoroutine(ThrowSequence());
        }
        else if (Input.GetKeyDown(pickKey))
        {
            // ✅ This must be here
            if (movementScript != null)
                StartCoroutine(movementScript.PlayPickAnimation());
            else
                Debug.LogWarning("movementScript reference not set in PickandThrow!");
        }
    }

    public void TryPickup(Collider other)
    {
        if (heldSlipper != null || Time.time < lastThrowTime + pickupCooldown)
            return;

        Slipper slipper = slippers.FirstOrDefault(s => s.slipperObject == other.gameObject);
        if (slipper == null) return;

        heldSlipper = slipper;
        other.gameObject.transform.parent = holdPoint;
        Destroy(other.gameObject.GetComponent<Rigidbody>());
        other.gameObject.transform.localPosition = Vector3.zero;
        other.gameObject.transform.localRotation = Quaternion.identity;

        Debug.Log($"Picked up {slipper.name}");


    }


    System.Collections.IEnumerator ThrowSequence()
    {
        if (movementScript.animationComponent == null) yield break;

        movementScript.isThrowing = true; // ✅ block movement/idle animations

        // Play throw animation
        movementScript.animationComponent.CrossFade("Throw Object", 0.1f);

        // Wait for animation length or a fallback
        float animLength = 0.5f;
        if (movementScript.animationComponent["Throw Object"] != null)
            //animLength = movementScript.animationComponent["Throw Object"].length;

            yield return new WaitForSeconds(0.5f);

        // Actually throw the slipper
        ThrowSlipper();

        // Give a small buffer before allowing movement animations again
        yield return new WaitForSeconds(0.2f);

        movementScript.isThrowing = false;
    }


    void ThrowSlipper()
    {
        if (heldSlipper == null) return;

        GameObject slipperObject = heldSlipper.slipperObject;
        slipperObject.transform.parent = null;
        // Set slipper rotation to match character's rotation
        slipperObject.transform.position = startPointThrow.position;
        slipperObject.transform.rotation = startPointThrow.rotation;


        // Ensure a Rigidbody exists and is ready for physics-driven motion
        Rigidbody rb = slipperObject.GetComponent<Rigidbody>();
        if (rb == null) rb = slipperObject.AddComponent<Rigidbody>();
        rb.mass = heldSlipper.mass;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // Determine throw direction using the camera's aim point (use the out param regardless of hit boolean)
        Vector3 throwDirection = startPointThrow.forward;
        if (cameraController != null)
        {
            cameraController.TryGetAimPoint(out Vector3 aimPoint); // point is valid even if no hit (farthest point)
            Vector3 dir = (aimPoint - startPointThrow.position);
            if (dir.sqrMagnitude > 0.0001f) throwDirection = dir.normalized;
        }
        else if (Camera.main != null)
        {
            throwDirection = Camera.main.transform.forward;
        }

        // apply small spread based on slipper accuracy
        throwDirection = (throwDirection + Random.insideUnitSphere * heldSlipper.accuracy).normalized;

        // Impart linear velocity directly instead of using AddForce
        rb.linearVelocity = throwDirection * throwForce;
        // Add some angular velocity for a more natural throw
        rb.angularVelocity = Random.insideUnitSphere * 5f;
        // Optionally, wake up the body
        rb.WakeUp();

        Debug.Log($"Threw {heldSlipper.name}");

        heldSlipper = null;
        lastThrowTime = Time.time;
    }
}
