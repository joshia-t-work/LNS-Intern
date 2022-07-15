using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LNS.Entities
{
    /// <summary>
    /// A damagable entity that has movement, aiming and respawning behaviour
    /// </summary>
    public class Bullet : Entity
    {
        #region Variables

        [SerializeField]
        GameObject _bulletDisplay;

        [SerializeField]
        ParticleSystem _particleSystem;

        Collider2D[] _prevCollider;
        Collider2D _bulletCollider;
        float lifetime = 0f;
        public const float MOVE_SPEED = 100f;

        #endregion
        #region MonoBehaviour

        public override void Awake()
        {
            base.Awake();
            _bulletCollider = GetComponentInChildren<Collider2D>();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            lifetime -= Time.deltaTime;
            if (lifetime <= 0)
            {
                Kill();
            }
        }

        #endregion
        #region Class Methods

        public void SetBullet(Soldier owner, float distance, Vector2 position, Vector2 aimDirection)
        {
            SetOwner(owner);
            ChangeOwner(owner);
            transform.position = position;
            lifetime = distance / MOVE_SPEED;

            _moveSpeed = 0f;
            MoveDirection = aimDirection;
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.AddForce(aimDirection.normalized * MOVE_SPEED, ForceMode2D.Impulse);
        }
        public void ChangeOwner(Soldier owner)
        {
            if (_prevCollider != null)
            {
                for (int i = 0; i < owner.Collider.Length; i++)
                {
                    Physics2D.IgnoreCollision(_prevCollider[i], _bulletCollider, false);
                }
            }
            _prevCollider = owner.Collider;
            for (int i = 0; i < owner.Collider.Length; i++)
            {
                Physics2D.IgnoreCollision(owner.Collider[i], _bulletCollider, true);
            }
        }

        #endregion
        #region Class Overrides

        public override void OnDeath()
        {
            InstancePool.RemoveFromPool(this);
        }
        public override void OnPoolCreate()
        {
            base.OnPoolCreate();
            _bulletDisplay.SetActive(true);
            enabled = true;
            _particleSystem.Play();
        }
        public override void OnPoolRemove()
        {
            base.OnPoolRemove();
            _bulletDisplay.SetActive(false);
            enabled = false;
            _particleSystem.Stop();
        }
        public override void OnCollision(Entity collider)
        {
            collider.PreventCollision();
            collider.Rb.AddForce(MoveDirection.normalized * MOVE_SPEED / 10f, ForceMode2D.Impulse);
            DealDamage(1, collider);
            Kill();
        }

        #endregion
    }
}
