using UnityEngine;
using static ParticlePool;

public class BulletBase : MonoBehaviour
{
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private int damage = 10;
    [SerializeField] public LayerMask hitMask;

    public float speed = 0f;

    private Vector3 lastPosition;
    private float traveledDistance = 0f;

    void Start()
    {
        ResetBullet();
    }

    void Update()
    {
        if (speed <= 0.01f)
            return;

        float distanceThisFrame = speed * Time.deltaTime;
        Vector3 currentPosition = transform.position + transform.forward * distanceThisFrame;
        RaycastHit hit;
        //Debug.DrawLine(lastPosition, currentPosition, Color.red, 100.0f);
        if (Physics.Raycast(lastPosition, (currentPosition - lastPosition).normalized, out hit, distanceThisFrame, hitMask))
        {
            Debug.Log($"Bullet hit {hit.collider.name}");
            if (hit.collider.CompareTag("Zombie"))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
                ReturnToPool();
                return;
            }
            else if (hit.collider.CompareTag("Obstacle"))
            {
                Vector3 bulletDir = (hit.point - lastPosition).normalized;
                Quaternion particleRotation = Quaternion.LookRotation(-bulletDir);
                ParticlePool.Instance.PlayFX(ParticleType.HitWall, hit.point, particleRotation);
                ReturnToPool();
                return;
            }
        }

        transform.position = currentPosition;
        lastPosition = currentPosition;

        traveledDistance += distanceThisFrame;
        if (traveledDistance >= maxDistance)
        {
            ReturnToPool();
        }
    }
    public void ResetBullet()
    {
        traveledDistance = 0f;
        lastPosition = transform.position;
    }

    private void ReturnToPool()
    {
        BulletPool.Instance.ReturnBullet(this);
    }
}
