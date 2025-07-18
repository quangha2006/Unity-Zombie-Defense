using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviourSingleton<ParticlePool>
{
    [SerializeField] private ParticleSystem hitWallFXPrefab;
    [SerializeField] private ParticleSystem hitZombieFXPrefab;
    [SerializeField] private ParticleSystem bulletExplosionPrefab;

    [SerializeField] private int poolSizePerType = 10;

    private Dictionary<ParticleType, Queue<ParticleSystem>> poolDict = new Dictionary<ParticleType, Queue<ParticleSystem>>();
    private Dictionary<ParticleType, ParticleSystem> prefabDict = new Dictionary<ParticleType, ParticleSystem>();


    private void Start()
    {
        prefabDict[ParticleType.HitWall] = hitWallFXPrefab;
        prefabDict[ParticleType.HitZombie] = hitZombieFXPrefab;
        prefabDict[ParticleType.BulletShotgunExplosive] = bulletExplosionPrefab;

        foreach (var type in prefabDict.Keys)
        {
            Queue<ParticleSystem> queue = new Queue<ParticleSystem>();

            for (int i = 0; i < poolSizePerType; i++)
            {
                ParticleSystem fx = Instantiate(prefabDict[type], transform);
                fx.gameObject.SetActive(false);
                queue.Enqueue(fx);
            }

            poolDict[type] = queue;
        }
    }

    public void PlayFX(ParticleType type, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(type)) 
            return;

        ParticleSystem fx = poolDict[type].Count > 0 ? poolDict[type].Dequeue() : Instantiate(prefabDict[type]);

        fx.transform.position = position;
        fx.transform.rotation = rotation;
        fx.gameObject.SetActive(true);
        fx.Play(true);
        var durationfx = GetDuration(fx);
        StartCoroutine(DeactivateAfterTime(type, fx, durationfx));
    }

    private float GetDuration(ParticleSystem fx)
    {
        if (fx != null)
        {
            return fx.main.duration + fx.main.startLifetime.constantMax;
        }

        return 1f;
    }

    private IEnumerator DeactivateAfterTime(ParticleType type, ParticleSystem fx, float time)
    {
        yield return new WaitForSeconds(time);
        fx.gameObject.SetActive(false);
        poolDict[type].Enqueue(fx);
    }
    public enum ParticleType { HitWall, HitZombie, BulletShotgunExplosive }
}
