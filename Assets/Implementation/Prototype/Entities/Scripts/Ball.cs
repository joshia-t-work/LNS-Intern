using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LNS.Entities
{
    /// <summary>
    /// A Ball
    /// </summary>
    public class Ball : Entity
    {
        public FuzzyLogicAI owner;
        Vector3 relativePos;
        protected Vector3 _spawnPosition;

        public override void Awake()
        {
            base.Awake();
            _spawnPosition = transform.position;
            MaxHealth.Value = int.MaxValue;
            Health.Value = int.MaxValue;
        }

        public void Hold(FuzzyLogicAI soldier)
        {
            if (owner == null)
            {
                owner = soldier;
                //transform.parent = soldier.transform;
            } else if (owner != soldier)
            {
                owner.HoldBall = false;
                owner = soldier;
                //transform.parent = soldier.transform;
            }
            soldier.HoldBall = true;
            relativePos = owner.transform.position - transform.position;
        }
        public void Drop(FuzzyLogicAI soldier)
        {
            if (owner == soldier)
            {
                Rb.velocity = owner._soldier.AimDirection.normalized * 2f;
                owner.HoldBall = false;
                owner = null;
                //transform.parent = null;
            }
        }

        public override void FixedUpdate()
        {
            _moveSpeed = 0f;
            base.FixedUpdate();
            if (owner != null)
            {
                transform.position = owner.transform.position - relativePos;
                Rb.velocity = owner._soldier.Rb.velocity;
                if (!owner._soldier.IsAlive)
                {
                    Drop(owner);
                }
            }
        }

        public override void OnDeath()
        {
            InstancePool.Deactivate(this);
            InstancePool.s_inst.StartCoroutine(RespawnCoroutine(5f));
            gameObject.SetActive(false);
        }

        IEnumerator RespawnCoroutine(float respawnTime)
        {
            yield return new WaitForSeconds(respawnTime);
            transform.position = _spawnPosition;
            InstancePool.Reactivate(this);
            gameObject.SetActive(true);
        }
    }
}
