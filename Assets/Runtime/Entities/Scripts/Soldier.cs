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
    [RequireComponent(typeof(LineRenderer))]
    public class Soldier : Entity
    {
        [Header("Soldier")]
        [SerializeField] protected Slider _healthbar;
        [SerializeField] protected Transform _character;
        [SerializeField] Sprite _meleeSprite;
        [SerializeField] Sprite _gunSprite;
        SpriteRenderer _characterSprite;
        protected Vector2 currentAimDirection = Vector2.zero;
        public Vector2 AimDirection { get; private set; }
        private RaycastHit2D _hit;
        protected const float GUNDISTANCE = 20f;
        protected const float MELEEDISTANCE = 5f;
        protected bool _isGun = false;
        private LineRenderer _lr;
        public bool IsReloaded
        {
            get
            {
                return Time.time - _attackCooldown > ATTACK_COOLDOWN;
            }
        }
        protected const float ATTACK_COOLDOWN = 1f;
        private float _attackCooldown = 0f;
        public override void Awake()
        {
            base.Awake();
            _characterSprite = _character.GetComponent<SpriteRenderer>();
            Health.AddObserver((value) =>
            {
                _healthbar.value = (float)value / MaxHealth.Value;
            });
            _lr = GetComponentInChildren<LineRenderer>();
            _lr.enabled = false;
        }
        private void Update()
        {
            if (IsReloaded)
            {
                SetGunSight(true);
            }
            else
            {
                SetGunSight(false);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            currentAimDirection = Vector3.RotateTowards(currentAimDirection, AimDirection, 180f * Mathf.Deg2Rad * Time.deltaTime, 1);
            float angle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;
            _character.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

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
        public void SetAimDirection(Vector2 dir)
        {
            AimDirection = dir;
        }
        public void Attack()
        {
            if (_isGun)
            {
                Fire();
            }
            else
            {
                Melee();
            }
        }
        public void SwitchStance()
        {
            _isGun = !_isGun;
            SetGunSight(_isGun);
            if (_isGun)
            {
                _characterSprite.sprite = _gunSprite;
            }
            else
            {
                _characterSprite.sprite = _meleeSprite;
            }
        }
        private void Fire()
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
        private void Melee()
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
        public override void OnDeath()
        {
            InstancePool.Deactivate(this);
            InstancePool.inst.StartCoroutine(RespawnCoroutine(_respawnTime));
            gameObject.SetActive(false);
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
