using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject monsterPrefab;
    public Transform[] spawnPoints;
    public float spawnInterval = 5f;
    public int maxMonsters = 10;

    [Header("Player Reference")]
    public string playerTag = "Player";
    public float minDistanceToPlayer = 10f; // ไม่เกิดใกล้ผู้เล่นเกินไป

    private float timer;
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null) player = playerObj.transform;
        
        timer = spawnInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (CountActiveMonsters() < maxMonsters)
            {
                SpawnMonster();
            }
            timer = spawnInterval;
        }
    }

    void SpawnMonster()
    {
        if (monsterPrefab == null || spawnPoints.Length == 0) return;

        // เลือกจุดเกิดแบบสุ่ม
        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        // เช็คว่าจุดเกิดใกล้ผู้เล่นเกินไปไหม (ถ้ามีข้อมูลผู้เล่น)
        if (player != null && Vector3.Distance(spawnPoint.position, player.position) < minDistanceToPlayer)
        {
            // ถ้าใกล้เกินไป ลองหาจุดอื่น (ข้ามรอบนี้ไปก่อนเพื่อความง่าย)
            return;
        }

        Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("Monster Spawned at: " + spawnPoint.name);
    }

    int CountActiveMonsters()
    {
        // ค้นหา Object ทั้งหมดที่มี script aienemy ติดอยู่
        return FindObjectsByType<aienemy>(FindObjectsSortMode.None).Length;
    }
}
