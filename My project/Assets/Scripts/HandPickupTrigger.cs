using UnityEngine;

public class HandPickupTrigger : MonoBehaviour
{
    public PickandThrow pickAndThrow;

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"Hand collided with {other.name}");
        if (other.CompareTag("Slipper"))
        {

            if (pickAndThrow != null)
            {
                pickAndThrow.TryPickup(other);
            }
            else
            {
                Debug.LogWarning("PickAndThrow reference not set on hand!");
            }
        }
    }
}

