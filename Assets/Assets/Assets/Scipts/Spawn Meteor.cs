using UnityEngine;
using System.Collections;

public class MeteorSpawner : MonoBehaviour
{
    public GameObject meteorPrefab;

    public int maxMeteor = 10;
    public float spawnInterval = 2f;
    public float spawnRadius = 10f;

    private int currentMeteor = 0;

    void Start()
    {
        StartCoroutine(SpawnMeteor());
    }

    IEnumerator SpawnMeteor()
    {
        while (true)
        {
            if (currentMeteor < maxMeteor)
            {
                Vector2 randomPos =
                    Random.insideUnitCircle * spawnRadius;

                GameObject meteor = Instantiate(
                    meteorPrefab,
                    transform.position + (Vector3)randomPos,
                    Quaternion.identity
                );

                meteor.transform.localScale =
                    new Vector3(3f, 3f, 1f);

                currentMeteor++;

                StartCoroutine(CountMeteor(meteor));
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator CountMeteor(GameObject meteor)
    {
        while (meteor != null)
        {
            yield return null;
        }

        currentMeteor--;
    }
}


