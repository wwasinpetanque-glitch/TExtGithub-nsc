using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class crawl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float crawlSpeed = 2.5f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.5f;
    private float xRotation = 0f;

    private CharacterController controller;
    private Animator anim;
    private Vector3 velocity;
    private Transform rootBone; // ตัวช่วยบล็อกการวาร์ป

    void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();

        // 1. ตั้งค่าพื้นฐาน: ล็อคเมาส์
        Cursor.lockState = CursorLockMode.Locked;

        // 2. หากระดูกที่ชื่อ "root" เพื่อหยุดการวาร์ป
        rootBone = transform.Find("root"); 
        if (rootBone == null) rootBone = transform.GetComponentInChildren<Transform>().Find("root");

        // 3. ปรับขนาดตัวละครให้เตี้ยลง (ท่าคลาน) อัตโนมัติ
        controller.height = 0.6f;
        controller.center = new Vector3(0, 0.3f, 0);

        // 4. บังคับท่าคลาน
        if (anim != null)
        {
            anim.SetBool("isCrawling", true);
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

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move.normalized * crawlSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;

        if (anim != null) anim.SetFloat("Speed", move.magnitude);
    }
}
