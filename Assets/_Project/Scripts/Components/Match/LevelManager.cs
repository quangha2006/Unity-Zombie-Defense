using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using Weapon;

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
    [SerializeField] private List<WeaponBase> weaponList;
    [SerializeField] private string backgroundMusic;
    [SerializeField] private string zombieChaseSfx;
    [SerializeField] private float timerChaseSfxMin;
    [SerializeField] private float timerChaseSfxMax;

    private List<ZombieController> zombiePolling = new List<ZombieController>();
    
    private MatchState matchState = MatchState.None;
    private MatchState lastMatchState = MatchState.None;
    private int navMeshAgentPriority = 50;
    private float spawnTimer = 0f;
    private float matchPrepareTime = 3f;
    private float loadingTimer = 0.7f;
    private float zombieChaseSfxTimer;
    private PlayerController mainPlayer;
    public int totalZombie          { get; private set; }
    public int totalZombieSpawned   { get; private set; }
    public int totalZombieDeath     { get; private set; }

    public event Action<PlayerController> OnPlayerSpawned;
    public event Action<ZombieController> OnZombieSpawned;
    public event Action<MatchState> OnMatchStateChanged;
    public event Action<int, int> OnZombieDeathChanged;

    private void Awake()
    {
        UIManager.Instance.UpdateLoadingBar(0.1f);
        totalZombie = zombieSpawned.Count + (int)(matchTime / zombieSpawnRate);
        totalZombieSpawned = zombieSpawned.Count;
        totalZombieDeath = 0;
    }
    private void Start()
    {
        UIManager.Instance.UpdateLoadingBar(0.11f);
        matchState++;
        if (!string.IsNullOrEmpty(backgroundMusic))
        {
            SoundManager.Instance.PlayBackgroundMusic(backgroundMusic);
        }
        zombieChaseSfxTimer = UnityEngine.Random.Range(timerChaseSfxMin, timerChaseSfxMax);
    }
    void Update()
    {
        switch (matchState)
        {
            case MatchState.None:
                
                break;
            case MatchState.SpawnPlayer:
                UIManager.Instance.UpdateLoadingBar(0.2f);
                SpawnPlayer();
                break;
            case MatchState.InitZombie:
                UIManager.Instance.UpdateLoadingBar(0.3f);
                InitZombies();
                break;
            case MatchState.LevelInitComplete:

                UIManager.Instance.UpdateLoadingBar(0.3f + 0.7f - loadingTimer);
                loadingTimer -= Time.deltaTime;
                if (loadingTimer <= 0f)
                {
                    //UIManager.Instance.SetActiveLoadingScreen(false);
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
                if (totalZombieSpawned < totalZombie)
                {
                    SpawnNewZombie();
                }
                zombieChaseSfxTimer -= Time.deltaTime;
                if (zombieChaseSfxTimer <= 0f)
                {
                    zombieChaseSfxTimer = UnityEngine.Random.Range(timerChaseSfxMin, timerChaseSfxMax);
                    SoundManager.Instance.PlaySFX(zombieChaseSfx);
                }
                break;
            case MatchState.PlayerDeath:
                UIManager.Instance.SetActiveOnScreenJoyStick(false);
                matchState++;
                break;
            case MatchState.Lose:
                //TODO: Show lose popup
                matchState = MatchState.End;
                break;
            case MatchState.Win:
                UIManager.Instance.SetActiveOnScreenJoyStick(false);
                //TODO: Show win popup
                matchState = MatchState.End;
                break;
            case MatchState.End:
                SoundManager.Instance.StopBackgroundMusic();
                break;
        }
        if (matchState != lastMatchState)
        {
            lastMatchState = matchState;
            OnMatchStateChanged?.Invoke(matchState);
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
            newZombie.onDie += OnZombieDeath;
            zombieSpawned.Add(newZombie);
            totalZombieSpawned++;
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
            mainPlayer.SetWeapons(weaponList);
            mainPlayer.levelManager = this;
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
            zombie.onDie += OnZombieDeath;
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
        matchState = MatchState.PlayerDeath;
    }
    private void OnZombieDeath()
    {
        totalZombieDeath++;
        OnZombieDeathChanged?.Invoke(totalZombieDeath, totalZombie);
        if (totalZombieDeath >= totalZombie)
        {
            matchState = MatchState.Win;
        }
    }
    public enum MatchState
    {
        None = 0,
        SpawnPlayer = 1,
        InitZombie = 2,
        LevelInitComplete = 3,
        PlayCutScene = 4,
        Playing = 5,
        PlayerDeath = 6,
        Win = 7,
        Lose = 8,
        End
    }
}
