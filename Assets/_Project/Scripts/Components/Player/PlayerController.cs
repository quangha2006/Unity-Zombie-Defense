using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using Weapon;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator animator;
    [SerializeField] private float shootSpeed = 0.1f;
    [SerializeField] private Transform equipGunsObj;
    [SerializeField] private Transform equipGrenadeObj;
    [SerializeField] private GameObject grenadePos;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Transform fixedBulletY;
    [SerializeField] private ParticleSystem takeDamageParticle;
    [SerializeField] private bool cheat = false;
    [SerializeField] private float shootAngleTolerance = 10f;
    public bool gameReady = true;
    private WeaponBase currentWeapon;
    private int currentHealth;

    public int health => currentHealth;
    public int MaxHealth => maxHealth;
    public Vector3 GrenadePos => grenadePos.transform.position;
    public event Action<int, int> onHealthChanged;
    public event Action onDeath;
    public event Action<WeaponBase> onWeaponChanged;

    private List<WeaponBase> weaponList;
    private Dictionary<string, WeaponBase> weaponLoaded;

    public bool isDeath { get; private set; }
    public string currentWeaponName => currentWeapon?.WeaponName ?? null;
    private bool isThrowingGrenade;
    public bool forceKeepRotation => isThrowingGrenade;
    public LevelManager levelManager;
    private LevelManager.MatchState currentMatchState = LevelManager.MatchState.None;
    private bool wasPressedFireLastFrame = false;
    private Coroutine currentAnimHasGunLerp;
    private float lerpHasGunDuration = 0.25f;
    public bool canMove => gameReady && !isDeath && currentMatchState == LevelManager.MatchState.Playing;

    private const string AnimParmName_HasGun = "HasGun";
    void Awake()
    {
        isDeath = false;
        isThrowingGrenade = false;
        currentHealth = maxHealth;
        weaponLoaded = new Dictionary<string, WeaponBase>();
        onWeaponChanged += OnWeaponChanged;
    }

    void Start()
    {
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
                animator.SetBool("IsShooting", false);
            }
            return;
        }

        var isFirePressed = InputManager.Instance.GetShootButton();
        var shootInput = InputManager.Instance.GetShootDirection();
        var shootInputDirection = new Vector3(shootInput.x, 0, shootInput.y).normalized;
        Vector3 currentDir = transform.forward;
        float angleCharacterAndJoystick = Vector3.Angle(currentDir, shootInputDirection);

        if (!isFirePressed && wasPressedFireLastFrame)
        {
            var bullet = currentWeapon.StopFire();
            if (bullet is BulletGrenade)
            {
                StartCoroutine(ThrowGrenade());
            }
        }

        if (!isDeath && isFirePressed)
        {
            if (wasPressedFireLastFrame  || angleCharacterAndJoystick <= shootAngleTolerance)
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
        animator.SetBool("IsShooting", false);
        // TODO: invoke die callback to level manager.
    }

    public void LoadWeapon(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("LoadWeapon Without name");
        }
        if (isThrowingGrenade || (currentWeapon != null && currentWeapon.WeaponName == name))
        {
            return;
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
                Transform parent = equipGunsObj;
                if (weaponPrefab.weaponType == WeaponType.GRENADE)
                {
                    parent = equipGrenadeObj;
                }
                weaponLoaded[name] = Instantiate(weaponPrefab, parent);
                weaponLoaded[name].SetPlayer(this);
            }
            currentWeapon = weaponLoaded[name];
            currentWeapon.gameObject.SetActive(true);
            onWeaponChanged?.Invoke(currentWeapon);
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
    private void OnWeaponChanged(WeaponBase weapon)
    {
        Debug.Log("OnWeaponChanged: " + weapon.weaponType);
        switch(weapon.weaponType)
        {
            case WeaponType.GRENADE:
                StartAnimHasGunLerpTo(0f);
                break;
            case WeaponType.SMG:
            case WeaponType.SHOTGUN:
                StartAnimHasGunLerpTo(1f);
                break;
        }
    }
    public void StartAnimHasGunLerpTo(float targetValue)
    {
        if (currentAnimHasGunLerp != null)
        {
            StopCoroutine(currentAnimHasGunLerp);
        }

        currentAnimHasGunLerp = StartCoroutine(LerpValueCoroutine(targetValue));
    }
    private IEnumerator LerpValueCoroutine(float targetValue)
    {
        Debug.Log("LerpValueCoroutine Begin: " + targetValue);
        float animationValue = animator.GetFloat(AnimParmName_HasGun);
        float startValue = animationValue;
        float timeElapsed = 0f;

        while (timeElapsed < lerpHasGunDuration)
        {
            animationValue = Mathf.Lerp(startValue, targetValue, timeElapsed / lerpHasGunDuration);

            animator.SetFloat(AnimParmName_HasGun, animationValue);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        animator.SetFloat(AnimParmName_HasGun, targetValue);
        Debug.Log("LerpValueCoroutine END: " + animationValue);
        currentAnimHasGunLerp = null;
    }
    private IEnumerator ThrowGrenade()
    {
        isThrowingGrenade = true;
        animator.SetTrigger("Throw");
        yield return new WaitForSeconds(0.6f);
        (currentWeapon as WeaponGrenade).ThrowGrenade();
        yield return new WaitForSeconds(0.5f);
        isThrowingGrenade = false;
    }
}
