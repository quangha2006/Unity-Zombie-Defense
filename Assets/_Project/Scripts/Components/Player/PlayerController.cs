using System;
using UnityEngine;
using Weapon;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator animator;
    [SerializeField] private float shootSpeed = 0.1f;
    [SerializeField] private GameObject equipweaponsObj;
    [SerializeField] private WeaponBase weaponPrefab;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Transform fixedBulletY;
    [SerializeField] private ParticleSystem takeDamageParticle;

    public bool gameReady = true;
    private float shootTimer = 0;
    private WeaponBase currentWeapon;
    private int currentHealth;

    public int health => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action<int, int> onHealthChanged;
    public event Action onDeath;

    public bool isDeath { get; private set; }
    void Awake()
    {
        isDeath = false;
        currentHealth = maxHealth;
    }
    void Start()
    {
        shootTimer = shootSpeed;
        currentWeapon = Instantiate(weaponPrefab, equipweaponsObj.transform);
    }

    void Update()
    {
        if (!gameReady || currentWeapon == null)
            return;

        if (!InputManager.Instance.GetShootButton())
        {
            animator.SetBool("IsShooting", false);
        }
        else if (!isDeath)
        {
            var bullet = currentWeapon.Fire(transform.rotation.eulerAngles, fixedBulletY.position.y);
            if (bullet != null )
            {
                animator.SetBool("IsShooting", true);
                animator.SetFloat("ShootingSpeed", 1 / currentWeapon.ShootSpeed);
                return;
            }
        }
        animator.SetBool("IsShooting", false);
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("TakeDamage: currentHealth = " + currentHealth);
        if (!isDeath)
            takeDamageParticle.Play();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDeath = true;
        onDeath?.Invoke();
        // TODO: invoke die callback to level manager.
        // TODO: trigger death animation, game over...
    }
}
