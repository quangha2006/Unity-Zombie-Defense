using UnityEngine;

namespace Weapon
{
    public class WeaponBase : MonoBehaviour
    {
        [SerializeField] private int numBullet;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private float shootSpeed = 0.5f;
        [SerializeField] private BulletBase bulletPrefab;
        [SerializeField] private Transform gunBarrelPos;
        [SerializeField] private float bulletHorizontalDeviation;
        [SerializeField] private ParticleSystem particle;

        public float ShootSpeed => shootSpeed;
        private float shootTimer;
        private void Start()
        {
            shootTimer = -0.1f;
        }

        private void Update()
        {
            if (shootTimer > 0f)
            {
                shootTimer -= Time.deltaTime;
            }
        }
        public virtual BulletBase Fire(Vector3 direction, float fixedy)
        {
            if (shootTimer <= 0f)
            {
                shootTimer = shootSpeed;
                var randomYaw = Random.Range(-bulletHorizontalDeviation, bulletHorizontalDeviation);
                var deviatedDirection = direction + new Vector3(0f, randomYaw, 0f);
                var bulletPos = new Vector3(gunBarrelPos.position.x, fixedy, gunBarrelPos.position.z);
                var bullet = Instantiate(bulletPrefab, bulletPos, Quaternion.Euler(deviatedDirection));
                bullet.speed = bulletSpeed;
                return bullet;
            }
            return null;
        }
    }
}
