using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // เพิ่ม TMPro เพื่อใช้แสดงผล UI

[System.Serializable]
public class EnemyGroup // คลาสสำหรับรวมกลุ่มมอนสเตอร์
{
    public string enemyName;      // ชื่อประเภท (เอาไว้ดูใน Inspector)
    public GameObject enemyPrefab; // มอนสเตอร์ที่จะเกิด
    public int count;             // จำนวนของประเภทนี้
}

[System.Serializable]
public class Wave
{
    public string name;           // ชื่อเวฟ
    public EnemyGroup[] enemies;  // รายการมอนสเตอร์หลายๆ แบบในเวฟนี้
    public float rate;            // ความเร็วในการเกิด (ตัวต่อวินาที)
}

public class wave : MonoBehaviour
{
    public enum SpawnState { SPAWNING, WAITING, COUNTING };

    [Header("Wave Settings")]
    public Wave[] waves;          // รายการเวฟทั้งหมด
    private int nextWave = 0;     // เวฟถัดไป
    public float timeBetweenWaves = 5f; // เวลาระหว่างเวฟ
    private float waveCountdown;

    [Header("UI References")]
    public TextMeshProUGUI waveText;      // ตัวหนังสือบอกเวฟ (เช่น Wave: 1)
    public TextMeshProUGUI countdownText; // ตัวหนังสือบอกเวลานับถอยหลัง

    [Header("Spawn Points")]
    public Transform[] spawnPoints; // จุดเกิดมอนสเตอร์ (ลากมาใส่ได้หลายจุด เช่น 4 จุด)

    private float searchCountdown = 1f;
    private SpawnState state = SpawnState.COUNTING;

    void Start()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points referenced!");
        }

        waveCountdown = timeBetweenWaves;
        UpdateUI();
    }

    void Update()
    {
        if (state == SpawnState.WAITING)
        {
            // ตรวจสอบว่ามอนสเตอร์ตายหมดหรือยัง
            if (!EnemyIsAlive())
            {
                WaveCompleted();
            }
            else
            {
                return;
            }
        }

        if (waveCountdown <= 0)
        {
            if (state != SpawnState.SPAWNING)
            {
                // เริ่มต้นการเกิดของเวฟ
                StartCoroutine(SpawnWave(waves[nextWave]));
            }
        }
        else
        {
            waveCountdown -= Time.deltaTime;
            UpdateUI(); // อัปเดตเวลานับถอยหลังบนหน้าจอ
        }
    }

    void WaveCompleted()
    {
        Debug.Log("Wave Completed!");

        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;

        if (nextWave + 1 > waves.Length - 1)
        {
            nextWave = 0; // วนลูปกลับไปเวฟแรก
            Debug.Log("ALL WAVES COMPLETE! Looping...");
        }
        else
        {
            nextWave++;
        }
        
        UpdateUI();
    }

    bool EnemyIsAlive()
    {
        searchCountdown -= Time.deltaTime;
        if (searchCountdown <= 0f)
        {
            searchCountdown = 1f;
            // ค้นหามอนสเตอร์ที่มี script aienemy และยังเปิดใช้งานอยู่
            aienemy[] enemies = FindObjectsByType<aienemy>(FindObjectsSortMode.None);
            
            // เช็คว่ามีตัวไหนที่ยังไม่ตาย (enabled = true)
            foreach (var enemy in enemies)
            {
                if (enemy.enabled) return true;
            }
            
            return false;
        }
        return true;
    }

    IEnumerator SpawnWave(Wave _wave)
    {
        Debug.Log("Spawning Wave: " + _wave.name);
        state = SpawnState.SPAWNING;

        if (waveText != null) waveText.text = "Wave: " + _wave.name;
        if (countdownText != null) countdownText.text = "Spawning...";

        // วนลูปตามกลุ่มมอนสเตอร์ที่มีในเวฟนี้
        foreach (var group in _wave.enemies)
        {
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(1f / _wave.rate);
            }
        }

        state = SpawnState.WAITING;
        if (countdownText != null) countdownText.text = "Eliminate Enemies!";

        yield break;
    }

    void SpawnEnemy(GameObject _enemy)
    {
        if (spawnPoints.Length == 0 || _enemy == null) return;

        // สุ่มจุดเกิดจากลิสต์ที่มี
        Transform _sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(_enemy, _sp.position, _sp.rotation);
        Debug.Log("Spawning Enemy: " + _enemy.name + " at " + _sp.name);
    }

    void UpdateUI()
    {
        if (waveText != null)
        {
            waveText.text = "Wave: " + (nextWave + 1).ToString();
        }

        if (countdownText != null)
        {
            if (state == SpawnState.COUNTING)
            {
                countdownText.text = "Next Wave in: " + Mathf.Ceil(waveCountdown).ToString() + "s";
            }
        }
    }
}
