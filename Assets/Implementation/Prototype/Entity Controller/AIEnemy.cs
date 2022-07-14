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
        private int _patrolIndex = 0;
        public enum AITypes
        {
            Default,
        }
        private States _aiState = States.Guard;
        protected enum States
        {
            Guard,
            Patrol,
            Chase
        }

        #endregion
        #region MonoBehaviour
        public override void Update()
        {
            base.Update();
            Vector3 _targetPosition = _soldier.SpawnPosition;
            if (PatrolPoints.Length > 0)
            {
                if (Vector3.Distance(transform.position, PatrolPoints[_patrolIndex]) < 1f)
                {
                    _patrolIndex = (_patrolIndex + 1) % PatrolPoints.Length;
                }
                _targetPosition = PatrolPoints[_patrolIndex];
            }
            SetTargetPosition(_targetPosition); 
        }

        #endregion
    }
}
