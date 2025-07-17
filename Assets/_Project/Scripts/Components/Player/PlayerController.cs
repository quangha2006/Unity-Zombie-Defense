using UnityEngine;
using Weapon;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator animator;
    [SerializeField] private float shootSpeed = 0.1f;
    [SerializeField] private GameObject equipweaponsObj;
    [SerializeField] private WeaponBase weaponPrefab;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Transform fixedBulletY;

    public bool gameReady = true;
    private float shootTimer = 0;
    private WeaponBase currentWeapon;
    private float currentHealth;
    void Start()
    {
        currentHealth = maxHealth;
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
        else
        {
            var bullet = currentWeapon.Fire(transform.rotation.eulerAngles, fixedBulletY.position.y);
            if (bullet != null )
            {
                animator.SetBool("IsShooting", true);
                animator.SetFloat("ShootingSpeed", 1 / currentWeapon.ShootSpeed);
            }
            else
            {
                animator.SetBool("IsShooting", false);
            }
        }
    }
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("TakeDamage: currentHealth = " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // TODO: invoke die callback to level manager.
        // TODO: trigger death animation, game over...
    }
}
