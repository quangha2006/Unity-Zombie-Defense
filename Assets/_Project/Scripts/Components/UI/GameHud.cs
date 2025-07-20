using System.Collections.Generic;
using UnityEngine;

public class GameHud : MonoBehaviour
{
    [SerializeField] private PlayerHealthBar playerHealthBar;
    [SerializeField] private ZombieHealthBar zombieHealthBarPrefab;
    [SerializeField] private Transform zombieHealthBarParent;
    [SerializeField] private LevelManager levelManager;

    private Queue<ZombieHealthBar> zombieHealthBarsPool = new Queue<ZombieHealthBar>();

    private void Awake()
    {
        levelManager.OnZombieSpawned += RegisterZombie;
        levelManager.OnPlayerSpawned += RegisterPlayer;
    }
    public void RegisterZombie(ZombieController zombie)
    {
        ZombieHealthBar zombieHealth;
        if (zombieHealthBarsPool.Count > 0)
        {
            zombieHealth = zombieHealthBarsPool.Dequeue();
        }
        else
        {
            zombieHealth = Instantiate(zombieHealthBarPrefab, zombieHealthBarParent);
            zombieHealth.onHealthBarDetached += OnHealthBarDetached;
        }
        zombieHealth.AssignZombie(zombie);
        zombieHealth.gameObject.SetActive(true);
    }

    public void RegisterPlayer(PlayerController player)
    {
        playerHealthBar.AssignPlayer(player);
    }

    private void OnHealthBarDetached(ZombieHealthBar healthBar)
    {
        healthBar.gameObject.SetActive(false);
        zombieHealthBarsPool.Enqueue(healthBar);
    }
    private void OnDestroy()
    {
        levelManager.OnZombieSpawned -= RegisterZombie;
        levelManager.OnPlayerSpawned -= RegisterPlayer;
    }
}
