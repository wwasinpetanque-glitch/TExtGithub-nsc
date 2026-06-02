using UnityEngine;

public class aienemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float moveSpeed = 3f;
    public float attackRange = 2f;
    public float damageToPlayer = 20f;
    public float maxHealth = 100f;

    [Header("Detection")]
    public string playerTag = "Player";

    private Transform player;
    private Animator anim;
    private PlayerHealth playerHP;
    private float currentHealth;
    private float lastAttackTime;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        
        // ปิด Root Motion
        if (anim != null) anim.applyRootMotion = false;

        FindPlayer();

        // บังคับฟิสิกส์ให้เป็น Kinematic เพื่อไม่ให้โดนดีด
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) 
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // แนะนำ: ให้ติ๊กช่อง "Is Trigger" ที่ Collider ของมอนสเตอร์ด้วยครับ
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null)
        {
            player = p.transform;
            playerHP = p.GetComponent<PlayerHealth>();
            if (playerHP == null) playerHP = p.GetComponentInChildren<PlayerHealth>();
        }
    }

    void Update()
    {
        if (player == null) { FindPlayer(); return; }

        float distance = Vector3.Distance(transform.position, player.position);

        // คำนวณตำแหน่งเป้าหมายในแนวราบ
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);

        if (distance > attackRange)
        {
            // หันหน้าหา
            transform.LookAt(targetPos);
            
            // เคลื่อนที่เข้าหาแบบตรงไปตรงมา (MoveTowards จะนิ่งกว่า Translate)
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (anim != null) anim.SetBool("isWalking", true);
        }
        else
        {
            if (anim != null) anim.SetBool("isWalking", false);
            
            // หันหน้าหาตอนโจมตี
            transform.LookAt(targetPos);

            if (Time.time >= lastAttackTime + 1.5f)
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        if (anim != null) anim.SetTrigger("Attack");
        if (playerHP != null) playerHP.TakeDamage(damageToPlayer);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) 
        {
            if (anim != null) anim.SetTrigger("Die");
            this.enabled = false;
            Destroy(gameObject, 2f);
        }
    }
}
