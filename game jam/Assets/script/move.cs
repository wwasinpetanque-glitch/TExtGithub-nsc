using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class crawl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float crawlSpeed = 2.5f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f; // แรงดึงลงพิเศษตอนกำลังตก (ยิ่งเยอะยิ่งลงเร็ว)
    public LayerMask groundLayer = ~0;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.5f;
    private float xRotation = 0f;

    private CharacterController controller;
    private Animator anim;
    private Vector3 velocity;
    private Transform rootBone; // ตัวช่วยบล็อกการวาร์ป

    [Header("Debug Info")]
    public bool isGrounded; // โชว์ใน Inspector ให้เห็นเลยว่ายืนบนพื้นไหม

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
        
        if (anim == null) anim = GetComponent<Animator>(); // ลองหาที่ตัวเองด้วยถ้าหาในลูกไม่เจอ

        // 1. ตั้งค่าพื้นฐาน: ล็อคเมาส์
        Cursor.lockState = CursorLockMode.Locked;

        // 2. หากระดูกที่ชื่อ "root" เพื่อหยุดการวาร์ป
        rootBone = transform.Find("root"); 
        if (rootBone == null) rootBone = transform.GetComponentInChildren<Transform>().Find("root");

        // 3. ใช้ค่า CharacterController จาก Inspector (อย่า override ที่นี่)
        velocity.y = -2f; // บังคับดึงลงพื้นตั้งแต่เริ่ม ป้องกันลอย

        // 4. บังคับท่าคลาน
        if (anim != null)
        {
            anim.SetBool("run", true);
            anim.applyRootMotion = false;
        }
        
        // 5. หากล้องอัตโนมัติถ้าไม่ได้ลากใส่
        if (cameraTransform == null && Camera.main != null) 
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void LateUpdate()
    {
        // --- ไม้ตายแก้การวาร์ป ---
        // สั่งให้กระดูกชื่อ root กลับมาอยู่ที่ 0,0,0 ทุกเฟรม ไม่ว่าจะขยับไปไหนก็ตาม
        if (rootBone != null)
        {
            rootBone.localPosition = Vector3.zero;
        }
    }

    void OnAnimatorMove()
    {
        // บล็อกแรงส่งจากแอนิเมชันถาวร
    }

    void HandleMouseLook()
    {
        if (Mouse.current != null && cameraTransform != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            xRotation -= mouseDelta.y * mouseSensitivity;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * (mouseDelta.x * mouseSensitivity));
        }
    }

    void HandleMovement()
    {
        float x = 0, z = 0;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) z = 1;
            if (Keyboard.current.sKey.isPressed) z = -1;
            if (Keyboard.current.aKey.isPressed) x = -1;
            if (Keyboard.current.dKey.isPressed) x = 1;
        }

        // 1. การเดิน (แกน X, Z)
        Vector3 moveDirection = transform.right * x + transform.forward * z;
        controller.Move(moveDirection.normalized * crawlSpeed * Time.deltaTime);

        // 2. เช็คพื้น (คำนวณหาตำแหน่งเท้าอัตโนมัติจาก CharacterController)
        int layerMask = groundLayer.value;
        // หาจุดต่ำสุดของ CharacterController (ก้น Capsule)
        Vector3 bottomPoint = transform.position + controller.center + Vector3.down * (controller.height / 2);
        // ยิงรังสีจากเหนือจุดต่ำสุดขึ้นมานิดนึง ลงไปหาพื้น
        isGrounded = controller.isGrounded || Physics.Raycast(bottomPoint + Vector3.up * 0.1f, Vector3.down, 0.2f, layerMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 3. ระบบกระโดด
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * (gravity < 0 ? gravity : -gravity));
                if (anim != null) anim.SetTrigger("Jump"); // ใช้ Trigger เหมือนเดิม
                Debug.Log("Jumping Success!");
            }
        }

        // 4. แรงโน้มถ่วง (แกน Y)
        if (velocity.y < 0)
        {
            // ถ้ากำลังตกลงมา ให้ใช้แรงโน้มถ่วงปกติคูณด้วย fallMultiplier เพื่อให้ลงพื้นเร็วขึ้น
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            // ถ้ากำลังพุ่งขึ้น ให้ใช้แรงโน้มถ่วงปกติ
            velocity.y += gravity * Time.deltaTime;
        }
        
        controller.Move(velocity * Time.deltaTime);

        // 5. อัปเดต Animator ให้ตรงกับสถานะจริง
        if (anim != null) 
        {
            anim.SetFloat("Speed", moveDirection.magnitude);
            anim.SetBool("isGrounded", isGrounded); // ส่งค่าว่าแตะพื้นไหมไปให้ Animator ด้วย
        }
    }
}
