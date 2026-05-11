using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Blood / Health Settings")]
    public int maxBlood = 10;           // เลือดสูงสุด
    public int currentBlood;            // เลือดปัจจุบัน

    [Header("Crawl Drain Settings")]
    public float drainDelay = 3f;       // รอกี่วินาทีก่อนเลือดจะเริ่มลด (ตอนคลาน)
    public float drainInterval = 2f;    // หลังจาก delay แล้ว เลือดลดทุกกี่วินาที
    public int drainAmount = 1;         // เลือดลดครั้งละกี่

    [Header("State")]
    public bool isCrawling = false;     // ตอนนี้คลานอยู่ไหม?

    [Header("UI (Optional)")]
    public Slider bloodSlider;          // Slider แสดงเลือด (ใส่ถ้ามี)
    public Text bloodText;              // Text แสดงเลือด (ใส่ถ้ามี)

    // ตัวแปร internal
    private float delayTimer = 0f;      // นับ delay ก่อนเลือดลด
    private float intervalTimer = 0f;   // นับ interval ระหว่างการลดแต่ละครั้ง
    private bool delayDone = false;     // delay ผ่านแล้วหรือยัง

    void Start()
    {
        currentBlood = maxBlood;
        UpdateUI();
    }

    void Update()
    {
        HandleCrawlDrain();
    }

    // ============================================================
    //  CRAWL DRAIN LOGIC
    // ============================================================
    void HandleCrawlDrain()
    {
        if (!isCrawling) return;  // ถ้าไม่ได้คลาน ไม่ทำอะไร

        if (!delayDone)
        {
            // Phase 1: รอ delay ก่อน
            delayTimer += Time.deltaTime;
            if (delayTimer >= drainDelay)
            {
                delayDone = true;
                intervalTimer = 0f;

                // ลดเลือดทันทีที่ delay ครบ
                DrainBlood();
            }
        }
        else
        {
            // Phase 2: ลดเลือดทุก interval
            intervalTimer += Time.deltaTime;
            if (intervalTimer >= drainInterval)
            {
                intervalTimer = 0f;
                DrainBlood();
            }
        }
    }

    // ============================================================
    //  เริ่มคลาน → เรียกเมื่อ animation คลานเริ่มทำงาน
    // ============================================================
    public void StartCrawling()
    {
        isCrawling = true;
        ResetDrainTimer(); // รีเซ็ต timer ทุกครั้งที่เริ่มคลาน
        Debug.Log("เริ่มคลาน - รอ " + drainDelay + " วินาทีก่อนเลือดลด");
    }

    // ============================================================
    //  หยุดคลาน → เรียกเมื่อหยุดเคลื่อนที่
    // ============================================================
    public void StopCrawling()
    {
        isCrawling = false;
        ResetDrainTimer();
        Debug.Log("หยุดคลาน");
    }

    // ============================================================
    //  โดนโจมตี → เลือดลดทันที
    // ============================================================
    public void TakeHit(int damage = 1)
    {
        currentBlood -= damage;
        currentBlood = Mathf.Clamp(currentBlood, 0, maxBlood);
        UpdateUI();
        Debug.Log("โดนโจมตี! เลือดเหลือ: " + currentBlood);

        if (currentBlood <= 0)
        {
            OnDead();
        }
    }

    // ============================================================
    //  กินยา → รีเซ็ต timer + ฟื้นเลือด (ถ้าต้องการ)
    // ============================================================
    public void UseMedicine(int healAmount = 0)
    {
        // รีเซ็ต drain timer (delay เริ่มใหม่)
        ResetDrainTimer();

        // ถ้ายาฟื้นเลือดด้วย
        if (healAmount > 0)
        {
            currentBlood += healAmount;
            currentBlood = Mathf.Clamp(currentBlood, 0, maxBlood);
            UpdateUI();
        }

        Debug.Log("กินยา! รีเซ็ต timer. เลือดเหลือ: " + currentBlood);
    }

    // ============================================================
    //  Internal: ลดเลือดจากการคลาน
    // ============================================================
    void DrainBlood()
    {
        currentBlood -= drainAmount;
        currentBlood = Mathf.Clamp(currentBlood, 0, maxBlood);
        UpdateUI();
        Debug.Log("คลาน: เลือดลด " + drainAmount + " เหลือ " + currentBlood);

        if (currentBlood <= 0)
        {
            OnDead();
        }
    }

    // ============================================================
    //  รีเซ็ต timer ทั้งหมด
    // ============================================================
    void ResetDrainTimer()
    {
        delayTimer = 0f;
        intervalTimer = 0f;
        delayDone = false;
    }

    // ============================================================
    //  ตายแล้ว
    // ============================================================
    void OnDead()
    {
        isCrawling = false;
        Debug.Log("ตายแล้ว!");
        // TODO: ใส่ logic game over หรือ animation ตายตรงนี้
    }

    // ============================================================
    //  อัปเดต UI
    // ============================================================
    void UpdateUI()
    {
        if (bloodSlider != null)
        {
            bloodSlider.maxValue = maxBlood;
            bloodSlider.value = currentBlood;
        }

        if (bloodText != null)
        {
            bloodText.text = "เลือด: " + currentBlood + " / " + maxBlood;
        }
    }
}
