using System;
using UnityEngine;

public class ZombieArmHitbox: MonoBehaviour
{
    [SerializeField] private Collider hitbox;
    public event Action<Collider> onTriggerEnter;
    public bool hitboxEnabled => hitbox.enabled;
    void Awake()
    {
        if (hitbox != null)
            hitbox.enabled = false;
    }

    public void EnableHitbox()
    {
        if (hitbox != null)
            hitbox.enabled = true;
    }

    public void DisableHitbox()
    {
        if (hitbox != null)
            hitbox.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter?.Invoke(other);
    }
}

