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
        private BulletGrenade grenadePrepare;

        private void Awake()
        {
            SetActiveIndicator(false);
        }

        public override BulletBase StopFire()
        {
            if (isAniming)
            {
                shootTimer = shootSpeed;
                isAniming = false;
                SetActiveIndicator(false);
                grenadePrepare = BulletPool.Instance.GetBullet(WeaponType.GRENADE) as BulletGrenade;
                grenadePrepare.transform.rotation = Quaternion.identity;
                grenadePrepare.ApplyVelocity(lastVelocity);
                return grenadePrepare;
            }
            return null;
        }
        public void ThrowGrenade()
        {
            if (grenadePrepare != null)
            {
                grenadePrepare.transform.position = player.GrenadePos;

                grenadePrepare.gameObject.SetActive(true);
                grenadePrepare = null;
            }
        }
        public override BulletBase[] Fire(Transform playerTrans)
        {
            if (shootTimer > 0f)
                return null;

            var input = InputManager.Instance.GetShootDirection();

            if (input.magnitude > minDistance)
            {
                isAniming = true;
                var dir = new Vector3(input.x, 0, input.y).normalized;
                var force = Mathf.Min(input.magnitude * maxThrowForce, maxThrowForce);

                lastVelocity = CalculateVelocity(dir, force);
                var targetPos = SimulateParabola(player.GrenadePos, lastVelocity, out var pathPoints);

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
        private Vector3 CalculateVelocity(Vector3 dir, float force)
        {
            return dir * force + Vector3.up * (force * Mathf.Tan(throwAngle * Mathf.Deg2Rad));
        }
        private Vector3 SimulateParabola(Vector3 pos, Vector3 velocity, out Vector3[] points)
        {
            var timestep = 0.1f;
            int maxSteps = 30;
            Vector3 gravity = Physics.gravity;
            var pointList = new List<Vector3>(maxSteps);
            var prevPos = pos;

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

        private void DrawArc(Vector3[] points)
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