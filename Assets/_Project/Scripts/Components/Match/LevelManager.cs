using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private int matchTime = 180;
    [SerializeField] private ZombieController[] zombiePrefabs;
    [SerializeField] private Transform[] zombieSpawnPoints;
    [SerializeField] private float zombieSpawnRate;
    [SerializeField] private List<ZombieController> zombieSpawned;
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private GameHud gameHud;
    [SerializeField] private CinemachineCamera virtualCamera;

    private List<ZombieController> zombiePolling = new List<ZombieController>();
    
    private MatchState matchState = MatchState.None;
    private int navMeshAgentPriority = 50;
    private float spawnTimer = 0f;
    private float matchPrepareTime = 3f;
    private float loadingTimer = 0.5f;
    private PlayerController mainPlayer;

    public event Action<PlayerController> OnPlayerSpawned;
    public event Action<ZombieController> OnZombieSpawned;
    public event Action<MatchState> OnMatchStateChanged;
    private void Start()
    {
        matchState++;
    }
    void Update()
    {
        switch (matchState)
        {
            case MatchState.None:
            case MatchState.SpawnPlayer:
                SpawnPlayer();
                break;
            case MatchState.InitZombie:
                InitZombies();
                break;
            case MatchState.LevelInitComplete:
                UIManager.Instance.UpdateLoadingBar(1f);
                loadingTimer -= Time.deltaTime;
                if (loadingTimer <= 0f)
                {
                    UIManager.Instance.SetActiveLoadingScreen(false);
                    UIManager.Instance.SetActiveOnScreenJoyStick(true);
                    matchState++;
                }
                break;
            case MatchState.PlayCutScene:
                matchPrepareTime -= Time.deltaTime;
                if (matchPrepareTime <= 0f)
                {
                    matchState++;
                    ActiveZombieAndPlayer();
                }
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

            int prefabIndex = UnityEngine.Random.Range(0, zombiePrefabs.Length);
            var prefab = zombiePrefabs[prefabIndex];

            // Random spawn point
            if (zombieSpawnPoints.Length == 0)
            {
                Debug.LogWarning("No spawn points assigned!");
                return;
            }

            int pointIndex = UnityEngine.Random.Range(0, zombieSpawnPoints.Length);
            Transform spawnPoint = zombieSpawnPoints[pointIndex];

            // Instantiate
            var newZombie = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

            newZombie.playerTarget = mainPlayer;
            newZombie.NavMeshAgent.avoidancePriority = navMeshAgentPriority++;
            newZombie.gameReady = true;
            zombieSpawned.Add(newZombie);
            OnZombieSpawned?.Invoke(newZombie);
        }
    }
    private void SpawnPlayer()
    {
        mainPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        if (mainPlayer != null)
        {
            OnPlayerSpawned?.Invoke(mainPlayer);
            virtualCamera.Follow = mainPlayer.transform;
            virtualCamera.LookAt = mainPlayer.transform;
            mainPlayer.onDeath += OnPlayerDeath;
            matchState++;
        }
        else
        {
            //TODO: Show popup error
        }
    }
    private void InitZombies()
    {
        foreach (var zombie in zombieSpawned)
        {
            zombie.playerTarget = mainPlayer;
            zombie.NavMeshAgent.avoidancePriority = navMeshAgentPriority++;
            OnZombieSpawned?.Invoke(zombie);
        }
        matchState++;
    }
    private void ActiveZombieAndPlayer()
    {
        mainPlayer.gameReady = true;
        foreach (var zombie in zombieSpawned)
        {
            zombie.gameReady = true;
        }
    }
    private void OnPlayerDeath()
    {
        matchState = MatchState.End;
    }
    public enum MatchState
    {
        None = 0,
        SpawnPlayer = 1,
        InitZombie = 2,
        LevelInitComplete = 3,
        PlayCutScene = 4,
        Playing = 5,
        TimeUp = 6,
        End
    }
}
