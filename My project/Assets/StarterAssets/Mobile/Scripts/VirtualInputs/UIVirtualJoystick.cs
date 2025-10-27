using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [System.Serializable]
    public class Event : UnityEvent<Vector2> { }

    [Header("Rect References")]
    public RectTransform containerRect;
    public RectTransform handleRect;

    [Header("Settings")]
    public float joystickRange = 50f;
    public float magnitudeMultiplier = 1f;
    public bool invertXOutputValue;
    public bool invertYOutputValue;

    [Header("Output")]
    public Event joystickOutputEvent;

    [Header("Joystick Mode")]
    public bool isCameraController = false; // false = movement joystick, true = camera joystick
    public PlayerCameraController cameraController; // Camera controller reference
    public CharacterControllerMovement characterControllerMovement; // Player movement reference

    private void Start()
    {
        SetupHandle();

        // ✅ Try to auto-detect the controller if not assigned manually
        if (isCameraController && cameraController == null)
        {
            cameraController = FindObjectOfType<PlayerCameraController>();
        }
        else if (!isCameraController && characterControllerMovement == null)
        {
            characterControllerMovement = FindObjectOfType<CharacterControllerMovement>();
        }

        // ✅ Automatically link joystick output to the correct controller
        joystickOutputEvent.AddListener(OnJoystickOutput);
    }

    private void SetupHandle()
    {
        if (handleRect)
        {
            UpdateHandleRectPosition(Vector2.zero);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            containerRect, eventData.position, eventData.pressEventCamera, out Vector2 position);

        position = ApplySizeDelta(position);
        Vector2 clampedPosition = ClampValuesToMagnitude(position);
        Vector2 outputPosition = ApplyInversionFilter(clampedPosition);

        OutputPointerEventValue(outputPosition * magnitudeMultiplier);

        if (handleRect)
        {
            UpdateHandleRectPosition(clampedPosition * joystickRange);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OutputPointerEventValue(Vector2.zero);
        if (handleRect)
        {
            UpdateHandleRectPosition(Vector2.zero);
        }
    }

    private void OutputPointerEventValue(Vector2 pointerPosition)
    {
        joystickOutputEvent.Invoke(pointerPosition);
    }

    private void UpdateHandleRectPosition(Vector2 newPosition)
    {
        handleRect.anchoredPosition = newPosition;
    }

    Vector2 ApplySizeDelta(Vector2 position)
    {
        float x = (position.x / containerRect.sizeDelta.x) * 2.5f;
        float y = (position.y / containerRect.sizeDelta.y) * 2.5f;
        return new Vector2(x, y);
    }

    Vector2 ClampValuesToMagnitude(Vector2 position)
    {
        return Vector2.ClampMagnitude(position, 1);
    }

    Vector2 ApplyInversionFilter(Vector2 position)
    {
        if (invertXOutputValue)
            position.x = -position.x;
        if (invertYOutputValue)
            position.y = -position.y;

        return position;
    }

    // ✅ Automatically routes joystick data to the correct controller
    private void OnJoystickOutput(Vector2 input)
    {
        if (isCameraController)
        {
            if (cameraController)
                cameraController.OnCameraJoystickInput(input);
        }
        else
        {
            if (characterControllerMovement)
                characterControllerMovement.OnJoystickInput(input);
        }
    }
}

