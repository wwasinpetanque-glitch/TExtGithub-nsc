using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class crawl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float crawlSpeed = 2.5f;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.5f;
    private float xRotation = 0f;

    private Rigidbody rb;
    private Animator anim;
    private Transform rootBone;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        // ตั้งค่า Rigidbody เพื่อไม่ให้ล้มพับ
        rb.freezeRotation = true;
        rb.useGravity = true;

        // ล็อคเมาส์
        Cursor.lockState = CursorLockMode.Locked;

        // หากระดูก root
        rootBone = transform.Find("root"); 
        if (rootBone == null) rootBone = transform.GetComponentInChildren<Transform>().Find("root");

        // ปิด Root Motion
        if (anim != null)
        {
            anim.SetBool("isCrawling", true);
            anim.applyRootMotion = false;
        }
        
        if (cameraTransform == null && Camera.main != null) 
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleMouseLook();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void LateUpdate()
    {
        if (rootBone != null) rootBone.localPosition = Vector3.zero;
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

        // คำนวณทิศทาง
        Vector3 moveDir = transform.right * x + transform.forward * z;
        moveDir = moveDir.normalized * crawlSpeed;

        // รักษาความเร็ว Y (แรงโน้มถ่วง) ไว้
        rb.linearVelocity = new Vector3(moveDir.x, rb.linearVelocity.y, moveDir.z);

        if (anim != null) anim.SetFloat("Speed", moveDir.magnitude);
    }
}

