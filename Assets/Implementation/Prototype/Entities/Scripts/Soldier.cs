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
        protected Animator _character;

        [SerializeField]
        protected Animator _feet;

        SpriteRenderer _characterSprite;
        Collider2D[] _collider;
        public Collider2D[] Collider
        {
            get { return _collider; }
        }
        private Vector2 _targetAimDirection = Vector2.zero;
        public Vector2 AimDirection
        {
            get { return _characterSprite.transform.right; }
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
            _isGun = false;
            SwitchStance();
        }
        public virtual void Update()
        {
            UpdateAnimators();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            float angle = Mathf.Atan2(_targetAimDirection.y, _targetAimDirection.x) * Mathf.Rad2Deg;
            _characterSprite.transform.rotation = Quaternion.Slerp(_characterSprite.transform.rotation, Quaternion.Euler(0, 0, angle), 5f * Time.deltaTime);
             //angle = Mathf.Atan2(MoveDirection.y, MoveDirection.x) * Mathf.Rad2Deg;
            _feet.transform.rotation = Quaternion.Slerp(_characterSprite.transform.rotation, Quaternion.Euler(0, 0, angle), 3f * Time.deltaTime);

            Vector3 gunDir = _characterSprite.transform.right;
            Vector3 gunPosition = _characterSprite.transform.position + _characterSprite.transform.up * -0.54f + gunDir * 1.4f;
            Vector3[] vector3s = new Vector3[] { gunPosition - gunDir * 0.2f, gunPosition + gunDir * GUNDISTANCE };
            _hit = Physics2D.Raycast(gunPosition, gunDir, GUNDISTANCE);
            if (_hit.collider != null)
            {
                vector3s[1] = _hit.point;
            }
        }
        private void OnDrawGizmosSelected()
        {
            ExtDebug.ExtDebug.DrawBoxCast2D(_characterSprite.transform.position, new Vector2(1, 2), _characterSprite.transform.eulerAngles.z, _characterSprite.transform.right, 1.5f, Color.white);
        }

        #endregion
        #region Class Methods

        private void UpdateAnimators()
        {
            float speed = Rb.velocity.magnitude / (_moveSpeed * 5f);
            _character.SetFloat("Speed", speed);
            _character.SetBool("IsGun", _isGun);
            _feet.SetFloat("Speed", speed);
            _feet.SetFloat("SpeedMultiplier", speed + 1f);
            _feet.SetFloat("Strafe", Vector3.Dot(Rb.velocity.normalized, _characterSprite.transform.up * -1) / 2f + 0.5f);
        }
        public void SetAimDirection(Vector2 dir)
        {
            _targetAimDirection = dir;
        }
        public void Attack()
        {
            if (IsReloaded)
            {
                _character.SetFloat("ReloadSpeed", 1.25f / ATTACK_COOLDOWN);
                _character.SetTrigger("Fire");
                if (_isGun)
                {
                    _attackCooldown = Time.time + 0.25f;
                    StartCoroutine(DelayedMethod(0.1f, Fire));
                }
                else
                {
                    _attackCooldown = Time.time;
                    StartCoroutine(DelayedMethod(ATTACK_COOLDOWN / 2f, Melee));
                }
            }
        }
        IEnumerator DelayedMethod(float time, System.Action action)
        {
            yield return new WaitForSeconds(time);
            action();
        }
        public void SwitchStance()
        {
            _isGun = !_isGun;
            if (_isGun)
            {
                _moveSpeed = 1.5f;
            }
            else
            {
                _moveSpeed = 2f;
            }
        }
        private void Fire()
        {
            Poolable bullet = InstancePool.TryInstantiate("Bullet");
            Quaternion leftRotation = Quaternion.Euler(0, 0, Random.Range(-2f, 2f));
            bullet.GetComponent<Bullet>().SetBullet(this, GUNDISTANCE, _characterSprite.transform.position + _characterSprite.transform.up * -0.54f + _characterSprite.transform.right * 1.2f, leftRotation * AimDirection);
            //if (_hit.rigidbody != null)
            //{
            //    Damagable damagable = _hit.rigidbody.GetComponent<Damagable>();
            //    if (damagable != null)
            //    {
            //        DealDamage(1, damagable);
            //    }
            //}
        }
        private void Melee()
        {
            RaycastHit2D coll = Physics2D.BoxCast(_characterSprite.transform.position, new Vector2(1, 2), _characterSprite.transform.eulerAngles.z, _characterSprite.transform.right, 1.5f);
            if (coll.rigidbody != null)
            {
                Damagable damagable = coll.rigidbody.GetComponent<Damagable>();
                if (damagable != null)
                {
                    DealDamage(1, damagable);
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
