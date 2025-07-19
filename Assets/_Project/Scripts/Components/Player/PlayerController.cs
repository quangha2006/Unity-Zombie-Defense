using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Weapon;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator animator;
    [SerializeField] private float shootSpeed = 0.1f;
    [SerializeField] private GameObject equipweaponsObj;
    [SerializeField] private GameObject grenadePos;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Transform fixedBulletY;
    [SerializeField] private ParticleSystem takeDamageParticle;
    [SerializeField] private bool cheat = false;
    public bool gameReady = true;
    private float shootTimer = 0;
    private WeaponBase currentWeapon;
    private int currentHealth;

    public int health => currentHealth;
    public int MaxHealth => maxHealth;
    public Vector3 GrenadePos => grenadePos.transform.position;
    public event Action<int, int> onHealthChanged;
    public event Action onDeath;
    public event Action<string> OnWeaponChanged;

    private List<WeaponBase> weaponList;
    private Dictionary<string, WeaponBase> weaponLoaded;

    public bool isDeath { get; private set; }
    public string currentWeaponName => currentWeapon?.WeaponName ?? null;
    public LevelManager levelManager;
    private LevelManager.MatchState currentMatchState = LevelManager.MatchState.None;
    private bool wasPressedFireLastFrame = false;

    void Awake()
    {
        isDeath = false;
        currentHealth = maxHealth;
        weaponLoaded = new Dictionary<string, WeaponBase>();
    }

    void Start()
    {
        shootTimer = shootSpeed;
        if (levelManager != null)
        {
            levelManager.OnMatchStateChanged += OnMatchStateChanged;
        }
    }

    void Update()
    {
        if (currentWeapon == null && weaponList != null && weaponList.Count > 0)
        {
            LoadWeapon(weaponList.First().WeaponName);
        }

        if (!gameReady || currentWeapon == null)
        {
            return;
        }

        if (currentMatchState > LevelManager.MatchState.Playing)
        {
            if(wasPressedFireLastFrame)
            {
                wasPressedFireLastFrame = false;
                currentWeapon.StopFire();
            }
            return;
        }

        var isFirePressed = InputManager.Instance.GetShootButton();

        if (!isFirePressed && wasPressedFireLastFrame)
        {
            currentWeapon.StopFire();
        }

        if (!isDeath && isFirePressed)
        {
            wasPressedFireLastFrame = true;
            var bullet = currentWeapon.Fire(transform);
            if (bullet != null && bullet.Length > 0)
            {
                animator.SetBool("IsShooting", true);
                animator.SetFloat("ShootingSpeed", 1 / currentWeapon.ShootSpeed);
                return;
            }
        }
        else
        {
            wasPressedFireLastFrame = false;
        }

        animator.SetBool("IsShooting", false);
    }
    public void TakeDamage(int amount)
    {
#if UNITY_EDITOR
        if (!cheat)
#endif
            if (!isDeath)
            {
                Debug.Log("Player take damage: " + amount);
                currentHealth -= amount;
                onHealthChanged?.Invoke(currentHealth, maxHealth);

                takeDamageParticle.Play();

                if (currentHealth <= 0)
                {
                    Die();
                }
            }
    }

    void Die()
    {
        isDeath = true;
        onDeath?.Invoke();
        animator.SetLayerWeight(1, 0);
        animator.SetTrigger("IsDeath");
        // TODO: invoke die callback to level manager.
    }

    public void LoadWeapon(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("LoadWeapon Without name");
        }

        var weaponPrefab = weaponList.Where(i => i.WeaponName == name).FirstOrDefault();
        if (weaponPrefab != null)
        {
            if (currentWeapon != null)
            {
                currentWeapon.gameObject.SetActive(false);
            }
            if (!weaponLoaded.ContainsKey(name))
            {
                weaponLoaded[name] = Instantiate(weaponPrefab, equipweaponsObj.transform);
                weaponLoaded[name].SetPlayer(this);
            }
            currentWeapon = weaponLoaded[name];
            currentWeapon.gameObject.SetActive(true);
            OnWeaponChanged?.Invoke(currentWeapon.WeaponName);
        }
        else
        {
            Debug.LogError("Not found weapon name = " + name);
        }
    }

    public void SetWeapons(List<WeaponBase> weaponlist)
    {
        weaponList = weaponlist;
    }
    public List<string> GetWeaponListName()
    {
        if (weaponList == null)
        {
            return null;
        }
        return weaponList.Select(i => i.WeaponName).ToList();
    }
    private void OnMatchStateChanged(LevelManager.MatchState matchType)
    {
        currentMatchState = matchType;
    }
}
