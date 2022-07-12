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
    public class Soldier : Entity
    {
        #region Variables

        [Header("Soldier")]

        [SerializeField]
        protected Slider _healthbar;

        [SerializeField]
        protected Transform _character;

        [SerializeField]
        Sprite _meleeSprite;

        [SerializeField]
        Sprite _gunSprite;

        SpriteRenderer _characterSprite;
        Collider2D[] _collider;
        public Collider2D[] Collider
        {
            get { return _collider; }
        }
        private Vector2 _currentAimDirection = Vector2.zero;
        private Vector2 _targetAimDirection = Vector2.zero;
        public Vector2 AimDirection
        {
            get { return _currentAimDirection; }
        }
        private RaycastHit2D _hit;
        protected const float GUNDISTANCE = 20f;
        protected const float MELEEDISTANCE = 5f;
        protected bool _isGun = false;
        public bool IsReloaded
        {
            get
            {
                return Time.time - _attackCooldown > ATTACK_COOLDOWN;
            }
        }
        protected const float ATTACK_COOLDOWN = 1f;
        private float _attackCooldown = 0f;

        #endregion
        #region MonoBehaviour

        public override void Awake()
        {
            base.Awake();
            _characterSprite = _character.GetComponent<SpriteRenderer>();
            Health.AddObserver((value) =>
            {
                _healthbar.value = (float)value / MaxHealth.Value;
            });
            _collider = GetComponentsInChildren<Collider2D>();
            _isGun = true;
            SwitchStance();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            _currentAimDirection = Vector3.RotateTowards(_currentAimDirection, _targetAimDirection, 180f * Mathf.Deg2Rad * Time.deltaTime, 1);
            float angle = Mathf.Atan2(_currentAimDirection.y, _currentAimDirection.x) * Mathf.Rad2Deg;
            _characterSprite.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            Vector3 gunDir = (Vector3)_currentAimDirection.normalized;
            Vector3 gunPosition = _characterSprite.transform.position + _characterSprite.transform.up * -0.54f + gunDir * 1.4f;
            Vector3[] vector3s = new Vector3[] { gunPosition - gunDir * 0.2f, gunPosition + gunDir * GUNDISTANCE };
            _hit = Physics2D.Raycast(gunPosition, _currentAimDirection, GUNDISTANCE);
            if (_hit.collider != null)
            {
                vector3s[1] = _hit.point;
            }
        }

        #endregion
        #region Class Methods

        public void SetAimDirection(Vector2 dir)
        {
            _targetAimDirection = dir;
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
            if (_isGun)
            {
                _moveSpeed = 1.5f;
                _characterSprite.sprite = _gunSprite;
            }
            else
            {
                _moveSpeed = 2f;
                _characterSprite.sprite = _meleeSprite;
            }
        }
        private void Fire()
        {
            if (IsReloaded)
            {
                _attackCooldown = Time.time;
                Poolable bullet = InstancePool.TryInstantiate("Bullet");
                Quaternion leftRotation = Quaternion.Euler(0, 0, Random.Range(-2f,2f));
                bullet.GetComponent<Bullet>().SetBullet(this, GUNDISTANCE, _characterSprite.transform.position + _characterSprite.transform.up * -0.54f + (Vector3)_currentAimDirection.normalized * 1.2f, leftRotation * AimDirection);
                //if (_hit.rigidbody != null)
                //{
                //    Damagable damagable = _hit.rigidbody.GetComponent<Damagable>();
                //    if (damagable != null)
                //    {
                //        DealDamage(1, damagable);
                //    }
                //}
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

        #endregion
        #region Class Triggers

        public override void OnDeath()
        {
            InstancePool.Deactivate(this);
            InstancePool.s_inst.StartCoroutine(RespawnCoroutine(_respawnTime));
            gameObject.SetActive(false);
        }
        IEnumerator RespawnCoroutine(float respawnTime)
        {
            yield return new WaitForSeconds(respawnTime);
            OnRespawn();
            Health.Value = MaxHealth.Value;
            InstancePool.Reactivate(this);
            gameObject.SetActive(true);
        }
        /// <summary>
        /// Called when the object respawns
        /// </summary>
        public virtual void OnRespawn()
        {

        }

        #endregion
    }
}
