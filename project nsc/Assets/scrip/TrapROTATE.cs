using UnityEngine;

public class TrapROTATE : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Rotation speed around X, Y, and Z axes (degrees per second)")]
    public Vector3 rotationSpeed = new Vector3(0f, 100f, 0f);

    [Tooltip("If true, rotates relative to the local coordinate system. Otherwise, uses world coordinates.")]
    public bool useLocalRotation = true;

    [Header("Special Effects (Optional)")]
    [Tooltip("Enable oscillating/ping-pong rotation (like a pendulum) instead of continuous rotation")]
    public bool oscillate = false;

    [Tooltip("The maximum angle of oscillation in degrees")]
    public float oscillationAngle = 45f;

    [Tooltip("How fast the oscillation repeats")]
    public float oscillationSpeed = 2f;

    // Internal state for oscillation
    private Vector3 initialRotation;
    private float oscillationTimer = 0f;

    void Start()
    {
        // Save initial rotation if we are oscillating
        if (useLocalRotation)
        {
            initialRotation = transform.localEulerAngles;
        }
        else
        {
            initialRotation = transform.eulerAngles;
        }
    }

    void Update()
    {
        if (oscillate)
        {
            HandleOscillation();
        }
        else
        {
            HandleContinuousRotation();
        }
    }

    private void HandleContinuousRotation()
    {
        // Calculate rotation step based on frame time
        Vector3 rotationStep = rotationSpeed * Time.deltaTime;

        if (useLocalRotation)
        {
            transform.Rotate(rotationStep, Space.Self);
        }
        else
        {
            transform.Rotate(rotationStep, Space.World);
        }
    }

    private void HandleOscillation()
    {
        oscillationTimer += Time.deltaTime * oscillationSpeed;
        
        // Calculate the oscillation factor using a sine wave (-1 to 1)
        float factor = Mathf.Sin(oscillationTimer);
        
        // Calculate current rotation offset
        Vector3 offset = rotationSpeed.normalized * (factor * oscillationAngle);
        Vector3 targetRotation = initialRotation + offset;

        if (useLocalRotation)
        {
            transform.localRotation = Quaternion.Euler(targetRotation);
        }
        else
        {
            transform.rotation = Quaternion.Euler(targetRotation);
        }
    }
}
