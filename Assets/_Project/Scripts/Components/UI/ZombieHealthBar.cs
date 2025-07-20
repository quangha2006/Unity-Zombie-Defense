using System;
using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    
    private ZombieController currentZombie;
    public event Action<ZombieHealthBar> onHealthBarDetached;
    private float heightOffset = 2f;
    private Camera cam;
    private RectTransform rectTransform;
    private bool shouldShow = true;
    void Start()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
        if (currentZombie == null)
            healthBar.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (currentZombie == null)
            return;
        var worldPos = currentZombie.transform.position;
        worldPos.y += heightOffset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        rectTransform.position = screenPos;

        healthBar.gameObject.SetActive(screenPos.z > 0 && shouldShow);
    }
    public void AssignZombie(ZombieController zombie)
    {
        currentZombie = zombie;
        zombie.onHealthChanged += ZombieHealthChanged;
        zombie.onDie += ZombieDie;
        ZombieHealthChanged(zombie.health, zombie.MaxHealth);
    }
    private void ZombieHealthChanged(int current, int max)
    {
        var healthPercent = current / (float)max;
        shouldShow = current < max;
        healthBar.value = healthPercent;
        healthBar.gameObject.SetActive(!shouldShow);
    }
    private void ZombieDie()
    {
        currentZombie.onHealthChanged -= ZombieHealthChanged;
        currentZombie.onDie -= ZombieDie;
        currentZombie = null;
        healthBar.gameObject.SetActive(false);
        onHealthBarDetached?.Invoke(this);
    }
}
