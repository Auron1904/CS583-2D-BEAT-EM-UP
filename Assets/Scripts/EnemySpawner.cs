using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyGrunt gruntPrefab;
    [SerializeField] private Transform rightSpawnPoint;
    [SerializeField] private Transform leftSpawnPoint;
    [SerializeField] private bool spawnFromBothSides = false;
    [SerializeField] private int gruntCount = 5;
    [SerializeField] private float firstSpawnDelay = 1f;
    [SerializeField] private float timeBetweenSpawns = 0.8f;

    void Start()
    {
        // Tell the level how many are coming BEFORE any spawn.
        if (LevelManager.Instance != null) LevelManager.Instance.ExpectEnemies(gruntCount);
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        for (int i = 0; i < gruntCount; i++)
        {
            // Both sides: alternate right, left, right, left...
            bool useLeft = spawnFromBothSides && i % 2 == 1;
            Transform point = useLeft ? leftSpawnPoint : rightSpawnPoint;

            EnemyGrunt grunt = Instantiate(gruntPrefab, point.position, Quaternion.identity);
            if (LevelManager.Instance != null)
                LevelManager.Instance.RegisterEnemy(grunt.GetComponent<Health>());

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }
}