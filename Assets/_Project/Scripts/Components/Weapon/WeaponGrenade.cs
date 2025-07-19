using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Weapon
{
    public class WeaponGrenade : WeaponBase
    {
        [SerializeField] private Transform indicator;
        [SerializeField] LineRenderer arcRenderer;
        [SerializeField] float maxThrowForce = 15f;
        [SerializeField] float throwAngle = 45f;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float minDistance = 0.4f;
        private bool isAniming = false;
        private Vector3 lastVelocity = Vector3.zero;

        private void Awake()
        {
            SetActiveIndicator(false);
        }
        protected override void Update()
        {

        }
        public override void StopFire()
        {
            if (isAniming)
            {
                isAniming = false;
                SetActiveIndicator(false);
                ThrowGrenade(lastVelocity);
            }
        }
        public void ThrowGrenade(Vector3 velocity)
        {
            Debug.Log("ThrowGrenade");
            BulletGrenade grenade = BulletPool.Instance.GetBullet(WeaponType.GRENADE) as BulletGrenade;
            grenade.transform.position = player.GrenadePos;
            grenade.transform.rotation = Quaternion.identity;
            grenade.ApplyVelocity(velocity);
            grenade.gameObject.SetActive(true);
        }
        public override BulletBase[] Fire(Transform playerTrans)
        {
            var input = InputManager.Instance.GetShootDirection();

            if (input.magnitude > minDistance)
            {
                isAniming = true;
                Vector3 dir = new Vector3(input.x, 0, input.y).normalized;
                float force = Mathf.Min(input.magnitude * maxThrowForce, maxThrowForce);

                lastVelocity = CalculateVelocity(dir, force);
                Vector3 targetPos = SimulateParabola(player.GrenadePos, lastVelocity, out var pathPoints);

                indicator.position = targetPos;
                SetActiveIndicator(true);
                DrawArc(pathPoints);
            }
            else
            {
                isAniming = false;
                SetActiveIndicator(false);
            }
            return null;
        }
        Vector3 CalculateVelocity(Vector3 dir, float force)
        {
            return dir * force + Vector3.up * (force * Mathf.Tan(throwAngle * Mathf.Deg2Rad));
        }
        Vector3 SimulateParabola(Vector3 pos, Vector3 velocity, out Vector3[] points)
        {
            float timestep = 0.1f;
            int maxSteps = 30;
            Vector3 gravity = Physics.gravity;
            List<Vector3> pointList = new List<Vector3>(maxSteps);
            Vector3 prevPos = pos;

            for (int i = 0; i < maxSteps; i++)
            {
                float t = i * timestep;
                Vector3 point = pos + velocity * t + 0.5f * gravity * t * t;
                

                Vector3 dir = point - prevPos;
                float dist = dir.magnitude;
                if (Physics.Raycast(prevPos, dir.normalized, out RaycastHit hit, dist, obstacleLayer))
                {
                    pointList.Add(hit.point);
                    points = pointList.ToArray();
                    return hit.point;
                }
                else
                {
                    pointList.Add(point);
                }

                prevPos = point;
            }
            points = pointList.ToArray();
            return pointList.Last();
        }

        void DrawArc(Vector3[] points)
        {
            arcRenderer.positionCount = points.Length;
            arcRenderer.SetPositions(points);
        }
        private void SetActiveIndicator(bool isActive)
        {
            indicator.gameObject.SetActive(isActive);
            arcRenderer.gameObject.SetActive(isActive);
        }
    }
}