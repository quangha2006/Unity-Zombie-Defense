using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviourSingleton<BulletPool>
{
    [SerializeField] private BulletBase bulletSMGPrefab;
    [SerializeField] private BulletBase bulletShotGunPrefab;
    [SerializeField] private BulletBase grenadePrefab;
    [SerializeField] private int initQueue = 10;

    private Dictionary<WeaponType, Queue<BulletBase>> bulletDic = new Dictionary<WeaponType, Queue<BulletBase>>();

    protected override void Awake()
    {
        base.Awake();
        bulletDic[WeaponType.SMG] = new Queue<BulletBase>();
        bulletDic[WeaponType.SHOTGUN] = new Queue<BulletBase>();

        for (int i = 0; i < initQueue; i++)
        {
            BulletBase bulletSMG = Instantiate(bulletSMGPrefab);
            bulletSMG.gameObject.SetActive(false);
            bulletDic[WeaponType.SMG].Enqueue(bulletSMG);

            BulletBase bulletShotGun = Instantiate(bulletShotGunPrefab);
            bulletShotGun.gameObject.SetActive(false);
            bulletDic[WeaponType.SHOTGUN].Enqueue(bulletShotGun);
        }
        //Specific for grenade
        bulletDic[WeaponType.GRENADE] = new Queue<BulletBase>();
        BulletBase grenade = Instantiate(grenadePrefab);
        grenade.gameObject.SetActive(false);
        bulletDic[WeaponType.GRENADE].Enqueue(grenade);
    }

    public BulletBase GetBullet(WeaponType weaponType)
    {
        BulletBase bullet = null;
        if (bulletDic.ContainsKey(weaponType))
        {
            var queuePool = bulletDic[weaponType];
            if (queuePool.Count > 0)
            {
                bullet = queuePool.Dequeue();
            }
            else
            {
                switch (weaponType)
                {
                    case WeaponType.GRENADE:
                        bullet = Instantiate(grenadePrefab);
                        break;
                    case WeaponType.SMG:
                        bullet = Instantiate(bulletSMGPrefab);
                        break;
                    case WeaponType.SHOTGUN:
                        bullet = Instantiate(bulletShotGunPrefab);
                        break;
                };
            }
            return bullet;
        }
        Debug.LogError("Not found bullet for " + weaponType);
        return null;
    }
    public void ReturnBullet(BulletBase bullet)
    {
        bullet.gameObject.SetActive(false);
        if (bulletDic.ContainsKey(bullet.weaponType))
        {
            var queue = bulletDic[bullet.weaponType];
            queue.Enqueue(bullet);
        }
        else
        {
            Debug.LogWarning("Cannot pool bullet type: " + bullet.weaponType);
        }
    }
}
