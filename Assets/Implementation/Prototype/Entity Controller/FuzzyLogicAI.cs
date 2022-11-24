using LNS.AI;
using LNS.FuzzyLogic;
using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace LNS.Entities
{
    public class FuzzyLogicAI : BaseSoldierAI
    {
        [SerializeField]
        Ball ball;
        public bool HoldBall;
        Vector2 holdBallPos;

        private Coroutine FindTargetCoroutine = null;

        private static bool isInitialized = false;
        private static FuzzyTruth Distance = new FuzzyTruth();
        private static FuzzyTruth HealthPercent = new FuzzyTruth();
        private static FuzzySection DistanceVeryNear = new FuzzySection("Very Near", 0f);
        private static FuzzySection DistanceNear = new FuzzySection("Near", 5f);
        private static FuzzySection DistanceMedium = new FuzzySection("Medium", 15f);
        private static FuzzySection DistanceFar = new FuzzySection("Far", 0f);
        private static FuzzySection DistanceVeryFar = new FuzzySection("Very Far", 100000f);
        private static FuzzySection HealthVeryLow = new FuzzySection("Very Low", 0f);
        private static FuzzySection HealthLow = new FuzzySection("Low", 0f);
        private static FuzzySection HealthMedium = new FuzzySection("Medium", 0f);
        private static FuzzySection HealthHigh = new FuzzySection("High", 0f);
        private static FuzzySection HealthVeryHigh = new FuzzySection("Very High", 0f);
        private const float FUZZY_THRES = 0.7f;

        //#region Variables

        //[Header("Enemy Settings")]

        //[SerializeField]
        //AITypes _aiType;

        //[SerializeField]
        //AIEnemy[] _commanderPlatoon;

        //private int _patrolIndex = 0;
        //public enum AITypes
        //{
        //    DoNothing,
        //    Default,
        //    Commander,
        //}
        //private List<Vector3> _retracePath = new List<Vector3>();
        //private Vector2 _guardDirection;
        //private Vector2 _chaseGuardDirection;
        //private Vector2 _lastTargetPosition;
        //private Vector2 _forceMovePosition;
        //private States _aiState = States.Rest;
        //private float _chaseGuardTime;
        //private const float CHASE_GUARD_DURATION = 4f;
        //private const float UNREACHABLE_CHECK_TIME = 2f;
        //private bool _isUnreachable;
        //private float _unreachableCheck;
        //protected enum States
        //{
        //    Rest,
        //    Guard,
        //    Patrol,
        //    Chase,
        //    ChaseGuard,
        //    OnCommand,
        //    Retrace,
        //}

        //#endregion
        //#region MonoBehaviour

        public override void Awake()
        {
            base.Awake();
            _pathfinder = new DirectionalPathfinder(1.5f, 0.5f, 0f);
            if (!isInitialized)
            {
                isInitialized = true;
                Distance.Add(DistanceVeryNear, 10f);
                Distance.Add(DistanceNear, 10f);
                Distance.Add(DistanceMedium, 20f);
                Distance.Add(DistanceFar, 30f);
                Distance.Add(DistanceVeryFar, 0f);
                HealthPercent.Add(HealthVeryLow, 0.2f);
                HealthPercent.Add(HealthLow, 0.2f);
                HealthPercent.Add(HealthMedium, 0.2f);
                HealthPercent.Add(HealthHigh, 0.2f);
                HealthPercent.Add(HealthVeryHigh, 0.2f);
            }
        }

        //private void Start()
        //{
        //    _retracePath.Add(_soldier.SpawnPosition);
        //    _guardDirection = _soldier.AimDirection;
        //}

        public override void Update()
        {
            bool isConsideringMoving = false;

            float isMeLowHealth = HealthPercent.Evaluate((float)_soldier.Health.Value / _soldier.MaxHealth.Value, HealthVeryLow, HealthLow);
            Vector3 myPos = transform.position;
            float maxPriority = -1;
            Entity selectedTarget = null;
            List<Poolable> targets = InstancePool.GetInstances(_targetType);
            if (ball.owner == null)
            {
                targets.Add(ball);
            }
            Entity entity;
            for (int i = 0; i < targets.Count; i++)
            {
                entity = (Entity)targets[i];
                if (entity != null)
                {
                    float priority = 0f;
                    bool isGoal = entity is Goal;
                    bool isBall = (entity == ball);
                    float distanceToEntity = Vector3.Distance(myPos, entity.transform.position);
                    float isCloseNearDistance = Distance.Evaluate(distanceToEntity, "Very Near", "Near");
                    if (entity.Team != _soldier.Team)
                    {
                        //bool isGoal = entity is Goal;
                        //bool isBall = (entity == ball);
                        //float distanceToEntity = Vector3.Distance(myPos, entity.transform.position);

                        float isClose = Distance.Evaluate(distanceToEntity, DistanceVeryNear);
                        float isNear = Distance.Evaluate(distanceToEntity, DistanceNear);
                        float isMedium = Distance.Evaluate(distanceToEntity, DistanceMedium);
                        float isFar = Distance.Evaluate(distanceToEntity, DistanceFar);
                        float isVeryFar = Distance.Evaluate(distanceToEntity, DistanceVeryFar);

                        float isLowHealth = HealthPercent.Evaluate((float)entity.Health.Value / entity.MaxHealth.Value, HealthVeryLow, HealthLow);
                        priority = OP.AND(OP.NOT(isBall), OP.NOT(isGoal), isMeLowHealth, OP.OR(isClose, isNear, isMedium)) * 0.5f;
                        priority = OP.OR(priority, OP.AND(OP.NOT(isMeLowHealth), OP.NOT(isGoal), OP.NOT(isBall), OP.NOT(HoldBall), isLowHealth, OP.OR(isClose, isNear, isMedium))) * 0.5f;
                        priority = OP.OR(priority, OP.AND(OP.NOT(isBall), OP.BOOL(HoldBall), OP.BOOL(isGoal))) * 1f;
                        priority = OP.OR(priority, OP.AND(OP.NOT(isMeLowHealth), OP.NOT(isBall), OP.BOOL(HoldBall), OP.OR(isClose, isNear))) * 0.5f;
                        if (entity.TryGetComponent(out FuzzyLogicAI ai))
                        {
                            priority = OP.OR(priority, OP.AND(OP.NOT(isMeLowHealth), OP.BOOL(ai.HoldBall))) * 0.55f;
                        }
                        if (HoldBall && isGoal && (distanceToEntity < 2f))
                        {
                            ball.Drop(this);
                            ball.Kill();
                            Soccer.goals[_soldier.Team] += 1;
                        }
                        float ballmult = 1f;
                        if (isBall)
                        {
                            ballmult = 0.3f;
                            priority += isClose * 0.5f;
                        }
                        priority += isClose * 0.25f * ballmult;
                        priority += isNear * 0.125f * ballmult;
                        priority += isMedium * 0.05f * ballmult;
                        priority += isFar * 0.025f * ballmult;
                        priority += isVeryFar * 0f * ballmult;
                    }
                    else
                    {
                        priority = OP.AND(OP.BOOL(HoldBall), OP.NOT(isBall), OP.NOT(isGoal), isMeLowHealth, isCloseNearDistance) * 1f;
                    }
                    if (priority > maxPriority)
                    {
                        maxPriority = priority;
                        selectedTarget = entity;
                    }
                }
            }
            if (ball.owner == null)
            {
                targets.Remove(ball);
            }
            if (selectedTarget != null)
            {
                SetTarget(selectedTarget);
            }

            //ConsiderationDirection(_retracePath[_retracePath.Count - 1], 1f);
            isConsideringMoving = true;
            //if (shouldConsiderTarget)
            //{
            //    ConsiderationKeepDistance(_target.transform.position, 1f, Soldier.GUNDISTANCE);
            //    isConsideringMoving = true;
            //    //ConsiderationKeepDistance(_target.transform.position, 1f, 10f);
            //}
            if (isConsideringMoving)
            {
                _pathfinder.AddConsideration(_soldier.MoveDirection.normalized, 0.5f);
            }
            if (_target != null)
            {
                if (!(_target is Goal) && (isMeLowHealth > FUZZY_THRES))
                {
                    ConsiderationKeepDistance(_target.transform.position, 1f, Soldier.GUNDISTANCE * 1.2f);
                }
                else
                {
                    if ((_target is Ball) || (_target is Goal) || (((Soldier)_target).Team == _soldier.Team))
                    {
                        ConsiderationDirection(_target.transform.position, 1f);
                    }
                    else
                    {
                        ConsiderationKeepDistance(_target.transform.position, 1f, Soldier.GUNDISTANCE * 0.7f);
                    }
                }
            }

            base.Update();
        }

        //private IEnumerator FindTarget()
        //{
        //    FindTargetCoroutine = null;
        //}

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.transform == ball.transform)
            {
                ball.Hold(this);
            }
        }

        //#endregion
        //#region Class Methods

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

        //public void CommandChasePoint(Vector3 point)
        //{
        //    _aiState = States.OnCommand;
        //    _forceMovePosition = point;
        //}

        //#endregion
        //#region Debugging

        public override void OnDrawGizmosSelected()
        {
            if (DEBUGSETTINGS.DETAIL > 0)
            {
                _pathfinder.DebugGizmos(_soldier.transform);
            }

            base.OnDrawGizmosSelected();

            if (DEBUGSETTINGS.DETAIL > 2)
            {
                float maxPriority = -1;
                Entity selectedTarget = null;
                float isMeLowHealth = HealthPercent.Evaluate((float)_soldier.Health.Value / _soldier.MaxHealth.Value, "Very Low", "Low");
                List<Poolable> targets = new List<Poolable>(InstancePool.GetInstances(_targetType));
                targets.Add(ball);
                for (int i = 0; i < targets.Count; i++)
                {
                    Entity entity = (Entity)targets[i];
                    if (entity != null)
                    {
                        if (entity.Team != _soldier.Team)
                        {
                            float distanceToEntity = Vector3.Distance(transform.position, entity.transform.position);
                            float isCloseNearMediumDistance = Distance.Evaluate(distanceToEntity, "Very Near", "Near", "Medium");
                            //float isNearMediumDistance = Distance.EvaluateOr(distanceToEntity, "Near", "Medium");
                            //float isMediumDistance = Distance.Evaluate(distanceToEntity, "Medium");
                            float isLowHealth = HealthPercent.Evaluate((float)entity.Health.Value / entity.MaxHealth.Value, "Very Low", "Low");
                            float priority;
                            priority = OP.AND(isMeLowHealth, isCloseNearMediumDistance);
                            priority = OP.OR(priority, OP.AND(OP.NOT(isMeLowHealth), isLowHealth, isCloseNearMediumDistance));
                            priority += Distance.Evaluate(distanceToEntity, "Very Near") * 0.5f;
                            priority += Distance.Evaluate(distanceToEntity, "Near") * 0.25f;
                            priority += Distance.Evaluate(distanceToEntity, "Medium") * 0.1f;
                            priority += Distance.Evaluate(distanceToEntity, "Far") * 0.05f;
                            priority += Distance.Evaluate(distanceToEntity, "Very Far") * 0f;
                            DrawBar(entity.transform.position + Vector3.down * 1f, isLowHealth);
                            DrawBar(entity.transform.position + Vector3.down * 2f, isCloseNearMediumDistance);
                            DrawBar(entity.transform.position + Vector3.down * 3f, priority);
                            GUIStyle style = new GUIStyle();
                            style.normal.textColor = Color.black;
                            Handles.Label(entity.transform.position + Vector3.right * 5f, $"Low Health: {isLowHealth}\nClose Distance: {isCloseNearMediumDistance}\nPriority: {priority}", style);
                            if (priority > maxPriority)
                            {
                                maxPriority = priority;
                                selectedTarget = entity;
                            }
                        }
                    }
                }
            }
        }

        private void DrawBar(Vector3 pos, float percent)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(pos + Vector3.right * 1.5f, Vector2.one + Vector2.right * 3f);
            Gizmos.color = Color.white;
            Gizmos.DrawCube(pos + Vector3.right * 1.5f * percent, Vector2.one + Vector2.right * percent * 3f);
        }

        //#endregion
    }
}
