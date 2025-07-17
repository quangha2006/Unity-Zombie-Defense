using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviourSingleton<ParticlePool>
{
    [SerializeField] private GameObject hitWallFXPrefab;
    [SerializeField] private GameObject hitZombieFXPrefab;
    [SerializeField] private int poolSizePerType = 10;

    private Dictionary<ParticleType, Queue<GameObject>> poolDict = new Dictionary<ParticleType, Queue<GameObject>>();
    private Dictionary<ParticleType, GameObject> prefabDict = new Dictionary<ParticleType, GameObject>();


    private void Start()
    {
        prefabDict[ParticleType.HitWall] = hitWallFXPrefab;
        prefabDict[ParticleType.HitZombie] = hitZombieFXPrefab;

        foreach (var type in prefabDict.Keys)
        {
            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < poolSizePerType; i++)
            {
                GameObject fx = Instantiate(prefabDict[type], transform);
                fx.SetActive(false);
                queue.Enqueue(fx);
            }

            poolDict[type] = queue;
        }
    }

    public void PlayFX(ParticleType type, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(type)) 
            return;

        GameObject fx = poolDict[type].Count > 0 ? poolDict[type].Dequeue() : Instantiate(prefabDict[type]);

        fx.transform.position = position;
        fx.transform.rotation = rotation;
        fx.SetActive(true);

        StartCoroutine(DeactivateAfterTime(fx, GetDuration(fx)));

        poolDict[type].Enqueue(fx);
    }

    private float GetDuration(GameObject fx)
    {
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            return ps.main.duration + ps.main.startLifetime.constantMax;
        }

        return 1f;
    }

    private IEnumerator DeactivateAfterTime(GameObject fx, float time)
    {
        yield return new WaitForSeconds(time);
        fx.SetActive(false);
    }
    public enum ParticleType { HitWall, HitZombie }
}
