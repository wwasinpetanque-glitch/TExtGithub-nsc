using UnityEngine;

public class syringe : MonoBehaviour
{
    [Header("Syringe Settings")]
    public int healAmount = 3;              // ฟื้นเลือดกี่หน่วย (0 = รีเซ็ต timer อย่างเดียว)
    public float rotateSpeed = 90f;         // หมุนรอบตัวเองให้ดูน่าเก็บ
    public GameObject pickupEffect;         // Effect ตอนเก็บ (ใส่ถ้ามี)
    public string playerTag = "Player";     // Tag ของผู้เล่น

    void Update()
    {
        // หมุนรอบตัวเองให้ดูน่าเก็บ
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    // เมื่อผู้เล่นชนกับเข็ม (ต้องติด Collider + ติ๊ก Is Trigger)
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // หา script 'heal' จากตัวผู้เล่น (หรือตัวที่เก็บ inventory)
        heal inventory = other.GetComponent<heal>();

        if (inventory != null)
        {
            inventory.AddSyringe(1); // เพิ่มเข็ม 1 อัน
            Debug.Log("เก็บเข็มฉีดยาลงในกระเป๋า!");
        }
        else
        {
            // ถ้าไม่มี script heal ให้ลองใช้ทันทีเหมือนเดิม (Fallback)
            NewMonoBehaviourScript bloodSystem = other.GetComponent<NewMonoBehaviourScript>();
            if (bloodSystem != null)
            {
                bloodSystem.UseMedicine(healAmount);
            }
        }

        // Spawn effect ถ้ามี
        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        // ทำลายเข็มหลังเก็บ
        Destroy(gameObject);
    }
}
