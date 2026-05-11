using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class heal : MonoBehaviour
{
    [Header("Inventory")]
    public int syringeCount = 0;        
    public int healAmount = 3;         

    [Header("UI References")]
    public TextMeshProUGUI syringeCountText; 

    [Header("References")]
    public NewMonoBehaviourScript healthSystem; 

    void Start()
    {
        if (healthSystem == null)
            healthSystem = GetComponent<NewMonoBehaviourScript>();
        
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseSyringe();
        }
    }

    public void AddSyringe(int amount = 1)
    {
        syringeCount += amount;
        UpdateUI();
    }

    void UseSyringe()
    {
        if (syringeCount > 0)
        {
            if (healthSystem != null && healthSystem.currentBlood < healthSystem.maxBlood)
            {
                syringeCount--;
                healthSystem.UseMedicine(healAmount);
                UpdateUI();
            }
        }
    }

    void UpdateUI()
    {
        if (syringeCountText != null)
        {
            // เปลี่ยนเป็นภาษาอังกฤษ และปรับสีเป็นสีแดงเลือด (Bloody Red)
            syringeCountText.text = "SYRINGES: " + syringeCount + " [E]";
            syringeCountText.color = new Color(0.65f, 0f, 0f); // สีแดงเข้มเหมือนเลือด
        }
    }
}
