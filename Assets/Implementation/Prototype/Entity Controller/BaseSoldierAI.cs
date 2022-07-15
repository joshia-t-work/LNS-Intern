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
    public abstract class BaseSoldierAI : MonoBehaviour
    {
        #region Variables

        [Header("AI Settings")]

        [SerializeField]
        protected Soldier _soldier;

        [SerializeField]
        public Vector3[] PatrolPoints;

        [SerializeField]
        private FireBehaviour _aimBehaviour;

        [SerializeField]
        private string _targetType;

        public enum FireBehaviour
        {
            Hold,
            AlwaysFire,
            FireAtWill,
            FireOnCommand,
        }

        private Vector3 _targetPosition;
        private float _randomRetargetTime;

        protected DirectionalPathfinder _pathfinder = new DirectionalPathfinder(DirectionalPathfinder.Behaviours.KeepDistance, 2f, 0.5f, 15f);
        protected const float CONE_DETECTION_CONST = 0.75f;
        protected const float CONE_DETECTION_RANGE = 25f;
        protected const float REACH_POINT_DISTANCE = 1f;
        protected const float RANDOM_RETARGET_TIME = 1f;
        protected bool _isTargetVisible { get; private set; }
        protected Poolable _target { get; private set; }

        #endregion
        #region MonoBehaviour

        public virtual void Update()
        {
            // If it does not target anything, aim into a random direction
            Vector2 aimDirection = _soldier.TargetAimDirection;
            _target = null;
            if (_targetType == "")
            {
                _isTargetVisible = false;
                if (Time.time - _randomRetargetTime > RANDOM_RETARGET_TIME)
                {
                    aimDirection = Random.insideUnitCircle;
                    _randomRetargetTime = Time.time;
                }
            } else
            {
                Poolable[] targets = InstancePool.GetInstances(_targetType);
                _isTargetVisible = false;
                aimDirection = _soldier.MoveDirection;
                if (targets.Length > 0)
                {
                    _target = targets[0];
                    _isTargetVisible = CanSee(targets[0]);
                    if (_isTargetVisible)
                    {
                        aimDirection = targets[0].transform.position - transform.position;
                    }
                }
            }
            // Try to fire when target is visible or if it is aiming randomly
            bool shouldFire = _isTargetVisible || (_targetType == "");
            if (shouldFire)
            {
                switch (_aimBehaviour)
                {
                    case FireBehaviour.Hold:
                        if (!_isTargetVisible)
                        {
                            aimDirection = _soldier.MoveDirection;
                        }
                        break;
                    case FireBehaviour.AlwaysFire:
                        //if (Time.time - _randomRetargetTime > RANDOM_RETARGET_TIME)
                        //{
                        //    _soldier.SetAimDirection(Random.insideUnitCircle);
                        //    _randomRetargetTime = Time.time;
                        //}
                        if (_soldier.IsReloaded)
                        {
                            _soldier.Attack();
                        }
                        break;
                    case FireBehaviour.FireAtWill:
                        if (_soldier.IsReloaded)
                        {
                            _soldier.Attack();
                        }
                        break;
                    case FireBehaviour.FireOnCommand:
                        break;
                    default:
                        break;
                }
            }
            _soldier.SetAimDirection(aimDirection);
            _soldier.SetMoveDirection(_pathfinder.EvaluateDirectionToTarget(transform.position, _targetPosition, _soldier.MoveDirection.normalized));
        }

        #endregion
        #region Debugging

        public virtual void OnDrawGizmosSelected()
        {
            _pathfinder.DebugGizmos(transform);
            Quaternion rotationVector = Quaternion.Euler(0f, 0f, Mathf.Acos(CONE_DETECTION_CONST) * Mathf.Rad2Deg);
            Vector3 left = rotationVector * _soldier.AimDirection;
            Vector3 right = Quaternion.Inverse(rotationVector) * _soldier.AimDirection;
            Gizmos.color = Color.gray;
            if (_isTargetVisible)
            {
                Gizmos.color = Color.black;
            }
            Gizmos.DrawLine(_soldier.transform.position, _soldier.transform.position + left * CONE_DETECTION_RANGE);
            Gizmos.DrawLine(_soldier.transform.position, _soldier.transform.position + right * CONE_DETECTION_RANGE);
            Gizmos.DrawLine(_soldier.transform.position, _soldier.transform.position + (Vector3)_soldier.AimDirection * CONE_DETECTION_RANGE);
            Gizmos.DrawLine(_soldier.transform.position + left * CONE_DETECTION_RANGE, _soldier.transform.position + (Vector3)_soldier.AimDirection * CONE_DETECTION_RANGE);
            Gizmos.DrawLine(_soldier.transform.position + (Vector3)_soldier.AimDirection * CONE_DETECTION_RANGE, _soldier.transform.position + right * CONE_DETECTION_RANGE);
        }

        #endregion
        #region Class Methods

        protected void SetTargetPosition(Vector3 position)
        {
            _targetPosition = position;
        }
        protected bool CanSee(Poolable poolable)
        {
            Vector3 aimDir = poolable.transform.position - _soldier.transform.position;
            if (Vector3.Dot(_soldier.AimDirection.normalized, aimDir.normalized) > CONE_DETECTION_CONST)
            {
                RaycastHit2D hit = Physics2D.Raycast(_soldier.transform.position, aimDir, CONE_DETECTION_RANGE);
                if (hit.collider != null)
                {
                    if (poolable.transform == hit.transform)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion
    }
}
