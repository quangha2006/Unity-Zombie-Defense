using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviourSingleton<BulletPool>
{
    [SerializeField] private BulletBase bulletPrefab;

    private Queue<BulletBase> bulletPool = new Queue<BulletBase>();

    protected override void Awake()
    {
        base.Awake();
        BulletBase bullet = Instantiate(bulletPrefab);
        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
    }

    public BulletBase GetBullet()
    {
        BulletBase bullet;

        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            bullet = Instantiate(bulletPrefab);
        }
        return bullet;
    }
    public void ReturnBullet(BulletBase bullet)
    {
        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}
