using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Weapon
{
    public class WeaponBase : MonoBehaviour
    {
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private float shootSpeed = 0.5f;
        [SerializeField] private Transform[] gunBarrelPos;
        [SerializeField] private float bulletHorizontalDeviation;
        [SerializeField] private ParticleSystem[] particles;
        [SerializeField] protected string shootVfx;
        [SerializeField] private string weaponName;
        [SerializeField] public WeaponType weaponType;

        public float ShootSpeed => shootSpeed;
        public string WeaponName => weaponName;
        protected float shootTimer;
        protected bool isFiring = false;
        protected PlayerController player;
        private void Start()
        {
            shootTimer = -0.1f;
        }

        protected virtual void Update()
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
        public virtual BulletBase[] Fire(Transform playerTrans)
        {
            if (shootTimer <= 0f)
            {
                var bulletList = new BulletBase[gunBarrelPos.Length];
                shootTimer = shootSpeed;
                isFiring = true;
                for (int i = 0; i < gunBarrelPos.Length; i++)
                {
                    var randomYaw = Random.Range(-bulletHorizontalDeviation, bulletHorizontalDeviation);
                    //var deviatedDirection = gunBarrelPos[i].right;//direction + new Vector3(0f, randomYaw, 0f);
                    //var deviatedDirection = direction + new Vector3(0f, randomYaw, 0f);
                    //var bulletPos = gunBarrelPos[i].position;
                    var bullet = BulletPool.Instance.GetBullet(weaponType);
                    if (bullet == null)
                        return null;
                    bullet.transform.position = gunBarrelPos[i].position;//playerTrans.position;
                    bullet.transform.rotation = playerTrans.rotation;
                    bullet.speed = bulletSpeed;
                    bullet.gameObject.SetActive(true);
                    bulletList[i] = bullet;
                }

                PlayParticles();
                PlayVfx();

                return bulletList;
            }
            return null;
        }
        public virtual void StopFire(){}

        private void PlayParticles()
        {
            foreach (var particle in particles)
            {
                particle.Play(true);
            }
        }
        protected virtual void PlayVfx()
        {
            SoundManager.Instance.PlaySFX(shootVfx, 0.3f);
        }
        public void SetPlayer(PlayerController player)
        {
            this.player = player;
        }
    }
}
