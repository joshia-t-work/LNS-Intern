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
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Entity : Damagable
    {
        [Header("Entity")]
        [SerializeField] protected Slider _healthbar;
        [SerializeField] protected Transform _character;
        protected Vector2 currentAimDirection = Vector2.zero;
        public Vector2 AimDirection { get; private set; }
        public Vector2 MoveDirection { get; private set; }
        public bool IsReloaded { get
            {
                return Time.time - _attackCooldown > ATTACK_COOLDOWN;
            } 
        }
        protected const float ATTACK_COOLDOWN = 1f;
        protected float _attackCooldown = 0f;
        protected Rigidbody2D _rb;
        protected float _respawnTime = 5f;
        protected float _moveSpeed = 1f;
        public virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            Health.AddObserver((value) =>
            {
                _healthbar.value = (float)value / MaxHealth.Value;
            });
        }
        public virtual void FixedUpdate()
        {
            if (MoveDirection != Vector2.zero)
            {
                _rb.AddForce(MoveDirection.normalized * _moveSpeed, ForceMode2D.Impulse);
            }

            currentAimDirection = Vector3.RotateTowards(currentAimDirection, AimDirection, 180f * Mathf.Deg2Rad * Time.deltaTime, 1);
            float angle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;
            _character.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        public void SetMoveDirection(Vector2 dir)
        {
            MoveDirection = dir;
        }
        public void SetAimDirection(Vector2 dir)
        {
            AimDirection = dir;
        }
        public override void OnDeath()
        {
            InstancePool.Deactivate(this);
            InstancePool.inst.StartCoroutine(RespawnCoroutine(_respawnTime));
            gameObject.SetActive(false);
            // TODO: handle death cleanup
        }
        /// <summary>
        /// Called when the object respawns
        /// </summary>
        public virtual void OnRespawn()
        {

        }
        IEnumerator RespawnCoroutine(float respawnTime)
        {
            yield return new WaitForSeconds(respawnTime);
            OnRespawn();
            Health.Value = MaxHealth.Value;
            InstancePool.Reactivate(this);
            gameObject.SetActive(true);
        }
    }
}