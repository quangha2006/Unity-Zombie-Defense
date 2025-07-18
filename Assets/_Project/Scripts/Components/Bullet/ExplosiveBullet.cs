using UnityEngine;

public class ExplosiveBullet : BulletBase
{
    [SerializeField] private int explosiveDamage;
    [SerializeField] public float explosionRadius = 1f;
    [SerializeField] private LayerMask affectLayer;
    protected override bool BulletUpdate(out RaycastHit hit)
    {
        var isReturnPool = base.BulletUpdate(out hit);
        if (hit.collider != null)
        {
            ParticlePool.Instance.PlayFX(ParticlePool.ParticleType.BulletShotgunExplosive, hit.point, Quaternion.identity);

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, affectLayer);

            foreach (var hitCol in hitColliders)
            {
                IDamageable damageable = hitCol.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(explosiveDamage);
                }
            }
        }
        return isReturnPool;
    }

}
