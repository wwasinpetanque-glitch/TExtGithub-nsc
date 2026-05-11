using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Hallucinations : MonoBehaviour
{
    [Header("References")]
    public NewMonoBehaviourScript healthSystem;
    public Volume volume;

    [Header("Settings")]
    [Range(0, 1)] public float threshold = 0.6f; 
    public float pulseSpeed = 4f; // ปรับให้เต้นเร็วขึ้นนิดนึง

    [Header("Vision Blur & Double")]
    public float maxBlurIntensity = 40f; 
    public float heavyJitterIntensity = 0.08f; // เพิ่มความแรงการสั่นภาพซ้อน (เดิม 0.03)
    public float maxChromaticAberration = 2.0f; // เพิ่มการแยกสีเป็น 2 เท่า!
    public float maxExtraVignette = 0.2f;       // ปรับค่าความเข้มของขอบจอที่เพิ่มเข้ามา (เดิม 0.45)


    private DepthOfField dof;
    private ChromaticAberration chromatic;
    private Vignette vignette;
    private LensDistortion distortion;
    private MotionBlur motionBlur;
    private PaniniProjection panini; // เพิ่มตัวช่วยยืดภาพให้ดูหลอน
    private Bloom bloom; // เพิ่มแสงฟุ้งเพื่อให้ภาพซ้อนดูนวลหนาขึ้น

    private float originalVignetteIntensity;
    private Color originalVignetteColor;
    private float originalChromaticIntensity;

    void Start()
    {
        if (healthSystem == null) 
            healthSystem = Object.FindAnyObjectByType<NewMonoBehaviourScript>();

        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out dof);
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out chromatic);
            volume.profile.TryGet(out distortion);
            volume.profile.TryGet(out motionBlur);
            volume.profile.TryGet(out panini);
            volume.profile.TryGet(out bloom);

            if (vignette != null) { originalVignetteIntensity = vignette.intensity.value; originalVignetteColor = vignette.color.value; }
            if (chromatic != null) originalChromaticIntensity = chromatic.intensity.value;
        }
    }

    void Update()
    {
        if (healthSystem == null || volume == null) return;

        float healthRatio = (float)healthSystem.currentBlood / healthSystem.maxBlood;
        float stress = 0f;

        if (healthRatio < threshold)
        {
            stress = 1f - (healthRatio / threshold);
            stress = Mathf.Pow(stress, 1.3f); // ปรับ Curve ให้เริ่มหลอนไวขึ้นนิดหน่อย
        }

        ApplyHallucinationEffects(stress);
    }

    void ApplyHallucinationEffects(float stress)
    {
        // 1. Double Vision Jitter (สั่นภาพซ้อนแบบหนัก)
        if (distortion != null)
        {
            distortion.active = true;
            // สุ่มสั่นแบบกว้างและรวดเร็ว
            float jitterX = Random.Range(-heavyJitterIntensity, heavyJitterIntensity) * stress;
            float jitterY = Mathf.Sin(Time.time * 15f) * heavyJitterIntensity * 0.5f * stress; // เพิ่มจังหวะเหวี่ยงขึ้นลง
            distortion.center.value = new Vector2(0.5f + jitterX, 0.5f + jitterY);
            distortion.intensity.value = (stress * -0.3f);
        }

        // 2. Heavy Chromatic Aberration (แยกสีซ้อนกัน 3 ชั้น)
        if (chromatic != null)
        {
            chromatic.intensity.value = originalChromaticIntensity + (stress * maxChromaticAberration);
        }

        // 3. Bloom (ทำให้ภาพซ้อนดูเบลอและนวลหนาขึ้น)
        if (bloom != null)
        {
            bloom.active = stress > 0.2f;
            bloom.intensity.value = stress * 5f; // เพิ่มความฟุ้ง
        }

        // 4. Panini Projection (ยืดภาพให้ดูมึนหัว)
        if (panini != null)
        {
            panini.active = stress > 0.3f;
            panini.distance.value = stress * 0.8f;
        }

        // 5. Depth of Field (เบลอตามสายตาสั้น)
        if (dof != null)
        {
            dof.active = stress > 0.05f;
            dof.focusDistance.value = Mathf.Lerp(10f, 0.1f, stress); 
            dof.aperture.value = Mathf.Lerp(1f, maxBlurIntensity, stress);
        }

        // 6. Vignette & Motion Blur (ขอบจอและเงาลาก)
        if (vignette != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.1f * stress;
            vignette.intensity.value = Mathf.Max(originalVignetteIntensity, originalVignetteIntensity + (stress * maxExtraVignette) + pulse);

            vignette.color.value = Color.Lerp(originalVignetteColor, new Color(0.7f, 0f, 0f), stress);
        }
        if (motionBlur != null)
        {
            motionBlur.active = stress > 0.1f;
            motionBlur.intensity.value = stress * 1.0f;
        }
    }
}
