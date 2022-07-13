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

        Vector3 _spawnPosition;
        private int _patrolIndex = 0;
        private DirectionalAI _directionalAI;
        private Vector3 _targetPosition;
        private float _retargetTime;

        #endregion
        #region MonoBehaviour

        public override void Awake()
        {
            base.Awake();
            _spawnPosition = transform.position;
            _respawnTime = 3f;
            _directionalAI = GetComponent<DirectionalAI>();
        }
        public override void Update()
        {
            base.Update();
            Vector3 newMoveDirection = Vector3.zero;
            _targetPosition = _spawnPosition;
            SetMoveDirection(Vector3.zero);
            bool isPlayerVisible = false;
            Poolable[] players = InstancePool.GetInstances("Player");
            if (players.Length > 0)
            {
                Vector3 playerAimDir = players[0].transform.position - transform.position;
                RaycastHit2D hit = Physics2D.Raycast(transform.position + playerAimDir.normalized * 1.4f, playerAimDir, GUNDISTANCE);
                if (hit.collider != null)
                {
                    Player player = hit.rigidbody.GetComponent<Player>();
                    if (player != null)
                    {
                        isPlayerVisible = true;
                    }
                }
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
