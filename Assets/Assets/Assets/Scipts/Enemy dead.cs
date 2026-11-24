
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public float spawnInterval = 5f;
    public int maxEnemies = 5;

    public Vector2 spawnPosition;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length >= maxEnemies)
            return;

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
    }
}


