using UnityEngine;
using UnityEngine.UI;  // For standard Unity UI elements like Button
using TMPro;

public class SelectCharacter : MonoBehaviour
{        // Reference to the button
    public TextMeshProUGUI charName;
    public TextMeshProUGUI charStatus;
    public TextMeshProUGUI charDescription;
    public Slider Slider1;
    public Slider Slider2;
    public Slider Slider3;

    public GameObject character1;
    public GameObject character2;
    public GameObject objRotator;
    public float rotationSpeed = 30f;  // Degrees per second
    private float targetRotation = 180f;  // Target rotation angle
    private float currentRotation = 0f;   // Current rotation
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        charName.text = "Melvz Carpio";
        charStatus.text = "Speed\nAccuracy\nStamina";
        charDescription.text = "Melvz is a lightning-fast fighter, able to close distances in a blink. His speed is unmatched, but his attacks sometimes miss due to his quick movements. With average stamina, he can only maintain his pace for short bursts before needing to recover.";
        Slider1.value = 70f;
        Slider2.value = 60f;
        Slider3.value = 40f;
    }

    // Update is called once per frame
    void Update()
    {
        // Smoothly interpolate from the current rotation to the target rotation
        currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime * rotationSpeed);
        // Apply the new rotation to the object
        transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }
    public void RotateSelection()
    {

        // Toggle the target rotation between 0 and 180 degrees
        if (targetRotation == 180f)
        {
            targetRotation = 0f;
            charName.text = "Maku Valenzuela";
            charStatus.text = "Speed\nAccuracy\nStamina";
            charDescription.text = "Maku is a master with perfect accuracy, capable of hitting any target can. While slower than others, her precision and stamina allow her to stay focused for long stretches, making her deadly from a distance.";
            Slider1.value = 50f;
            Slider2.value = 95f;
            Slider3.value = 60f;
        }
        else
        {
            targetRotation = 180f;
            charName.text = "Melvz Carpio";
            charStatus.text = "Speed\nAccuracy\nStamina";
            charDescription.text = "Melvz is a lightning-fast fighter, able to close distances in a blink. His speed is unmatched, but his attacks sometimes miss due to his quick movements. With average stamina, he can only maintain his pace for short bursts before needing to recover.";
            Slider1.value = 70f;
            Slider2.value = 60f;
            Slider3.value = 40f;
        }
    }
}
