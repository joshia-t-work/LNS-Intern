using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    /// <summary>
    /// A damagable entity that has movement
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class Entity : Damagable
    {
        public Vector2 MoveDirection { get; private set; }
        protected Rigidbody2D _rb;
        protected float _respawnTime = 5f;
        protected float _moveSpeed = 1f;
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
        public void SetMoveDirection(Vector2 dir)
        {
            MoveDirection = dir;
        }
    }
}