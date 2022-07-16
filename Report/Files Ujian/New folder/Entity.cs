using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    /// <summary>
    /// A damagable entity that has movement, aiming and respawning behaviour
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Entity : Damagable
    {
        [Header("Damagable")]
        [SerializeField] protected Slider healthbar;
        [SerializeField] protected Transform character;
        public Vector2 AimDirection { get; private set; }
        private Vector2 currentAimDirection = Vector2.zero;
        public Vector2 MoveDirection { get; private set; }
        public bool isReloaded { get
            {
                return Time.time - attackCooldown > ATTACK_COOLDOWN;
            } 
        }
        private const float ATTACK_COOLDOWN = 1f;
        private float attackCooldown = 0f;
        private Rigidbody2D rb;
        private LineRenderer lr;
        protected float respawnTime = 5f;
        protected float moveSpeed = 1f;
        private RaycastHit2D hit;
        protected const float GUNDISTANCE = 20f;
        protected const float MELEEDISTANCE = 5f;
        public virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            lr = GetComponentInChildren<LineRenderer>();
            health.AddObserver((value) =>
            {
                healthbar.value = (float)value / maxHealth.Value;
            });
            lr.enabled = false;
        }
        private void FixedUpdate()
        {
            if (MoveDirection != Vector2.zero)
            {
                rb.AddForce(MoveDirection.normalized * moveSpeed, ForceMode2D.Impulse);
            }

            currentAimDirection = Vector3.RotateTowards(currentAimDirection, AimDirection, 180f * Mathf.Deg2Rad * Time.deltaTime, 1);
            float angle = Mathf.Atan2(currentAimDirection.y, currentAimDirection.x) * Mathf.Rad2Deg;
            character.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            Vector3 gunDir = (Vector3)currentAimDirection.normalized;
            Vector3 gunPosition = transform.position + character.transform.up * -0.54f + gunDir * 1.4f;
            Vector3[] vector3s = new Vector3[] { gunPosition - gunDir * 0.2f, gunPosition + gunDir * GUNDISTANCE };
            hit = Physics2D.Raycast(gunPosition, currentAimDirection, GUNDISTANCE);
            if (hit.collider != null)
            {
                vector3s[1] = hit.point;
            }
            if (lr.enabled)
            {
                lr.SetPositions(vector3s);
            }
        }
        public void Fire()
        {
            if (isReloaded)
            {
                attackCooldown = Time.time;
                if (hit.rigidbody != null)
                {
                    Damagable damagable = hit.rigidbody.GetComponent<Damagable>();
                    if (damagable != null)
                    {
                        DealDamage(1, damagable);
                    }
                }
            }
        }
        public void Melee()
        {
            if (isReloaded)
            {
                attackCooldown = Time.time;
                if (hit.rigidbody != null)
                {
                    if (Vector3.Distance(transform.position, hit.point) < MELEEDISTANCE)
                    {
                        Damagable damagable = hit.rigidbody.GetComponent<Damagable>();
                        if (damagable != null)
                        {
                            DealDamage(1, damagable);
                        }
                    }
                }
            }
        }
        public void SetMoveDirection(Vector2 dir)
        {
            MoveDirection = dir;
        }
        public void SetAimDirection(Vector2 dir)
        {
            AimDirection = dir;
        }
        public void SetGunSight(bool enabled)
        {
            lr.enabled = enabled;
        }
        public override void OnDeath()
        {
            InstancePool.Deactivate(this);
            InstancePool.inst.StartCoroutine(RespawnCoroutine(respawnTime));
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
            health.Value = maxHealth.Value;
            InstancePool.Reactivate(this);
            gameObject.SetActive(true);
        }
    }
}