using System;
using UnityEngine;
using UnityEngine.AI;
using static ParticlePool;

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
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float enableHitBoxTime = 0.3f;
    [SerializeField] private float disableHitBoxTime = 0.8f;
    [SerializeField] private ZombieArmHitbox hitbox;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] public bool gameReady = false;
    [SerializeField] private Transform bodyPos;
    [SerializeField] private string deathVfx;
    [SerializeField] private bool cheat = false;
    [SerializeField] private string hitSfx;
    [HideInInspector] public PlayerController playerTarget;
    public NavMeshAgent NavMeshAgent => agent;
    private float updatePlayerPosTimer;
    private float smoothSpeed = 0f;
    private float lerpSpeed = 5f;
    private float rotationTolerance = 5f;
    private bool isAttacking = false;
    private bool hasDealtDamage = false;
    private AttackStateBehaviour[] attackStateBehaviour;
    private int currentHealth;
    private bool isDead = false;
    private Collider collider;
    public event Action<int, int> onHealthChanged;
    public event Action onDie;
    public int health => currentHealth;
    public int MaxHealth => maxHealth;
    private bool isGoAround = false;
    private float goAroundWaiting;
    private const float maxGoAroundWaiting = 6f;

    void Awake()
    {
        currentHealth = maxHealth;
    }
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
            behaviour.onStateEnter += OnAnimAttackEnter;
        }
        hitbox.onTriggerEnter += OnArmHitboxTriggerEnter;

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

        Vector3 playerPos = playerTarget.transform.position;
        float distance = Vector3.Distance(transform.position, playerPos);

        if (playerTarget.isDeath)
        {
            if (!isAttacking)
            {
                anim.SetBool("Attacking", false);

                SwitchToAgent();
                
                if (agent.remainingDistance <= agent.stoppingDistance && goAroundWaiting <= 0f)
                {
                    goAroundWaiting = UnityEngine.Random.Range(1, maxGoAroundWaiting);
                }

                goAroundWaiting -= Time.deltaTime;

                if (goAroundWaiting  <= 0f && (!isGoAround || agent.remainingDistance <= agent.stoppingDistance))
                {
                    Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 15f;
                    randomDirection += transform.position;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomDirection, out hit, 15f, NavMesh.AllAreas))
                    {
                        agent.SetDestination(hit.position);
                        agent.speed = moveSpeed * 0.6f;
                        anim.SetFloat("RunAnimSpeed", 0.6f);
                        isGoAround = true;
                    }
                }
            }
        }
        else if (updatePlayerPosTimer < 0f && distance > stoppingDistance)
        {
            if (!isAttacking)
            {
                SwitchToAgent();
                agent.SetDestination(playerPos);
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
        Vector3 direction = (playerTarget.transform.position - transform.position).normalized;

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
        currentHealth = maxHealth;
        isGoAround = false;
        isAttacking = false;
        anim.SetBool("Attacking", false);
        anim.SetFloat("RunAnimSpeed", 1f);
        SwitchToAgent();
        agent.speed = moveSpeed;
        agent.acceleration = accelerationSpeed;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stoppingDistance;
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
    public void OnAnimAttackEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!string.IsNullOrEmpty(hitSfx))
        {
            SoundManager.Instance.PlaySFX(hitSfx);
        }
    }
    void OnDisable()
    {
        foreach (var behaviour in attackStateBehaviour)
        {
            behaviour.onStateExit -= OnAnimAttackExit;
            behaviour.onStateUpdate -= OnAnimAttackUpdate;
            behaviour.onStateEnter -= OnAnimAttackEnter;
        }
        hitbox.onTriggerEnter -= OnArmHitboxTriggerEnter;
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;
#if UNITY_EDITOR
        if (!cheat)
#endif
            currentHealth -= amount;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        ParticlePool.Instance.PlayFX(ParticleType.HitZombie, bodyPos.position, Quaternion.identity);
        if (currentHealth <= 0)
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
        onDie?.Invoke();
        SoundManager.Instance.PlaySFX(deathVfx);
        //Play particle and destroy or return to pool
        //Destroy(gameObject, 5f);
    }
    private void SwitchToAgent()
    {
        if (!agent.enabled)
        {
            agentObstacle.enabled = false;
            agent.enabled = true;
            agent.ResetPath();
            agent.Warp(transform.position);
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
