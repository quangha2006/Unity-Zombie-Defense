using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private int matchTime = 180;
    [SerializeField] private ZombieController[] zombiePrefabs;
    [SerializeField] private Transform[] zombieSpawnPoints;
    [SerializeField] private float zombieSpawnRate;
    [SerializeField] private List<ZombieController> zombieSpawned;
    [SerializeField] private PlayerController player;

    private List<ZombieController> zombiePolling = new List<ZombieController>();
    
    private MatchState matchState = MatchState.None;
    private int navMeshAgentPriority = 50;
    private float spawnTimer = 0f;
    private float matchPrepareTime = 3f;
    void Start()
    {
        foreach (var zombie in zombieSpawned)
        {
            zombie.playerTarget = player.transform;
            zombie.NavMeshAgent.avoidancePriority = navMeshAgentPriority++;
        }
        matchState = MatchState.Preparing;
    }

    void Update()
    {
        switch (matchState)
        {
            case MatchState.None:
                break;
            case MatchState.Preparing:
                matchPrepareTime -= Time.deltaTime;
                if (matchPrepareTime <= 0f)
                    matchState = MatchState.Playing;
                break;
            case MatchState.Playing:
                SpawnNewZombie();
                break;
            case MatchState.TimeUp:
                break;
            case MatchState.End:
                break;
        }
    }
    private void SpawnNewZombie()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            spawnTimer = zombieSpawnRate;

            if (zombiePrefabs.Length == 0)
            {
                Debug.LogWarning("Zombie prefab list is empty!");
                return;
            }

            int prefabIndex = Random.Range(0, zombiePrefabs.Length);
            var prefab = zombiePrefabs[prefabIndex];

            // Random spawn point
            if (zombieSpawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points assigned!");
                return;
            }

            int pointIndex = Random.Range(0, zombieSpawnPoints.Length);
            Transform spawnPoint = zombieSpawnPoints[pointIndex];

            // Instantiate
            var newZombie = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            newZombie.playerTarget = player.transform;
            newZombie.NavMeshAgent.avoidancePriority = navMeshAgentPriority++;
            newZombie.gameReady = true;
            zombieSpawned.Add(newZombie);

            Debug.Log($"Spawned {prefab.name} at {spawnPoint.name}");
        }
    }


    private enum MatchState
    {
        None,
        Preparing,
        Playing,
        TimeUp,
        End
    }
}
