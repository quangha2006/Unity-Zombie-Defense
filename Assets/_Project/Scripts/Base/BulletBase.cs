using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private int damage = 10;
    [SerializeField] private ParticleSystem trail;
    [SerializeField] public LayerMask hitMask;

    public float speed = 0f;

    private Vector3 lastPosition;
    private float traveledDistance = 0f;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (speed <= 0.01f)
            return;
        if (trail != null && !trail.isPlaying)
        {
            trail.Play();
        }
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
                    // Spawn effect
                    //Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
                }
            }
            else if (hit.collider.CompareTag("Obstacle"))
            {
                // Spawn effect
                //Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
            }

            Destroy(gameObject);
            return;
        }

        transform.position = currentPosition;
        lastPosition = currentPosition;

        traveledDistance += distanceThisFrame;
        if (traveledDistance >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
}
