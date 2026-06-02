using UnityEngine;
using System.Collections;

public class BearTrap : MonoBehaviour
{
    [Header("Trap Jaws (โมเดลส่วนปากของกับดัก)")]
    [Tooltip("ลาก Object ปากกับดักฝั่งซ้ายมาใส่ตรงนี้")]
    public Transform leftJaw;
    [Tooltip("ลาก Object ปากกับดักฝั่งขวามาใส่ตรงนี้")]
    public Transform rightJaw;

    [Header("Rotation Settings (ค่ามุมในการเปิด/ปิด)")]
    [Tooltip("มุมของกรามซ้ายตอนกับดัก 'เปิดอยู่' (เช่น กางออก)")]
    public Vector3 leftJawOpenRotation = new Vector3(0, 0, -90);
    [Tooltip("มุมของกรามซ้ายตอนกับดัก 'งับปิดลงมา'")]
    public Vector3 leftJawClosedRotation = new Vector3(0, 0, 0);
    
    [Space(5)]
    [Tooltip("มุมของกรามขวาตอนกับดัก 'เปิดอยู่' (เช่น กางออก)")]
    public Vector3 rightJawOpenRotation = new Vector3(0, 0, 90);
    [Tooltip("มุมของกรามขวาตอนกับดัก 'งับปิดลงมา'")]
    public Vector3 rightJawClosedRotation = new Vector3(0, 0, 0);
    
    [Header("Position Settings (ระบบจะบันทึกให้อัตโนมัติเมื่อกดปุ่มบันทึก)")]
    public Vector3 leftJawOpenPosition;
    public Vector3 leftJawClosedPosition;
    public Vector3 rightJawOpenPosition;
    public Vector3 rightJawClosedPosition;
    
    [Tooltip("ความเร็วในการสับ/งับปิดของกับดัก (หน่วยเป็นวินาที, ยิ่งน้อยยิ่งสับเร็ว)")]
    public float snapSpeed = 0.08f;
    
    [Header("Damage Settings (ความเสียหาย)")]
    [Tooltip("จำนวนดาเมจที่จะหักจากเลือดของผู้เล่น")]
    public int damage = 2;
    
    [Header("Reset Settings (การรีเซ็ตกลับดัก)")]
    [Tooltip("เปิดใช้งานเพื่อให้กับดักเปิดออกอีกครั้งโดยอัตโนมัติหลังจากงับแล้ว")]
    public bool autoReset = true;
    [Tooltip("เวลาที่รอก่อนจะเริ่มเปิดกับดักใหม่อีกครั้ง (หน่วยวินาที)")]
    public float resetDelay = 3f;

    private bool isClosed = false;
    private bool isAnimating = false;

#if UNITY_EDITOR
    [ContextMenu("1. บันทึกมุมเปิด (Set Open Rotation จากท่าปัจจุบัน)")]
    private void SetOpenRotationFromCurrent()
    {
        if (leftJaw != null && rightJaw != null)
        {
            leftJawOpenRotation = leftJaw.localEulerAngles;
            leftJawOpenPosition = leftJaw.localPosition;
            rightJawOpenRotation = rightJaw.localEulerAngles;
            rightJawOpenPosition = rightJaw.localPosition;
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("บันทึกมุมและตำแหน่งเปิดเรียบร้อยแล้ว!");
        }
        else
        {
            Debug.LogError("กรุณาใส่ Transform ของ Left Jaw และ Right Jaw ก่อนกดบันทึกครับ!");
        }
    }

    [ContextMenu("2. บันทึกมุมปิด (Set Closed Rotation จากท่าปัจจุบัน)")]
    private void SetClosedRotationFromCurrent()
    {
        if (leftJaw != null && rightJaw != null)
        {
            leftJawClosedRotation = leftJaw.localEulerAngles;
            leftJawClosedPosition = leftJaw.localPosition;
            rightJawClosedRotation = rightJaw.localEulerAngles;
            rightJawClosedPosition = rightJaw.localPosition;
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("บันทึกมุมและตำแหน่งปิดเรียบร้อยแล้ว!");
        }
        else
        {
            Debug.LogError("กรุณาใส่ Transform ของ Left Jaw และ Right Jaw ก่อนกดบันทึกครับ!");
        }
    }
#endif

    void Start()
    {
        // ตรวจสอบว่ามีการใส่ Transform ของปากกับดักมาครบถ้วน
        if (leftJaw == null || rightJaw == null)
        {
            Debug.LogError("กรุณาใส่ Transform ของ Left Jaw และ Right Jaw ในช่องของ BearTrap ด้วยครับ!");
            return;
        }

        // ตั้งค่าให้กรามทั้งสองฝั่งอยู่ในตำแหน่ง "เปิด" ตั้งแต่เริ่มเกม
        leftJaw.localPosition = leftJawOpenPosition;
        leftJaw.localRotation = Quaternion.Euler(leftJawOpenRotation);
        rightJaw.localPosition = rightJawOpenPosition;
        rightJaw.localRotation = Quaternion.Euler(rightJawOpenRotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ตรวจจับเมื่อผู้เล่นเดินมาเหยียบ (ใช้ Trigger Collider บนตัวฐานของกับดัก)
        if (!isClosed && !isAnimating)
        {
            // ตรวจสอบว่าวัตถุที่เข้ามาชนมีสคริปต์เลือดของผู้เล่นอยู่หรือไม่ (คลาส NewMonoBehaviourScript)
            NewMonoBehaviourScript playerHealth = other.GetComponent<NewMonoBehaviourScript>();
            
            // หรือตรวจสอบจาก Tag "Player"
            if (playerHealth != null || other.CompareTag("Player"))
            {
                SnapShut(playerHealth);
            }
        }
    }

    // ฟังก์ชันงับปิดของกับดักหมี
    public void SnapShut(NewMonoBehaviourScript playerHealth = null)
    {
        if (isClosed || isAnimating) return;
        StartCoroutine(SnapRoutine(playerHealth));
    }

    private IEnumerator SnapRoutine(NewMonoBehaviourScript playerHealth)
    {
        isAnimating = true;
        isClosed = true;

        // ลดเลือดผู้เล่นทันทีถ้ามีข้อมูล
        if (playerHealth != null)
        {
            playerHealth.TakeHit(damage);
        }

        // เล่นเสียงงับปิด (หากติดตั้ง AudioSource ไว้ที่ตัวกับดัก)
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }

        float elapsed = 0f;
        Quaternion leftStartRot = Quaternion.Euler(leftJawOpenRotation);
        Quaternion leftEndRot = Quaternion.Euler(leftJawClosedRotation);
        Quaternion rightStartRot = Quaternion.Euler(rightJawOpenRotation);
        Quaternion rightEndRot = Quaternion.Euler(rightJawClosedRotation);

        Vector3 leftStartPos = leftJawOpenPosition;
        Vector3 leftEndPos = leftJawClosedPosition;
        Vector3 rightStartPos = rightJawOpenPosition;
        Vector3 rightEndPos = rightJawClosedPosition;

        // งับปิดอย่างรวดเร็ว (Snap Shut Animation)
        while (elapsed < snapSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / snapSpeed;
            
            // ใช้ t * t เพื่อทำให้จุดเริ่มต้นรวดเร็วและกระแทกปิดลงอย่างรวดเร็ว (Ease-In Effect)
            t = t * t; 

            leftJaw.localRotation = Quaternion.Slerp(leftStartRot, leftEndRot, t);
            leftJaw.localPosition = Vector3.Lerp(leftStartPos, leftEndPos, t);

            rightJaw.localRotation = Quaternion.Slerp(rightStartRot, rightEndRot, t);
            rightJaw.localPosition = Vector3.Lerp(rightStartPos, rightEndPos, t);
            yield return null;
        }

        leftJaw.localRotation = leftEndRot;
        leftJaw.localPosition = leftEndPos;
        rightJaw.localRotation = rightEndRot;
        rightJaw.localPosition = rightEndPos;
        isAnimating = false;

        Debug.Log("BearTrap snapped shut!");

        // ถ้ารีเซ็ตอัตโนมัติ ให้หน่วงเวลาและเปิดออกใหม่
        if (autoReset)
        {
            yield return new WaitForSeconds(resetDelay);
            StartCoroutine(ResetRoutine());
        }
    }

    private IEnumerator ResetRoutine()
    {
        isAnimating = true;
        
        float elapsed = 0f;
        float resetSpeed = snapSpeed * 6f; // ค่อยๆ กางออกช้าๆ กว่าตอนงับ
        Quaternion leftStartRot = Quaternion.Euler(leftJawClosedRotation);
        Quaternion leftEndRot = Quaternion.Euler(leftJawOpenRotation);
        Quaternion rightStartRot = Quaternion.Euler(rightJawClosedRotation);
        Quaternion rightEndRot = Quaternion.Euler(rightJawOpenRotation);

        Vector3 leftStartPos = leftJawClosedPosition;
        Vector3 leftEndPos = leftJawOpenPosition;
        Vector3 rightStartPos = rightJawClosedPosition;
        Vector3 rightEndPos = rightJawOpenPosition;

        // ค่อยๆ กางกรามออก
        while (elapsed < resetSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / resetSpeed;
            t = Mathf.SmoothStep(0f, 1f, t); // หมุนออกอย่างนุ่มนวล

            leftJaw.localRotation = Quaternion.Slerp(leftStartRot, leftEndRot, t);
            leftJaw.localPosition = Vector3.Lerp(leftStartPos, leftEndPos, t);

            rightJaw.localRotation = Quaternion.Slerp(rightStartRot, rightEndRot, t);
            rightJaw.localPosition = Vector3.Lerp(rightStartPos, rightEndPos, t);
            yield return null;
        }

        leftJaw.localRotation = leftEndRot;
        leftJaw.localPosition = leftEndPos;
        rightJaw.localRotation = rightEndRot;
        rightJaw.localPosition = rightEndPos;
        
        isClosed = false;
        isAnimating = false;
        Debug.Log("BearTrap reset and ready!");
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(BearTrap))]
public class BearTrapEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        // วาดข้อมูล Inspector ตามปกติขึ้นมาก่อน
        DrawDefaultInspector();

        BearTrap trap = (BearTrap)target;

        GUILayout.Space(15);
        GUILayout.Label("🛠️ ตัวช่วยตั้งค่าทิศทางกับดัก (Helper Tools)", UnityEditor.EditorStyles.boldLabel);
        
        // ปุ่มบันทึกมุมเปิด
        if (GUILayout.Button("1. บันทึกท่าปัจจุบันเป็น 'มุมตอนเปิด' (Open)", GUILayout.Height(32)))
        {
            if (trap.leftJaw != null && trap.rightJaw != null)
            {
                trap.leftJawOpenRotation = trap.leftJaw.localEulerAngles;
                trap.leftJawOpenPosition = trap.leftJaw.localPosition;
                trap.rightJawOpenRotation = trap.rightJaw.localEulerAngles;
                trap.rightJawOpenPosition = trap.rightJaw.localPosition;
                UnityEditor.EditorUtility.SetDirty(trap);
                Debug.Log("💾 บันทึกมุมเปิดและตำแหน่งสำเร็จ! ซ้าย: " + trap.leftJawOpenRotation + " | ขวา: " + trap.rightJawOpenRotation);
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog("แจ้งเตือน", "กรุณาลาก Left Jaw และ Right Jaw ใส่ในช่องของสคริปต์ก่อนครับ!", "รับทราบ");
            }
        }

        // ปุ่มบันทึกมุมปิด
        if (GUILayout.Button("2. บันทึกท่าปัจจุบันเป็น 'มุมตอนปิด' (Closed)", GUILayout.Height(32)))
        {
            if (trap.leftJaw != null && trap.rightJaw != null)
            {
                trap.leftJawClosedRotation = trap.leftJaw.localEulerAngles;
                trap.leftJawClosedPosition = trap.leftJaw.localPosition;
                trap.rightJawClosedRotation = trap.rightJaw.localEulerAngles;
                trap.rightJawClosedPosition = trap.rightJaw.localPosition;
                UnityEditor.EditorUtility.SetDirty(trap);
                Debug.Log("💾 บันทึกมุมปิดและตำแหน่งสำเร็จ! ซ้าย: " + trap.leftJawClosedRotation + " | ขวา: " + trap.rightJawClosedRotation);
            }
            else
            {
                UnityEditor.EditorUtility.DisplayDialog("แจ้งเตือน", "กรุณาลาก Left Jaw และ Right Jaw ใส่ในช่องของสคริปต์ก่อนครับ!", "รับทราบ");
            }
        }
        GUILayout.Space(10);
    }
}
#endif

