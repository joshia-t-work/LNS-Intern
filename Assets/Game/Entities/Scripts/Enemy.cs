using LNS.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    public class Enemy : Soldier
    {
        private float _sightLocked = 0f;
        private int _sightLockState = 0;
        [Header("Dummy Settings")]
        [SerializeField] bool _canShoot;
        [SerializeField] bool _runOnSight;
        [SerializeField] Vector3[] _patrolPoints;
        Vector3 _spawnPosition;
        private int _patrolIndex = 0;
        public override void Awake()
        {
            base.Awake();
            _spawnPosition = transform.position;
            _respawnTime = 3f;
        }
        private void Update()
        {
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
            _sightLocked -= Time.deltaTime;
            //Vector2 inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            //SetMoveDirection(inputDirection);
            if (_patrolPoints.Length > 0)
            {
                if (Vector3.Distance(transform.position, _patrolPoints[_patrolIndex]) < 1f)
                {
                    _patrolIndex = (_patrolIndex + 1) % _patrolPoints.Length;
                }
                SetMoveDirection(_patrolPoints[_patrolIndex] - transform.position);
                if (!_canShoot)
                {
                    SetAimDirection(_patrolPoints[_patrolIndex] - transform.position);
                }
            }
            if (isPlayerVisible)
            {
                if (_canShoot)
                {
                    if (_sightLocked < 0f)
                    {
                        _sightLockState = (_sightLockState % 3) + 1;
                        _sightLocked = 1f;
                        if (_sightLockState == 3)
                        {
                            Fire();
                        }
                    }
                    switch (_sightLockState)
                    {
                        case 1:
                            Vector2 aimDirection = InstancePool.GetInstances("Player")[0].transform.position - transform.position;
                            SetAimDirection(aimDirection);
                            break;
                        case 2:
                            SetGunSight(true);
                            break;
                        case 3:
                            SetGunSight(false);
                            break;
                        default:
                            break;
                    }
                }
            }
            if (_runOnSight)
            {
                if (isPlayerVisible)
                {
                    if (Vector3.Distance(transform.position, players[0].transform.position) < 10f)
                    {
                        SetMoveDirection(transform.position - players[0].transform.position);
                        SetAimDirection(players[0].transform.position - transform.position);
                    }
                } else
                {
                    if (Vector3.Distance(transform.position, _spawnPosition) > 5f)
                    {
                        SetMoveDirection(_spawnPosition - transform.position);
                        SetAimDirection(_spawnPosition - transform.position);
                    }
                }
            }
        }
        public override void OnRespawn()
        {
            base.OnRespawn();
            transform.position = _spawnPosition;
        }
    }
}
