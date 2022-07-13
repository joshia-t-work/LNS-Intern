using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    /// <summary>
    /// A damagable entity that has movement and can collide
    /// </summary>
    public abstract class Entity : Damagable
    {
        #region Variables

        public Vector2 MoveDirection { get; private set; }
        protected Rigidbody2D _rb;
        public Rigidbody2D Rb { get { return _rb; } }
        protected float _respawnTime = 5f;
        protected float _moveSpeed = 1f;

        #endregion
        #region MonoBehaviour

        public virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }
        public virtual void FixedUpdate()
        {
            if (MoveDirection != Vector2.zero)
            {
                _rb.AddForce(MoveDirection.normalized * _moveSpeed, ForceMode2D.Impulse);
            }
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.rigidbody != null)
            {
                Entity collider = collision.rigidbody.GetComponent<Entity>();
                if (collider != null)
                {
                    OnCollision(collider);
                }
            }
        }

        #endregion
        #region Class Methods

        public void SetMoveDirection(Vector2 dir)
        {
            MoveDirection = dir;
        }

        #endregion
        #region Class Triggers

        /// <summary>
        /// Called when this entity collides with another entity.
        /// </summary>
        /// <param name="collider">The other entity colliding</param>
        public virtual void OnCollision(Entity collider)
        {

        }

        #endregion
    }
}