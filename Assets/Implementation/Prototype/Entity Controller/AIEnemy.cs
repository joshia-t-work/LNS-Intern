using LNS.AI;
using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    /// <summary>
    /// Enemy soldier with behaviours
    /// </summary>
    public class AIEnemy : BaseSoldierAI
    {
        #region Variables

        [Header("Enemy Settings")]

        [SerializeField]
        AITypes _aiType;

        private int _patrolIndex = 0;
        public enum AITypes
        {
            DoNothing,
            Default,
        }
        private List<Vector3> _retracePath = new List<Vector3>();
        private Vector2 _guardDirection;
        private Vector2 _lastTargetPosition;
        private States _aiState = States.Rest;
        private float _chaseGuardTime;
        private const float CHASE_GUARD_DURATION = 4f;
        protected enum States
        {
            Rest,
            Guard,
            Patrol,
            Chase,
            ChaseGuard,
            Retrace,
        }

        #endregion
        #region MonoBehaviour

        public virtual void Awake()
        {
            if (_aiType == AITypes.DoNothing)
            {
                _aiState = States.Rest;
            } else
            {
                _aiState = States.Guard;
            }
            _retracePath.Add(_soldier.SpawnPosition);
            _guardDirection = _soldier.AimDirection;
            _pathfinder = new DirectionalPathfinder(DirectionalPathfinder.Behaviours.Stop, 2f, 0.5f, 0.5f);
        }

        public override void Update()
        {
            Vector3 _targetPosition = _soldier.SpawnPosition;
            switch (_aiState)
            {
                case States.Rest:
                    break;
                case States.Guard:
                    if (_isTargetVisible)
                    {
                        _aiState = States.Chase;
                    }
                    else
                    {
                        if (PatrolPoints.Length > 0)
                        {
                            _aiState = States.Patrol;
                        }
                        const float LOOK_DURATION = 8f;
                        float sinVal = ((Time.time / LOOK_DURATION) % 1f) * Mathf.PI * 2f;
                        Quaternion leftRotation = Quaternion.Euler(0, 0, Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(sinVal)))))))) / 0.526f * 45f);
                        _soldier.SetAimDirection(leftRotation * _guardDirection);
                    }
                    break;
                case States.Patrol:
                    if (Vector3.Distance(transform.position, PatrolPoints[_patrolIndex]) < 1f)
                    {
                        _patrolIndex = (_patrolIndex + 1) % PatrolPoints.Length;
                    }
                    _targetPosition = PatrolPoints[_patrolIndex];
                    break;
                case States.ChaseGuard:
                    if (_isTargetVisible)
                    {
                        _aiState = States.Chase;
                    }
                    else
                    {
                        const float LOOK_DURATION = 8f;
                        float sinVal = ((Time.time / LOOK_DURATION) % 1f) * Mathf.PI * 2f;
                        Quaternion leftRotation = Quaternion.Euler(0, 0, Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(sinVal)))))))) / 0.526f * 45f);
                        _soldier.SetAimDirection(leftRotation * _guardDirection);
                    }
                    if (Time.time - _chaseGuardTime > CHASE_GUARD_DURATION)
                    {
                        _aiState = States.Retrace;
                    }
                    break;
                case States.Chase:
                    if (!CanSeePoint(_retracePath[_retracePath.Count - 1]))
                    {
                        _retracePath.Add(transform.position);
                    }
                    if (_retracePath.Count > 1)
                    {
                        // simplify retract path, e.g. when going in loops
                        int retraceIndex = -1;
                        for (int i = 0; i < _retracePath.Count; i++)
                        {
                            if (CanSeePoint(_retracePath[i]))
                            {
                                retraceIndex = i;
                                break;
                            }
                        }
                        if (retraceIndex > -1)
                        {
                            while (_retracePath.Count - 1 > retraceIndex)
                            {
                                _retracePath.RemoveAt(_retracePath.Count - 1);
                            }
                        }
                    }
                    if (_isTargetVisible)
                    {
                        _targetPosition = _target.transform.position;
                        _lastTargetPosition = _target.transform.position;
                    }
                    else
                    {
                        _targetPosition = _lastTargetPosition;
                        if (Vector2.Distance(transform.position, _targetPosition) < REACH_POINT_DISTANCE)
                        {
                            _chaseGuardTime = Time.time;
                            _aiState = States.ChaseGuard;
                        }
                    }
                    break;
                case States.Retrace:
                    if (_isTargetVisible)
                    {
                        _aiState = States.Chase;
                    } else
                    {
                        _targetPosition = _retracePath[_retracePath.Count - 1];
                        if (_retracePath.Count == 1)
                        {
                            _aiState = States.Guard;
                        }
                        else
                        {
                            if (Vector2.Distance(transform.position, _targetPosition) < REACH_POINT_DISTANCE)
                            {
                                _retracePath.RemoveAt(_retracePath.Count - 1);
                            }
                        }
                    }
                    break;
            }
            SetTargetPosition(_targetPosition);
            base.Update();
        }

        #endregion
        #region Class Methods

        public bool CanSeePoint(Vector3 point)
        {
            Vector3 directionVector = point - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionVector, directionVector.magnitude, LayerMask.GetMask("Level"));
            return hit.collider == null;
        }

        #endregion
        #region Debugging

        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            if (_retracePath.Count > 0)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(_retracePath[_retracePath.Count - 1], transform.position);
                if (_retracePath.Count > 1)
                {
                    for (int i = 0; i < _retracePath.Count - 1; i++)
                    {
                        Gizmos.DrawLine(_retracePath[i], _retracePath[i + 1]);
                    }
                }
            }
        }

        #endregion
    }
}
