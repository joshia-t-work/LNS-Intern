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

        [SerializeField]
        AIEnemy[] _commanderPlatoon;

        private int _patrolIndex = 0;
        public enum AITypes
        {
            DoNothing,
            Default,
            Commander,
        }
        private List<Vector3> _retracePath = new List<Vector3>();
        private Vector2 _guardDirection;
        private Vector2 _chaseGuardDirection;
        private Vector2 _lastTargetPosition;
        private Vector2 _forceMovePosition;
        private States _aiState = States.Rest;
        private float _chaseGuardTime;
        private const float CHASE_GUARD_DURATION = 4f;
        private const float UNREACHABLE_CHECK_TIME = 2f;
        private bool _isUnreachable;
        private float _unreachableCheck;
        protected enum States
        {
            Rest,
            Guard,
            Patrol,
            Chase,
            ChaseGuard,
            OnCommand,
            Retrace,
        }

        #endregion
        #region MonoBehaviour

        public override void Awake()
        {
            base.Awake();
            if (_aiType == AITypes.DoNothing)
            {
                _aiState = States.Rest;
            } else
            {
                _aiState = States.Guard;
            }
        }

        private void Start()
        {
            _retracePath.Add(_soldier.SpawnPosition);
            _guardDirection = _soldier.AimDirection;
        }

        public override void Update()
        {
            bool shouldRetracePath = true;
            bool shouldConsiderTarget = false;
            if (!_soldier.IsAlive)
            {
                _aiState = States.Guard;
            }
            void TargetOnSight()
            {
                shouldConsiderTarget = true;
                if (_aiType == AITypes.Commander)
                {
                    float radius = Mathf.Sqrt(_commanderPlatoon.Length) / 2.5f;
                    for (int i = 0; i < _commanderPlatoon.Length; i++)
                    {
                        _commanderPlatoon[i].CommandChasePoint(_target.transform.position + (Vector3)Random.insideUnitCircle * radius);
                    }
                }
                else
                {
                    _aiState = States.Chase;
                }
            }
            void StopChase()
            {
                _chaseGuardDirection = _soldier.AimDirection;
                _chaseGuardTime = Time.time;
                _aiState = States.ChaseGuard;
            }
            switch (_aiState)
            {
                case States.Rest:
                    _soldier.SetAimDirection(_guardDirection);
                    break;
                case States.Guard:
                    if (_isTargetVisible)
                    {
                        TargetOnSight();
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
                    shouldRetracePath = false;
                    if (_isTargetVisible)
                    {
                        TargetOnSight();
                    }
                    if (Vector3.Distance(transform.position, PatrolPoints[_patrolIndex]) < 1f)
                    {
                        _patrolIndex = (_patrolIndex + 1) % PatrolPoints.Length;
                    }
                    ConsiderationDirection(PatrolPoints[_patrolIndex], 1f);
                    break;
                case States.ChaseGuard:
                    shouldRetracePath = false;
                    if (_isTargetVisible)
                    {
                        TargetOnSight();
                    }
                    else
                    {
                        const float LOOK_DURATION = 4f;
                        float sinVal = (((Time.time - _chaseGuardTime) / LOOK_DURATION) % 1f) * Mathf.PI * 2f;
                        Quaternion leftRotation = Quaternion.Euler(0, 0, Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(Mathf.Sin(sinVal)))))))) / 0.526f * 45f);
                        _soldier.SetAimDirection(leftRotation * _chaseGuardDirection);
                    }
                    if (Time.time - _chaseGuardTime > CHASE_GUARD_DURATION)
                    {
                        _aiState = States.Retrace;
                    }
                    break;
                case States.Chase:
                    shouldRetracePath = false;
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
                        shouldConsiderTarget = true;
                        if (CanSeePoint(_target.transform.position))
                        {
                            _lastTargetPosition = _target.transform.position;
                        }
                        _isUnreachable = false;
                        _unreachableCheck = Time.time;
                    }
                    else
                    {
                        if (Time.time - _unreachableCheck > UNREACHABLE_CHECK_TIME)
                        {
                            _isUnreachable = !CanSeePoint(_lastTargetPosition, 10);
                            _unreachableCheck = Time.time;
                        }
                        if (Vector2.Distance(transform.position, _lastTargetPosition) < REACH_POINT_DISTANCE || _isUnreachable)
                        {
                            StopChase();
                        } else
                        {
                            ConsiderationDirection(_lastTargetPosition, 1f);
                        }
                    }
                    break;
                case States.OnCommand:
                    shouldRetracePath = false;
                    if (_isTargetVisible)
                    {
                        TargetOnSight();
                    }
                    if (Vector2.Distance(transform.position, _forceMovePosition) < REACH_POINT_DISTANCE)
                    {
                        StopChase();
                    }
                    else
                    {
                        ConsiderationDirection(_forceMovePosition, 1f);
                    }
                    break;
                case States.Retrace:
                    if (_isTargetVisible)
                    {
                        _aiState = States.Chase;
                    } else
                    {
                        if (_retracePath.Count == 1)
                        {
                            _aiState = States.Guard;
                        }
                        else
                        {
                            if (Vector2.Distance(transform.position, _retracePath[_retracePath.Count - 1]) < REACH_POINT_DISTANCE)
                            {
                                _retracePath.RemoveAt(_retracePath.Count - 1);
                            }
                        }
                    }
                    break;
            }
            bool isConsideringMoving = false;
            if (shouldRetracePath)
            {
                if (Vector2.Distance(transform.position, _retracePath[_retracePath.Count - 1]) > REACH_POINT_DISTANCE)
                {
                    ConsiderationDirection(_retracePath[_retracePath.Count - 1], 1f);
                    isConsideringMoving = true;
                }
            }
            if (shouldConsiderTarget)
            {
                ConsiderationKeepDistance(_target.transform.position, 1f, Soldier.GUNDISTANCE);
                isConsideringMoving = true;
                //ConsiderationKeepDistance(_target.transform.position, 1f, 10f);
            }
            if (isConsideringMoving)
            {
                _pathfinder.AddConsideration(_soldier.MoveDirection.normalized, 0.5f);
            }
            base.Update();
        }

        #endregion
        #region Class Methods

        /// <summary>
        /// Helper function turning position into direction relative to entity position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="desiribility"></param>
        private void ConsiderationDirection(Vector3 position, float desiribility)
        {
            _pathfinder.AddConsideration((position - transform.position).normalized, desiribility);
        }

        /// <summary>
        /// Helper function turning position into direction relative to entity position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="desiribility"></param>
        private void ConsiderationKeepDistance(Vector3 position, float desiribility, float distance)
        {
            _pathfinder.AddKeepDistanceConsideration(position - transform.position, desiribility, distance);
        }
        public bool CanSeePoint(Vector3 point, int tries = 1)
        {
            Vector3 directionVector = point - transform.position;
            if (tries == 1)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionVector, directionVector.magnitude, LayerMask.GetMask("Level"));
                return (hit.collider == null);
            }
            for (int i = 0; i < tries; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position + (Vector3)Random.insideUnitCircle, directionVector, directionVector.magnitude, LayerMask.GetMask("Level"));
                if (hit.collider == null)
                {
                    return true;
                }
            }
            return false;
        }

        public void CommandChasePoint(Vector3 point)
        {
            _aiState = States.OnCommand;
            _forceMovePosition = point;
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
                if (_aiState == States.Chase)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position, _lastTargetPosition);
                }
            }
        }

        #endregion
    }
}
