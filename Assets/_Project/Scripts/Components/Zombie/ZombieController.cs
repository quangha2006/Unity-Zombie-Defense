using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator anim;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private NavMeshObstacle agentObstacle;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float accelerationSpeed;
    [SerializeField] private float angularSpeed;
    [SerializeField] private float stoppingDistance;
    [SerializeField] private float updatePlayerPosRate = 0.2f;
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float enableHitBoxTime = 0.3f;
    [SerializeField] private float disableHitBoxTime = 0.8f;
    [SerializeField] private ZombieArmHitbox hitbox;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] public bool gameReady = false;

    public Transform playerTarget;
    public NavMeshAgent NavMeshAgent => agent;
    private float updatePlayerPosTimer;
    private float smoothSpeed = 0f;
    private float lerpSpeed = 5f;
    private float rotationTolerance = 5f;
    private bool isAttacking = false;
    private bool hasDealtDamage = false;
    private AttackStateBehaviour[] attackStateBehaviour;
    private float currentHealth;
    private bool isDead = false;
    private Collider collider;
    void Start()
    {
        agent.speed = moveSpeed;
        agent.acceleration = accelerationSpeed;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stoppingDistance;
        updatePlayerPosTimer = updatePlayerPosRate;
        attackStateBehaviour = anim.GetBehaviours<AttackStateBehaviour>();
        foreach (var behaviour in attackStateBehaviour)
        {
            behaviour.onStateExit += OnAnimAttackExit;
            behaviour.onStateUpdate += OnAnimAttackUpdate;
        }
        hitbox.onTriggerEnter += OnArmHitboxTriggerEnter;
        currentHealth = maxHealth;
        collider = GetComponent<Collider>();
        SwitchToAgent();
    }

    void Update()
    {
        if (playerTarget == null || isDead || !gameReady)
        {
            return;
        }
        if (updatePlayerPosTimer > 0f)
            updatePlayerPosTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, playerTarget.position);

        if (updatePlayerPosTimer < 0f && distance > stoppingDistance)
        {
            if (!isAttacking)
            {
                SwitchToAgent();
                agent.SetDestination(playerTarget.position);
            }
            anim.SetBool("Attacking", false);
            updatePlayerPosTimer = updatePlayerPosRate;
        }
        else if (distance <= stoppingDistance)
        {
            SwitchToObstacle();
            if (!isAttacking && RotateTowardsTarget())
            {
                anim.SetBool("Attacking", true);
                isAttacking = true;
            }
        }
        float currentMoveSpeed = agent.velocity.magnitude;

        smoothSpeed = Mathf.MoveTowards(smoothSpeed, currentMoveSpeed > 0.1f ? 1f : 0, lerpSpeed * Time.deltaTime);

        anim.SetFloat("MoveSpeed", smoothSpeed);

        if (!agent.enabled && !hitbox.hitboxEnabled)
        {
            RotateTowardsTarget();
        }
    }
    private bool RotateTowardsTarget()
    {
        Vector3 direction = (playerTarget.position - transform.position).normalized;

        direction.y = 0f;

        if (direction == Vector3.zero)
            return true;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        
        float angle = Quaternion.Angle(transform.rotation, lookRotation);
        return angle <= rotationTolerance;
    }
    public void ResetZombie()
    {
        // TODO
    }

    public void DealDamage()
    {
        if (playerTarget != null)
        {
            IDamageable damageable = playerTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damageAmount);
            }
        }
    }

    public void OnArmHitboxTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasDealtDamage)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                hasDealtDamage = true;
                damageable.TakeDamage(damageAmount);
            }
        }
    }

    public void OnAnimAttackExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        isAttacking = false;
        hitbox.DisableHitbox();
        hasDealtDamage = false;
    }
    public void OnAnimAttackUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float animTime = stateInfo.normalizedTime % 1f;
        if (!hitbox.hitboxEnabled && animTime >= enableHitBoxTime && animTime < disableHitBoxTime)
        {
            hitbox.EnableHitbox();
        }
        if (hitbox.hitboxEnabled && animTime >= disableHitBoxTime)
        {
            hitbox.DisableHitbox();
            isAttacking = false;
            hasDealtDamage = false;
        }
    }

    void OnDisable()
    {
        foreach (var behaviour in attackStateBehaviour)
        {
            behaviour.onStateExit -= OnAnimAttackExit;
            behaviour.onStateUpdate -= OnAnimAttackUpdate;
        }
        hitbox.onTriggerEnter -= OnArmHitboxTriggerEnter;
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        if (agentObstacle != null)
        {
            agentObstacle.enabled = false;
        }

        if (anim != null)
        {
            anim.SetBool("Death", true);
        }

        if (collider != null)
            collider.enabled = false;

        //Play particle and destroy or return to pool
        //Destroy(gameObject, 5f);
    }
    private void SwitchToAgent()
    {
        if (!agent.enabled)
        {
            agentObstacle.enabled = false;
            agent.enabled = true;
        }
        agent.isStopped = false;
    }

    private void SwitchToObstacle()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
            agentObstacle.enabled = true;
        }
    }
}
