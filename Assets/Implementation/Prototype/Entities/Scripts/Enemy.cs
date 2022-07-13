using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    public class Enemy : Soldier
    {
        #region Variables

        [Header("Dummy Settings")]

        [SerializeField]
        bool _canShoot;

        [SerializeField]
        bool _targetsPlayer;

        [SerializeField]
        bool _runOnSight;

        [SerializeField]
        public Vector3[] PatrolPoints;

        [SerializeField]
        public float debugTest;

        Vector3 _spawnPosition;
        private int _patrolIndex = 0;
        private DirectionalAI _directionalAI;
        private Vector3 _targetPosition;
        private float _retargetTime;
        private const float CONE_DETECTION_CONST = 0.75f;
        private const float CONE_DETECTION_RANGE = 25f;

        #endregion
        #region MonoBehaviour

        public override void Awake()
        {
            base.Awake();
            _spawnPosition = transform.position;
            _directionalAI = GetComponent<DirectionalAI>();
        }
        public override void Update()
        {
            base.Update();
            Vector3 newMoveDirection = Vector3.zero;
            _targetPosition = _spawnPosition;
            SetMoveDirection(Vector3.zero);
            Poolable[] players = InstancePool.GetInstances("Player");
            bool isPlayerVisible = false;
            if (players.Length > 0)
            {
                isPlayerVisible = CanSee(players[0]);
            }
            if (PatrolPoints.Length > 0)
            {
                if (Vector3.Distance(transform.position, PatrolPoints[_patrolIndex]) < 1f)
                {
                    _patrolIndex = (_patrolIndex + 1) % PatrolPoints.Length;
                }
                _targetPosition = PatrolPoints[_patrolIndex];
                newMoveDirection = PatrolPoints[_patrolIndex] - transform.position;
            }
            if (_canShoot)
            {
                if (_targetsPlayer)
                {
                    if (isPlayerVisible)
                    {
                        Vector2 aimDirection = InstancePool.GetInstances("Player")[0].transform.position - transform.position;
                        SetAimDirection(aimDirection);
                        if (IsReloaded)
                        {
                            Attack();
                        }
                    }
                }
                else
                {
                    if (_retargetTime < 0f)
                    {
                        SetAimDirection(Random.insideUnitCircle);
                        _retargetTime = 1f;
                    } else
                    {
                        _retargetTime -= Time.deltaTime;
                    }
                    if (IsReloaded)
                    {
                        Attack();
                    }
                }
            }
            if (_runOnSight)
            {
                if (isPlayerVisible)
                {
                    if (Vector3.Distance(transform.position, players[0].transform.position) < 10f)
                    {
                        newMoveDirection = transform.position - players[0].transform.position;
                        _targetPosition = transform.position + newMoveDirection.normalized;
                        SetAimDirection(players[0].transform.position - transform.position);
                    }
                } else
                {
                    if (Vector3.Distance(transform.position, _spawnPosition) > 5f)
                    {
                        newMoveDirection = _spawnPosition - transform.position;
                        _targetPosition = _spawnPosition;
                        SetAimDirection(_spawnPosition - transform.position);
                    }
                }
            }
            newMoveDirection = _directionalAI.EvaluateDirectionToTarget(transform.position, _targetPosition, newMoveDirection.normalized);
            if (!_canShoot)
            {
                SetAimDirection(newMoveDirection);
            }
            SetMoveDirection(newMoveDirection);
        }
        private void OnDrawGizmosSelected()
        {
            if (_character != null)
            {
                Quaternion rotationVector = Quaternion.Euler(0f, 0f, Mathf.Acos(CONE_DETECTION_CONST) * Mathf.Rad2Deg);
                Vector3 left = rotationVector * _character.transform.right;
                Vector3 right = Quaternion.Inverse(rotationVector) * _character.transform.right;
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(_character.transform.position, _character.transform.position + left * CONE_DETECTION_RANGE);
                Gizmos.DrawLine(_character.transform.position, _character.transform.position + right * CONE_DETECTION_RANGE);
                Gizmos.DrawLine(_character.transform.position, _character.transform.position + _character.transform.right * CONE_DETECTION_RANGE);
                Gizmos.DrawLine(_character.transform.position + left * CONE_DETECTION_RANGE, _character.transform.position + _character.transform.right * CONE_DETECTION_RANGE);
                Gizmos.DrawLine(_character.transform.position + _character.transform.right * CONE_DETECTION_RANGE, _character.transform.position + right * CONE_DETECTION_RANGE);
                //float count = 72;
                //for (int i = 0; i < count; i++)
                //{
                //    Vector3 dir = new Vector3(Mathf.Cos(i * 2f * Mathf.PI / count), Mathf.Sin(i * 2f * Mathf.PI / count), 0f);
                //    if (Vector3.Dot(_character.transform.right, dir) > debugTest)
                //        Gizmos.DrawLine(transform.position, transform.position + dir * 3f);
                //}
            }
        }

        #endregion
        #region Class Methods
        
        private bool CanSee(Poolable poolable)
        {
            Vector3 aimDir = poolable.transform.position - transform.position;
            if (Vector3.Dot(_character.transform.right, aimDir) > CONE_DETECTION_CONST)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, aimDir, CONE_DETECTION_RANGE);
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
        #region Class Overrides

        public override void OnRespawn()
        {
            base.OnRespawn();
            transform.position = _spawnPosition;
        }

        #endregion
    }
}
