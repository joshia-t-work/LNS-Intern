using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    [RequireComponent(typeof(LineRenderer))]
    public class Soldier : Entity
    {
        private RaycastHit2D _hit;
        protected const float GUNDISTANCE = 20f;
        protected const float MELEEDISTANCE = 5f;
        private LineRenderer _lr;
        public override void Awake()
        {
            base.Awake();
            _lr = GetComponentInChildren<LineRenderer>();
            _lr.enabled = false;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            Vector3 gunDir = (Vector3)currentAimDirection.normalized;
            Vector3 gunPosition = transform.position + _character.transform.up * -0.54f + gunDir * 1.4f;
            Vector3[] vector3s = new Vector3[] { gunPosition - gunDir * 0.2f, gunPosition + gunDir * GUNDISTANCE };
            _hit = Physics2D.Raycast(gunPosition, currentAimDirection, GUNDISTANCE);
            if (_hit.collider != null)
            {
                vector3s[1] = _hit.point;
            }
            if (_lr.enabled)
            {
                _lr.SetPositions(vector3s);
            }
        }
        public void Fire()
        {
            if (IsReloaded)
            {
                _attackCooldown = Time.time;
                if (_hit.rigidbody != null)
                {
                    Damagable damagable = _hit.rigidbody.GetComponent<Damagable>();
                    if (damagable != null)
                    {
                        DealDamage(1, damagable);
                    }
                }
            }
        }
        public void Melee()
        {
            if (IsReloaded)
            {
                _attackCooldown = Time.time;
                if (_hit.rigidbody != null)
                {
                    if (Vector3.Distance(transform.position, _hit.point) < MELEEDISTANCE)
                    {
                        Damagable damagable = _hit.rigidbody.GetComponent<Damagable>();
                        if (damagable != null)
                        {
                            DealDamage(1, damagable);
                        }
                    }
                }
            }
        }
        public void SetGunSight(bool enabled)
        {
            _lr.enabled = enabled;
        }
    }
}
