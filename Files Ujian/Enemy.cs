using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Enemy : Entity
    {
        public override InstancePool.PoolableType poolableType => InstancePool.PoolableType.Enemy;
        private float sightLocked = 0f;
        private int sightLockState = 0;
        [Header("Dummy Settings")]
        [SerializeField] bool canShoot;
        [SerializeField] bool runOnSight;
        [SerializeField] Vector3[] patrolPoints;
        Vector3 spawnPosition;
        private int patrolIndex = 0;
        public override void Awake()
        {
            base.Awake();
            spawnPosition = transform.position;
            respawnTime = 3f;
        }
        private void Update()
        {
            SetMoveDirection(Vector3.zero);
            bool isPlayerVisible = false;
            Poolable[] players = InstancePool.GetInstances(InstancePool.PoolableType.Player);
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
            sightLocked -= Time.deltaTime;
            //Vector2 inputDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            //SetMoveDirection(inputDirection);
            if (patrolPoints.Length > 0)
            {
                if (Vector3.Distance(transform.position, patrolPoints[patrolIndex]) < 1f)
                {
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                }
                SetMoveDirection(patrolPoints[patrolIndex] - transform.position);
                if (!canShoot)
                {
                    SetAimDirection(patrolPoints[patrolIndex] - transform.position);
                }
            }
            if (isPlayerVisible)
            {
                if (canShoot)
                {
                    if (sightLocked < 0f)
                    {
                        sightLockState = (sightLockState % 3) + 1;
                        sightLocked = 1f;
                        if (sightLockState == 3)
                        {
                            Fire();
                        }
                    }
                    switch (sightLockState)
                    {
                        case 1:
                            Vector2 aimDirection = InstancePool.GetInstances(InstancePool.PoolableType.Player)[0].transform.position - transform.position;
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
            if (runOnSight)
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
                    if (Vector3.Distance(transform.position, spawnPosition) > 5f)
                    {
                        SetMoveDirection(spawnPosition - transform.position);
                        SetAimDirection(spawnPosition - transform.position);
                    }
                }
            }
        }
        public override void OnRespawn()
        {
            base.OnRespawn();
            transform.position = spawnPosition;
        }
    }
}
