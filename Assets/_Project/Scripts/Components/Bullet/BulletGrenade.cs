using UnityEngine;

public class BulletGrenade : BulletBase
{
    [SerializeField] private float grenadeTimer = 1.6f;
    [SerializeField] private int damageToPlayer;
    [SerializeField] private Rigidbody grenadeRigidbody;
    [SerializeField] private string sfxExplosive;
    [SerializeField] private string sfxRolling;

    private float timer;
    protected override void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            Explode();
            ReturnToPool();
        }
    }
    private void Explode()
    {
        ParticlePool.Instance.PlayFX(ParticlePool.ParticleType.GrenadeExplosive, transform.position, Quaternion.identity);
        SoundManager.Instance.PlaySFX(sfxExplosive);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, bulletRadius, hitMask);

        foreach (var hitCol in hitColliders)
        {

            IDamageable damageable = hitCol.GetComponent<IDamageable>();
            if (damageable != null)
            {
                var applyDamage = damage;
                if (hitCol.CompareTag("Player"))
                {
                    applyDamage = damageToPlayer;
                }
                damageable.TakeDamage(applyDamage);
            }
        }
    }
    public void ApplyVelocity(Vector3 velocity)
    {
        grenadeRigidbody.linearVelocity = velocity;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Terrain"))
        {
            SoundManager.Instance.PlaySFX(sfxRolling);
        }
    }
    private void OnEnable()
    {
        timer = grenadeTimer;
    }
}
