using UnityEngine;

public class CampfireLight : MonoBehaviour
{
    [Header("Settings")]
    public Light campfireLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerSpeed = 0.1f;

    [Header("Movement (Optional)")]
    public bool useMovement = true;
    public float movementAmount = 0.05f;

    private float targetIntensity;
    private Vector3 basePosition;

    void Start()
    {
        if (campfireLight == null)
            campfireLight = GetComponent<Light>();
        
        basePosition = transform.localPosition;
    }

    void Update()
    {
        if (campfireLight == null) return;

        // 1. Random Intensity Flickering
        // Using Mathf.PerlinNoise for a more "organic" feel than Random.Range
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed * 10f, 0);
        campfireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        // 2. Subtle Position Jitter (Simulates flame movement)
        if (useMovement)
        {
            Vector3 jitter = new Vector3(
                Mathf.PerlinNoise(Time.time * flickerSpeed * 5f, 10) - 0.5f,
                Mathf.PerlinNoise(Time.time * flickerSpeed * 5f, 20) - 0.5f,
                Mathf.PerlinNoise(Time.time * flickerSpeed * 5f, 30) - 0.5f
            ) * movementAmount;
            
            transform.localPosition = basePosition + jitter;
        }
    }
}
