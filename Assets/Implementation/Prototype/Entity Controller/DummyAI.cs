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
                    _targetPosition = transform.position;
                    if (Vector3.Distance(transform.position, _target.transform.position) < 10f)
                    {
                        _pathfinder.AddKeepDistanceConsideration((_target.transform.position - transform.position).normalized, 1f, 10f);
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
            if (Vector2.Distance(transform.position, _targetPosition) > REACH_POINT_DISTANCE)
            {
                _pathfinder.AddConsideration((_targetPosition - transform.position).normalized, 1f);
                _pathfinder.AddConsideration(_soldier.MoveDirection.normalized, 0.5f);
            }
            base.Update();
        }

        #endregion
    }
}
