using UnityEngine;

public class camera : MonoBehaviour
{
    [Header("Bobbing Settings (จังหวะการคลาน)")]
    public float walkingBobbingSpeed = 4f;
    [Tooltip("ระยะการส่ายซ้าย-ขวา (เน้นตัวนี้แทนการขยับขึ้นลง)")]
    public float sideBobbingAmount = 0.06f;
    [Tooltip("ระยะการเอียงคอ (องศา)")]
    public float tiltAmount = 2.0f;
    public float smoothReturnSpeed = 8f;

    [Header("References")]
    public Rigidbody rb;

    private float defaultPosY = 0;
    private float defaultPosX = 0;
    private float timer = 0;

    void Start()
    {
        defaultPosY = transform.localPosition.y;
        defaultPosX = transform.localPosition.x;

        if (rb == null) rb = GetComponentInParent<Rigidbody>();
    }

    void Update()
    {
        Vector3 horizontalVelocity = new Vector3(rb != null ? rb.linearVelocity.x : 0, 0, rb != null ? rb.linearVelocity.z : 0);
        
        if (rb != null && horizontalVelocity.magnitude > 0.1f)
        {
            timer += Time.deltaTime * walkingBobbingSpeed;

            // 1. คำนวณตำแหน่ง (Position)
            // ลบการคำนวณแกน Y ออก (ให้ Y คงที่เท่ากับค่าเริ่มต้นเสมอ)
            float newX = defaultPosX + Mathf.Cos(timer / 2) * sideBobbingAmount;
            transform.localPosition = new Vector3(newX, defaultPosY, transform.localPosition.z);

            // 2. คำนวณการเอียง (Tilt / Roll)
            float tilt = Mathf.Sin(timer / 2) * tiltAmount;
            Vector3 currentRot = transform.localEulerAngles;
            transform.localRotation = Quaternion.Euler(currentRot.x, currentRot.y, tilt);
        }
        else
        {
            timer = 0;
            Vector3 targetPos = new Vector3(defaultPosX, defaultPosY, transform.localPosition.z);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smoothReturnSpeed);

            Vector3 currentRot = transform.localEulerAngles;
            float lerpedZ = Mathf.LerpAngle(currentRot.z, 0, Time.deltaTime * smoothReturnSpeed);
            transform.localRotation = Quaternion.Euler(currentRot.x, currentRot.y, lerpedZ);
        }
    }
}





