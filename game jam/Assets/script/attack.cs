using UnityEngine;
using UnityEngine.InputSystem;

public class attack : MonoBehaviour
{
    private Animator anim;
    
    [Header("Attack Settings")]
    public float attackCooldown = 0.8f; // เวลาหน่วงระหว่างการโจมตีแต่ละครั้ง
    private float nextAttackTime = 0f;

    void Start()
    {
        // หา Animator ในลูกหรือตัวเอง
        anim = GetComponentInChildren<Animator>();
        if (anim == null) anim = GetComponent<Animator>();
    }

    void Update()
    {
        // ตรวจสอบการคลิกเมาส์ซ้าย และดูว่าหมดเวลา Cooldown หรือยัง
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            PerformAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void PerformAttack()
    {
        if (anim != null)
        {
            // ส่ง Trigger "Attack" ไปที่ Animator
            anim.SetTrigger("Attack");
            Debug.Log("Attack Performed!");
        }
    }

    // เพิ่มฟังก์ชันนี้เพื่อแก้ Error Animation Event
    public void SetAttackEnd()
    {
        Debug.Log("Attack Animation Finished.");
        // คุณสามารถใส่โค้ดที่ต้องการให้ทำตอนจบอนิเมชั่นได้ที่นี่
    }
}

