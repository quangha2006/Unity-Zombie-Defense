using UnityEngine;
using static ParticlePool;
using static UnityEngine.UI.Image;

public class BulletBase : MonoBehaviour
{
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private int damage = 10;
    [SerializeField] public LayerMask hitMask;
    [SerializeField] public WeaponType weaponType;
    [SerializeField] public float bulletRadius;
    [HideInInspector] public float speed = 0f;

    private Vector3 lastPosition;
    private float traveledDistance = 0f;

    void Start()
    {
        ResetBullet();
    }

    protected virtual void Update()
    {
        var isReturnPool = BulletUpdate(out RaycastHit hit);
        if (isReturnPool)
        {
            ReturnToPool();
        }
    }

    protected virtual bool BulletUpdate(out RaycastHit hit)
    {
        if (speed <= 0.01f)
        {
            hit = default(RaycastHit);
            return false;
        }

        float distanceThisFrame = speed * Time.deltaTime;
        Vector3 currentPosition = transform.position + transform.forward * distanceThisFrame;
        //RaycastHit hit;
        //Debug.DrawLine(lastPosition, currentPosition, Color.red, 100.0f);
        Ray ray = new Ray(lastPosition, (currentPosition - lastPosition).normalized);
        if (Physics.SphereCast(ray, bulletRadius, out hit, distanceThisFrame, hitMask))
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
                return true;
            }
            else if (hit.collider.CompareTag("Obstacle"))
            {
                Vector3 bulletDir = (hit.point - lastPosition).normalized;
                Quaternion particleRotation = Quaternion.LookRotation(-bulletDir);
                ParticlePool.Instance.PlayFX(ParticleType.HitWall, hit.point, particleRotation);
                return true;
            }
        }

        transform.position = currentPosition;
        lastPosition = currentPosition;

        traveledDistance += distanceThisFrame;
        if (traveledDistance >= maxDistance)
        {
            return true;
        }
        return false;
    }

    public void ResetBullet()
    {
        traveledDistance = 0f;
        lastPosition = transform.position;
    }

    protected void ReturnToPool()
    {
        BulletPool.Instance.ReturnBullet(this);
    }
}
