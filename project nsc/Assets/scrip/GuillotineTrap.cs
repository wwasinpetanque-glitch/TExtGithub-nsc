using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GuillotineTrap : MonoBehaviour
{
    public enum TrapState
    {
        Idle,            // สแตนด์บาย รอตรวจพบผู้เล่น
        DelayBeforeDrop, // ตรวจพบแล้ว รอดีเลย์เล็กน้อย
        Dropping,        // ใบมีดกำลังสับลงมา
        Bottom,          // ใบมีดค้างที่พื้น
        Raising,         // ดึงใบมีดกลับขึ้น
        Cooldown         // รอก่อนพร้อมรอบใหม่
    }

    // ─────────────────────────────────────────
    [Header("Guillotine Parts (ส่วนประกอบ)")]
    [Tooltip("ลาก Transform ของใบมีดมาใส่ตรงนี้")]
    public Transform blade;

    // ─────────────────────────────────────────
    [Header("Proximity Detection (ระยะตรวจจับ)")]
    [Tooltip("ใบมีดจะสับลงเมื่อผู้เล่นเข้ามาในระยะนี้ (Unity Units)")]
    public float detectionRadius = 2.0f;
    [Tooltip("LayerMask ของผู้เล่น — ถ้าไม่แน่ใจทิ้งไว้ที่ Everything ได้เลย")]
    public LayerMask playerLayer = ~0;

    // ─────────────────────────────────────────
    [Header("Position Settings (กดปุ่มใน Inspector เพื่อบันทึก)")]
    [Tooltip("ตำแหน่ง Local ของใบมีดตอนอยู่ด้านบน (ก่อนสับ)")]
    public Vector3 upperLocalPosition;
    [Tooltip("ตำแหน่ง Local ของใบมีดตอนสับลงมาสุด")]
    public Vector3 lowerLocalPosition;

    // ─────────────────────────────────────────
    [Header("Timing (ความเร็วและเวลา)")]
    [Tooltip("หน่วงกี่วินาทีหลังตรวจพบผู้เล่นก่อนสับ")]
    public float delayBeforeDrop = 0.2f;
    [Tooltip("ใช้เวลากี่วินาทีในการสับลง (น้อย = เร็วมาก)")]
    public float dropDuration = 0.15f;
    [Tooltip("ใช้เวลากี่วินาทีในการดึงกลับขึ้น")]
    public float raiseDuration = 2.5f;
    [Tooltip("ค้างใบมีดที่พื้นกี่วินาทีก่อนดึงกลับ")]
    public float resetDelay = 2.0f;
    [Tooltip("คูลดาวน์หลังรีเซ็ตก่อนพร้อมทำงานรอบใหม่")]
    public float cooldownDuration = 1.0f;

    // ─────────────────────────────────────────
    [Header("Movement Curves (กราฟการเคลื่อนที่)")]
    [Tooltip("กราฟการตกของใบมีด — แนะนำ Ease-In (ช้า→เร็ว) เพื่อจำลองแรงโน้มถ่วง")]
    public AnimationCurve dropCurve  = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 0f),
        new Keyframe(1f, 1f, 2f, 0f));
    [Tooltip("กราฟการดึงกลับขึ้น — แนะนำ Ease-In-Out เพื่อความนุ่มนวล")]
    public AnimationCurve raiseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // ─────────────────────────────────────────
    [Header("Damage (ความเสียหาย)")]
    [Tooltip("จำนวนเลือดที่หักออกจากผู้เล่นเมื่อถูกสับ")]
    public int damage = 5;

    // ─────────────────────────────────────────
    [Header("Reset")]
    [Tooltip("ให้กับดักดึงใบมีดกลับขึ้นและรีเซ็ตอัตโนมัติหรือไม่")]
    public bool autoReset = true;

    // ─────────────────────────────────────────
    [Header("Visual Effects (ไม่บังคับ)")]
    [Tooltip("Prefab เอฟเฟกต์เลือด/กระแทกที่จะสปอนด์ตอนสับโดน")]
    public GameObject hitEffectPrefab;
    [Tooltip("จุดสปอนด์เอฟเฟกต์ — ถ้าว่างจะใช้ตำแหน่งใบมีด")]
    public Transform  hitEffectSpawnPoint;

    // ─────────────────────────────────────────
    [Header("Audio (ไม่บังคับ)")]
    public AudioClip releaseSound;
    public AudioClip dropSound;
    public AudioClip impactSound;
    public AudioClip raiseSound;

    // ─── Private ──────────────────────────────
    private AudioSource  audioSource;
    private TrapState    currentState           = TrapState.Idle;
    private bool         hasDealtDamageThisChop = false;

    // =============================================================
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource              = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake  = false;
            audioSource.spatialBlend = 1f;
        }

        if (blade == null)
        {
            Debug.LogError("[GuillotineTrap] กรุณาลาก Transform ของใบมีดมาใส่ช่อง Blade!");
            return;
        }

        blade.localPosition = upperLocalPosition;
    }

    // =============================================================
    //  PROXIMITY CHECK ทุก Frame
    // =============================================================
    void Update()
    {
        if (currentState != TrapState.Idle || blade == null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player")
                || hit.GetComponent<NewMonoBehaviourScript>() != null
                || hit.GetComponentInParent<NewMonoBehaviourScript>() != null)
            {
                TriggerTrap();
                return;
            }
        }
    }

    // =============================================================
    //  PUBLIC API
    // =============================================================
    public void TriggerTrap()
    {
        if (currentState == TrapState.Idle)
            StartCoroutine(TrapSequence());
    }

    public void ResetTrapToIdle()
    {
        StopAllCoroutines();
        if (blade != null) blade.localPosition = upperLocalPosition;
        currentState           = TrapState.Idle;
        hasDealtDamageThisChop = false;
        Debug.Log("[GuillotineTrap] รีเซ็ตกลับเป็น Idle แล้ว");
    }

    // =============================================================
    //  MAIN COROUTINE
    // =============================================================
    private IEnumerator TrapSequence()
    {
        // 1. ดีเลย์เล็กน้อยก่อนสับ
        currentState           = TrapState.DelayBeforeDrop;
        hasDealtDamageThisChop = false;
        PlaySound(releaseSound);
        yield return new WaitForSeconds(delayBeforeDrop);

        // 2. สับลงมา
        currentState = TrapState.Dropping;
        PlaySound(dropSound);

        yield return MoveBladeRoutine(upperLocalPosition, lowerLocalPosition, dropDuration, dropCurve, dealDamageMidway: true);

        blade.localPosition = lowerLocalPosition;
        TryDealDamage(); // เช็กครั้งสุดท้าย

        // 3. ค้างที่พื้น
        currentState = TrapState.Bottom;
        PlaySound(impactSound);
        yield return new WaitForSeconds(resetDelay);

        // 4. ดึงขึ้น
        if (autoReset)
        {
            currentState = TrapState.Raising;
            PlaySound(raiseSound);

            yield return MoveBladeRoutine(lowerLocalPosition, upperLocalPosition, raiseDuration, raiseCurve, dealDamageMidway: false);

            blade.localPosition = upperLocalPosition;

            // 5. Cooldown
            currentState = TrapState.Cooldown;
            yield return new WaitForSeconds(cooldownDuration);
        }

        currentState = TrapState.Idle;
    }

    // =============================================================
    //  MOVEMENT HELPER
    // =============================================================
    private IEnumerator MoveBladeRoutine(Vector3 from, Vector3 to, float duration, AnimationCurve curve, bool dealDamageMidway)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            blade.localPosition = Vector3.Lerp(from, to, curve.Evaluate(t));

            if (dealDamageMidway && !hasDealtDamageThisChop && t >= 0.5f)
                TryDealDamage();

            yield return null;
        }
    }

    // =============================================================
    //  DAMAGE
    // =============================================================
    private void TryDealDamage()
    {
        if (hasDealtDamageThisChop) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        foreach (Collider hit in hits)
        {
            NewMonoBehaviourScript player =
                hit.GetComponent<NewMonoBehaviourScript>() ??
                hit.GetComponentInParent<NewMonoBehaviourScript>();

            if (player != null || hit.CompareTag("Player"))
            {
                hasDealtDamageThisChop = true;
                player?.TakeHit(damage);

                if (hitEffectPrefab != null)
                {
                    Transform spawnAt = hitEffectSpawnPoint != null ? hitEffectSpawnPoint : blade;
                    Destroy(Instantiate(hitEffectPrefab, spawnAt.position, Quaternion.identity), 4f);
                }
                break;
            }
        }
    }

    // =============================================================
    //  AUDIO
    // =============================================================
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    // =============================================================
    //  GIZMO — แสดงรัศมีใน Scene View
    // =============================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

// =============================================================
//  CUSTOM EDITOR
// =============================================================
#if UNITY_EDITOR
[CustomEditor(typeof(GuillotineTrap))]
public class GuillotineTrapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GuillotineTrap trap = (GuillotineTrap)target;

        GUILayout.Space(15);
        GUILayout.Label("🛠️ ตัวช่วยตั้งค่าตำแหน่งใบมีด", EditorStyles.boldLabel);

        if (GUILayout.Button("1. บันทึกท่าปัจจุบันเป็น 'ตำแหน่งบนสุด' (Upper)", GUILayout.Height(32)))
        {
            if (trap.blade != null)
            {
                Undo.RecordObject(trap, "Set Upper Position");
                trap.upperLocalPosition = trap.blade.localPosition;
                EditorUtility.SetDirty(trap);
                Debug.Log("💾 Upper: " + trap.upperLocalPosition);
            }
            else EditorUtility.DisplayDialog("แจ้งเตือน", "กรุณาลาก Blade มาใส่ในช่องก่อนครับ!", "ตกลง");
        }

        if (GUILayout.Button("2. บันทึกท่าปัจจุบันเป็น 'ตำแหน่งล่างสุด' (Lower)", GUILayout.Height(32)))
        {
            if (trap.blade != null)
            {
                Undo.RecordObject(trap, "Set Lower Position");
                trap.lowerLocalPosition = trap.blade.localPosition;
                EditorUtility.SetDirty(trap);
                Debug.Log("💾 Lower: " + trap.lowerLocalPosition);
            }
            else EditorUtility.DisplayDialog("แจ้งเตือน", "กรุณาลาก Blade มาใส่ในช่องก่อนครับ!", "ตกลง");
        }

        GUILayout.Space(10);
        GUILayout.Label("⚙️ ทดสอบ (เฉพาะตอน Play Mode)", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("🔥 Test Fire", GUILayout.Height(30)))
                trap.TriggerTrap();
            if (GUILayout.Button("🔄 Test Reset", GUILayout.Height(30)))
                trap.ResetTrapToIdle();
            GUILayout.EndHorizontal();

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("กด Play ก่อนถึงจะใช้ปุ่มทดสอบได้ครับ", MessageType.Info);
        }
        GUILayout.Space(10);
    }
}
#endif
