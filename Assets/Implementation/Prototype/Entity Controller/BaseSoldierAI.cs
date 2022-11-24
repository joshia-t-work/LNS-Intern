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
        public Soldier _soldier;

        [SerializeField]
        public Vector3[] PatrolPoints;

        [SerializeField]
        private AimBehaviour _aimBehaviour;

        [SerializeField]
        private TargetBehaviour _targetBehaviour;

        [SerializeField]
        protected string _targetType;

        public enum AimBehaviour
        {
            DoNotAim,
            AimForwards,
            FireAtWill,
        }

        public enum TargetBehaviour
        {
            AlwaysSearch,
            DoNotSearch,
        }

        private float _randomRetargetTime;

        protected DirectionalPathfinder _pathfinder = new DirectionalPathfinder(1.5f, 0.5f, 0.5f);
        protected const float CONE_DETECTION_CONST = 0.75f;
        protected const float CONE_DETECTION_RANGE = 25f;
        protected const float REACH_POINT_DISTANCE = 1f;
        protected const float RANDOM_RETARGET_TIME = 1f;
        protected bool _isTargetVisible { get; private set; }
        protected Poolable _target { get; private set; } = null;

        #endregion
        #region MonoBehaviour

        public virtual void Awake()
        {
            _soldier.SetAimDirection(_soldier.AimDirection);
        }
        public virtual void Update()
        {
            // If it does not target anything, aim into a random direction
            Vector2 aimDirection = _soldier.TargetAimDirection;
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
                _isTargetVisible = false;
                if (_soldier.MoveDirection != Vector2.zero)
                {
                    aimDirection = _soldier.MoveDirection;
                }
                if (_targetBehaviour == TargetBehaviour.AlwaysSearch)
                {
                    List<Poolable> targets = InstancePool.GetInstances(_targetType);
                    for (int i = 0; i < targets.Count; i++)
                    {
                        Entity entity = (Entity)targets[i];
                        if (entity != null)
                        {
                            if (entity.Team != _soldier.Team)
                            {
                                _target = entity;
                                break;
                            }
                        }
                    }
                }
                if (_target != null)
                {
                    _isTargetVisible = CanSee(_target);
                    if (_isTargetVisible)
                    {
                        aimDirection = _target.transform.position - transform.position;
                        // prediction
                        Vector3 predictedMove = Random.Range(1f, 2f) * ((Entity)_target).Rb.velocity * (aimDirection.magnitude / Bullet.MOVE_SPEED);
                        aimDirection = _target.transform.position + predictedMove - transform.position;
                    }
                }
            }
            // Try to fire when target is visible or if it is aiming randomly
            bool shouldFire = _isTargetVisible || (_targetType == "");
            if (shouldFire)
            {
                switch (_aimBehaviour)
                {
                    case AimBehaviour.DoNotAim:
                        aimDirection = _soldier.TargetAimDirection;
                        break;
                    case AimBehaviour.AimForwards:
                        aimDirection = _soldier.MoveDirection;
                        break;
                    case AimBehaviour.FireAtWill:
                        if (_soldier.IsReloaded)
                        {
                            _soldier.Attack();
                        }
                        break;
                    default:
                        break;
                }
            }
            _soldier.SetAimDirection(aimDirection);
            _soldier.SetMoveDirection(_pathfinder.EvaluateDirectionToTarget(transform.position));
        }

        #endregion
        #region Debugging

        public virtual void OnDrawGizmosSelected()
        {
            if (DEBUGSETTINGS.DETAIL > 0)
            {
                //_pathfinder.DebugGizmos(transform);
            }
            if (DEBUGSETTINGS.DETAIL > 1)
            {
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
            if (DEBUGSETTINGS.DETAIL > 0)
            {
                if (_target != null)
                {
                    if (CanSee(_target))
                    {
                        Gizmos.color = Color.red;
                    } else
                    {
                        Gizmos.color = Color.green;
                    }
                    Gizmos.DrawLine(_soldier.transform.position, _target.transform.position);
                }
            }
        }

        #endregion
        #region Class Methods

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
        public void SetTarget(Poolable poolable)
        {
            _target = poolable;
        }

        #endregion
    }
}
