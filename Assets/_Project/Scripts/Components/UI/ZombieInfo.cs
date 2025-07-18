using UnityEngine;
using TMPro;

public class ZombieInfo : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TMP_Text zombieCountText;
    [SerializeField] private GameObject zombieContent;

    private void Awake()
    {
        zombieContent.SetActive(false);
    }

    void Start()
    {
        levelManager.OnZombieDeathChanged += OnZombieCountChanged;
        zombieCountText.text = $"{levelManager.totalZombieDeath}/{levelManager.totalZombie}";
        zombieContent.SetActive(true);
    }

    private void OnZombieCountChanged(int death, int max)
    {
        zombieCountText.text = $"{death}/{max}";
    }
}
