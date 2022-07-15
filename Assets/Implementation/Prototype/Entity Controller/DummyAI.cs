using LNS.AI;
using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    /// <summary>
    /// Enemy with dummy AI
    /// </summary>
    public class DummyAI : BaseSoldierAI
    {
        #region Variables

        [Header("Dummy Settings")]

        [SerializeField]
        bool _runOnSight;

        private int _patrolIndex = 0;

        #endregion
        #region MonoBehaviour

        public virtual void Awake()
        {
            _pathfinder = new DirectionalPathfinder(DirectionalPathfinder.Behaviours.Stop, 2f, 0.5f, 0.5f);
        }

        public override void Update()
        {
            Vector3 _targetPosition = _soldier.SpawnPosition;
            if (PatrolPoints.Length > 0)
            {
                if (Vector3.Distance(transform.position, PatrolPoints[_patrolIndex]) < 1f)
                {
                    _patrolIndex = (_patrolIndex + 1) % PatrolPoints.Length;
                }
                _targetPosition = PatrolPoints[_patrolIndex];
            }
            if (_runOnSight)
            {
                if (_isTargetVisible)
                {
                    if (Vector3.Distance(transform.position, _target.transform.position) < 10f)
                    {
                        Vector2 aimDirection = _target.transform.position - transform.position;
                        _targetPosition = transform.position - (Vector3)aimDirection.normalized;
                    } else
                    {
                        _targetPosition = transform.position;
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, _soldier.SpawnPosition) > 5f)
                    {
                        _targetPosition = _soldier.SpawnPosition;
                    }
                }
            }
            SetTargetPosition(_targetPosition);
            base.Update();
        }

        #endregion
    }
}
