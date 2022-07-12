using LNS.ObjectPooling;
using LNS.Observable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    /// <summary>
    /// A damagable object that has health, can damage and be damaged by others and can die. Can be owned.
    /// </summary>
    public abstract class Damagable : Poolable
    {
        #region Variables

        public ObservableValue<int> Health { get; private set; } = new ObservableValue<int>(3);
        public ObservableValue<int> MaxHealth { get; private set; } = new ObservableValue<int>(3);
        public Damagable Owner
        {
            get { return _owner; }
        }
        private Damagable _owner;

        #endregion
        #region Class Methods

        /// <summary>
        /// Sets the owner of the damagable. e.g. bullet owned by player
        /// </summary>
        /// <param name="owner">Owner of the entity</param>
        public void SetOwner(Damagable owner)
        {
            _owner = owner;
        }
        /// <summary>
        /// Gets the health of this damagable
        /// </summary>
        /// <returns>Health of damagable</returns>
        public int GetHealth()
        {
            return Health.Value;
        }
        /// <summary>
        /// Manually sets health to this damagable, reducing to zero kills it
        /// </summary>
        /// <param name="value">Value of health</param>
        public void SetHealth(int value)
        {
            Health.Value = value;
        }
        /// <summary>
        /// Manually adds health to this damagable, reducing to zero kills it
        /// </summary>
        /// <param name="value">Value of health</param>
        public void AddHealth(int value)
        {
            Health.Value += value;
            if (Health.Value <= 0)
            {
                Kill();
            }
        }
        /// <summary>
        /// Recursively traces the owner of the damagable up to the root parent.
        /// </summary>
        /// <returns>Root owner</returns>
        public Damagable TraceOwner()
        {
            Damagable damagable = this;
            while (damagable.Owner != null)
            {
                damagable = damagable.Owner;
            }
            return damagable;
        }
        /// <summary>
        /// Makes this damagable take damage
        /// </summary>
        /// <param name="value">Value of damage</param>
        public void TakeDamage(int value)
        {
            AddHealth(-value);
        }
        /// <summary>
        /// Makes this damagable take damage
        /// </summary>
        /// <param name="value">Value of damage</param>
        /// <param name="source">Damager</param>
        public void TakeDamage(int value, Damagable source)
        {
            TakeDamage(value);
            if (Health.Value <= 0)
            {
                Damagable damagable = source.TraceOwner();
                damagable.OnKill(this);
                OnKilledBy(damagable);
            }
        }
        /// <summary>
        /// Kills this damagable (kill self)
        /// </summary>
        public void Kill()
        {
            OnDeath();
        }
        /// <summary>
        /// Kills the target damagable
        /// </summary>
        /// <param name="target">Target</param>
        public void Kill(Damagable target)
        {
            target.OnDeath();
            target.OnKilledBy(this);
            Damagable rootTarget = target.TraceOwner();
            OnKill(rootTarget);
        }
        /// <summary>
        /// Deals damage to a target damagable
        /// </summary>
        /// <param name="value">Value of damage</param>
        /// <param name="target">Target damagable</param>
        public void DealDamage(int value, Damagable target)
        {
            target.TakeDamage(value, this);
        }

        #endregion
        #region Class Triggers

        public virtual void OnDeath()
        {

        }
        /// <summary>
        /// Triggers when this damagable kills another. Will only be called by root owner.
        /// </summary>
        /// <param name="other">The damagable killed</param>
        public virtual void OnKill(Damagable other)
        {

        }
        /// <summary>
        /// Triggers when this damagable is killed by another.
        /// </summary>
        /// <param name="other">The damagable responsible for the last damage dealth</param>
        public virtual void OnKilledBy(Damagable other)
        {

        }

        #endregion
    }
}