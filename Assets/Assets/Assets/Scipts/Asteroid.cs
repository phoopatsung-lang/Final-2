using System.Collections;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab; // ช่องสำหรับใส่ Prefab อุกกาบาต
    public float spawnInterval = 20f; // เวลาหน่วงในการเกิดใหม่ (20 วินาที)
    public float minX = -8f;          // ขอบเขตการสุ่มซ้ายสุด
    public float maxX = 8f;           // ขอบเขตการสุ่มขวาสุด

    void Start()
    {
        // เริ่มทำงานฟังก์ชันวนลูปสร้างอุกกาบาต
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnAsteroid();
            // รอ 20 วินาที ก่อนจะวนกลับไปสร้างลูกใหม่อีกครั้ง
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnAsteroid()
    {
        // สุ่มตำแหน่งแกน X และใช้ความสูง (แกน Y) ของตัว Spawner เอง
        float randomX = Random.Range(minX, maxX);
        Vector2 spawnPosition = new Vector2(randomX, transform.position.y);

        // เสกอุกกาบาตขึ้นมาใหม่
        Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);
    }
}