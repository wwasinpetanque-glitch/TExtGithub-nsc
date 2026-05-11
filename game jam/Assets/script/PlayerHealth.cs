using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI References")]
    public TextMeshProUGUI healthText; // ตัวหนังสือบอกเลือด
    public Image screenOverlay;      // แผ่นสีดำที่บังหน้าจอ

    [Header("Darkening Settings")]
    [Range(0, 1)] public float threshold = 0.5f; // จะเริ่มมืดตอนเลือดเหลือกี่ % (0.5 = 50%)
    public float maxDarkness = 0.8f;              // มืดสุดแค่ไหน (0-1)

    [Header("Post Processing Effects")]
    public Volume postProcessVolume;
    private DepthOfField depthOfField;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;
    private LensDistortion lensDistortion;

    void Start()
    {
        currentHealth = maxHealth;
        
        // ดึงค่า Effects ต่างๆ จาก Volume
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out depthOfField);
            postProcessVolume.profile.TryGet(out chromaticAberration);
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out lensDistortion);
        }

        UpdateUI();
    }

    void Update()
    {
        // สำหรับทดสอบ: กด K เพื่อลดเลือด, กด L เพื่อเพิ่มเลือด
        if (Input.GetKeyDown(KeyCode.K)) TakeDamage(10);
        if (Input.GetKeyDown(KeyCode.L)) Heal(10);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    void UpdateUI()
    {
        // 1. อัปเดตตัวหนังสือ
        if (healthText != null)
        {
            healthText.text = "Health: " + Mathf.CeilToInt(currentHealth).ToString();
        }

        // 2. คำนวณความมืดและเบลอของหน้าจอ
        if (screenOverlay != null || depthOfField != null)
        {
            float healthPercent = currentHealth / maxHealth;

            if (healthPercent < threshold)
            {
                // คำนวณความเข้ม (0-1) ของเอฟเฟกต์
                float normalizedLowerHalf = 1 - (healthPercent / threshold); 

                // --- จัดการความมืด (Overlay) ---
                if (screenOverlay != null)
                {
                    float targetAlpha = normalizedLowerHalf * maxDarkness;
                    Color color = screenOverlay.color;
                    color.a = targetAlpha;
                    screenOverlay.color = color;
                }

                // --- จัดการความเบลอ (Depth of Field) ---
                if (depthOfField != null)
                {
                    depthOfField.focusDistance.value = Mathf.Lerp(10f, 0.1f, normalizedLowerHalf);
                    depthOfField.focalLength.value = Mathf.Lerp(50f, 300f, normalizedLowerHalf);
                }

                // --- จัดการภาพซ้อน (Chromatic Aberration) ---
                if (chromaticAberration != null)
                {
                    chromaticAberration.intensity.value = Mathf.Lerp(0f, 1f, normalizedLowerHalf);
                }

                // --- จัดการความมืดขอบจอ (Vignette) ---
                if (vignette != null)
                {
                    vignette.intensity.value = Mathf.Lerp(0f, 0.5f, normalizedLowerHalf);
                }

                // --- จัดการภาพบิดเบี้ยว (Lens Distortion) ---
                if (lensDistortion != null)
                {
                    lensDistortion.intensity.value = Mathf.Lerp(0f, -0.4f, normalizedLowerHalf);
                }
            }
            else
            {
                // ถ้าเลือดมากกว่า threshold ให้กลับเป็นปกติ
                if (screenOverlay != null)
                {
                    Color color = screenOverlay.color;
                    color.a = 0;
                    screenOverlay.color = color;
                }

                if (depthOfField != null)
                {
                    depthOfField.focusDistance.value = 10f;
                    depthOfField.focalLength.value = 50f;
                }

                if (chromaticAberration != null) chromaticAberration.intensity.value = 0f;
                if (vignette != null) vignette.intensity.value = 0f;
                if (lensDistortion != null) lensDistortion.intensity.value = 0f;
            }
        }
    }
}
