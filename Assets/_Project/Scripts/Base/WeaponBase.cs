using UnityEngine;

namespace Weapon
{
    public class WeaponBase : MonoBehaviour
    {
        [SerializeField] private int numBullet;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private float shootSpeed = 0.5f;
        [SerializeField] private Transform gunBarrelPos;
        [SerializeField] private float bulletHorizontalDeviation;
        [SerializeField] private ParticleSystem[] particles;
        [SerializeField] protected string shootVfx;
        [SerializeField] private string weaponName;
        [SerializeField] private WeaponType weaponType;

        public float ShootSpeed => shootSpeed;
        public string WeaponName => weaponName;
        protected float shootTimer;
        protected bool isFiring = false;
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
            if (shootTimer <= 0f)
            {
                isFiring = false;
            }
        }
        public virtual BulletBase Fire(Vector3 direction, float fixedy)
        {
            if (shootTimer <= 0f)
            {
                shootTimer = shootSpeed;
                var randomYaw = Random.Range(-bulletHorizontalDeviation, bulletHorizontalDeviation);
                var deviatedDirection = direction + new Vector3(0f, randomYaw, 0f);
                var bulletPos = gunBarrelPos.position;
                var bullet = BulletPool.Instance.GetBullet();
                bullet.transform.position = bulletPos;
                bullet.transform.rotation = Quaternion.Euler(deviatedDirection);
                bullet.speed = bulletSpeed;
                bullet.ResetBullet();
                bullet.gameObject.SetActive(true);
                PlayParticles();
                PlayVfx();
                isFiring = true;
                return bullet;
            }
            return null;
        }
        private void PlayParticles()
        {
            foreach (var particle in particles)
            {
                particle.Play(true);
            }
        }
        protected virtual void PlayVfx()
        {
            SoundManager.Instance.PlaySFX(shootVfx);
        }
    }
}
