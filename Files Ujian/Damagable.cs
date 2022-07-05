using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// A damagable object that has health, can damage and be damaged by others and can die
    /// </summary>
    public abstract class Damagable : Poolable
    {
        public ObservableValue<int> health { get; private set; } = new ObservableValue<int>(3);
        public ObservableValue<int> maxHealth { get; private set; } = new ObservableValue<int>(3);
        private void Start()
        {
            if (!isPooled)
            {
                InstancePool.AddToPool(this);
            }
        }
        public int GetHealth()
        {
            return health.Value;
        }
        public void SetHealth(int value)
        {
            health.Value = value;
        }
        public void AddHealth(int value)
        {
            health.Value += value;
            if (health.Value <= 0)
            {
                OnDeath();
            }
        }
        public void TakeDamage(int value)
        {
            AddHealth(-value);
        }
        public void TakeDamage(int value, Damagable source)
        {
            TakeDamage(value);
            if (health.Value <= 0)
            {
                source.OnKill(this);
                OnKilled(source);
            }
        }
        public void DealDamage(int value, Damagable source)
        {
            source.TakeDamage(value, this);
        }
        public virtual void OnDeath()
        {

        }
        public virtual void OnKill(Damagable other)
        {

        }
        public virtual void OnKilled(Damagable other)
        {

        }

    }
}