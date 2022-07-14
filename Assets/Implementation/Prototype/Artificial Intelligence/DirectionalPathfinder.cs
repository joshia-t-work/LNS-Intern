using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.AI
{
    public class DirectionalPathfinder
    {
        #region Variables

        public enum Behaviours
        {
            KeepDistance,
            Stop
        }
        private Behaviours _behaviourOnTargetReach;
        private float _collisionDetectionRange = 2f;
        private float _collisionDetectionSensitivity = 0.3f;

        //List<Consideration> considerations = new List<Consideration>();
        List<Vector3> _considerationDirections = new List<Vector3>();
        List<float> _considerationDesirability = new List<float>();
        Vector3[] _fixedConsiderationDirections = new Vector3[0];
        float[] _fixedConsiderationDesirability = new float[0];
        int _interestCount = 32;
        Vector3[] _interest = new Vector3[0];
        float[] _interestMultiplier = new float[0];
        Vector3 _targetPosition;

        private float keepDistanceSq;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new DirectionalAI
        /// </summary>
        /// <param name="behaviour">Whether returns a direction on reaching target to maintain distance or Vector.zero</param>
        /// <param name="collisionDetectionRange">Distance a collider is detected</param>
        /// <param name="collisionDetectionSensitivity">How much the object avoids collision, 1 means total stop upon detecting collider</param>
        /// <param name="keepDistance">Maintain this amount of distance from target</param>
        public DirectionalPathfinder(Behaviours behaviour, float collisionDetectionRange, float collisionDetectionSensitivity, float keepDistance)
        {
            _behaviourOnTargetReach = behaviour;
            _collisionDetectionRange = collisionDetectionRange;
            _collisionDetectionSensitivity = collisionDetectionSensitivity;
            keepDistanceSq = keepDistance * keepDistance;

            _interest = new Vector3[_interestCount];
            _fixedConsiderationDirections = new Vector3[_interestCount + 2];
            _fixedConsiderationDesirability = new float[_interestCount + 2];
            _fixedConsiderationDesirability[0] = 0.5f;
            for (int i = 0; i < _interestCount; i++)
            {
                _interest[i] = new Vector3(Mathf.Cos((float)i / _interestCount * Mathf.PI * 2), Mathf.Sin((float)i / _interestCount * Mathf.PI * 2), 0);
                _fixedConsiderationDirections[2 + i] = -_interest[i];
            }
        }

        #endregion
        #region Class Methods

        /// <summary>
        /// Returns normalized evaluated direction.
        /// </summary>
        /// <param name="targetPosition">Position to move to</param>
        /// <param name="movedir">Current normalized movement direction</param>
        /// <returns></returns>
        public Vector3 EvaluateDirectionToTarget(Vector3 fromPosition, Vector3 targetPosition, Vector3 movedir)
        {
            _targetPosition = targetPosition;
            _fixedConsiderationDirections[0] = movedir;

            Vector3 targetVec;
            targetVec = (_targetPosition - fromPosition);
            float distSq = Vector3.SqrMagnitude(targetVec);
            targetVec = targetVec.normalized;
            switch (_behaviourOnTargetReach)
            {
                case Behaviours.KeepDistance:
                    if (distSq < keepDistanceSq)
                    {
                        _fixedConsiderationDirections[1] = targetVec * (2f * distSq / keepDistanceSq - 1f);
                        _fixedConsiderationDesirability[1] = Mathf.Abs(distSq / keepDistanceSq);
                    }
                    else
                    {
                        _fixedConsiderationDirections[1] = targetVec;
                        _fixedConsiderationDesirability[1] = 1f;
                    }
                    break;
                case Behaviours.Stop:
                    if (distSq < keepDistanceSq)
                    {
                        return Vector3.zero;
                    }
                    else
                    {
                        _fixedConsiderationDirections[1] = targetVec;
                        _fixedConsiderationDesirability[1] = 1f;
                    }
                    break;
                default:
                    break;
            }

            _interestMultiplier = new float[_interestCount];
            for (int i = 0; i < _interestCount; i++)
            {
                _interestMultiplier[i] = 1f;
            }

            for (int i = 0; i < _interestCount; i++)
            {
                _fixedConsiderationDesirability[2 + i] = _collisionDetectionSensitivity;
            }
            for (int i = 0; i < _interestCount; i++)
            {
                _fixedConsiderationDesirability[2 + i] = 0f;
                //RaycastHit2D[] hits = Physics2D.RaycastAll(fromPosition, interest[i], collisionDetectionRange);
                //for (int j = 0; j < hits.Length; j++)
                //{
                //    Rigidbody2D collider = hits[j].rigidbody;
                //    if (collider == rb)
                //    {
                //        continue;
                //    }
                //    fixedConsiderationDesirability[2 + i] = collisionDetectionSensitivity;
                //    interestMultiplier[i] = 0;
                //    break;
                //}
                RaycastHit2D hit = Physics2D.Raycast(fromPosition, _interest[i], _collisionDetectionRange);
                if (hit.collider != null)
                {
                    _fixedConsiderationDesirability[2 + i] = _collisionDetectionSensitivity;
                    _interestMultiplier[i] = 0;
                }
            }

            for (int i = 0; i < _interestCount + 2; i++)
            {
                if (_fixedConsiderationDesirability[i] == 0)
                {
                    continue;
                }
                targetVec = _fixedConsiderationDirections[i];
                for (int ii = 0; ii < _interestCount; ii++)
                {
                    _interestMultiplier[ii] *= 1f + _fixedConsiderationDesirability[i] * (Vector3.Dot(_interest[ii], targetVec) / 2f - 0.5f);
                }
            }
            for (int i = 0; i < _considerationDirections.Count; i++)
            {
                targetVec = _considerationDirections[i];
                for (int ii = 0; ii < _interestCount; ii++)
                {
                    _interestMultiplier[ii] *= 1f + _considerationDesirability[i] * (Vector3.Dot(_interest[ii], targetVec) / 2f - 0.5f);
                }
            }

            int maxInterest = 0;
            for (int i = 0; i < _interest.Length; i++)
            {
                if (_interestMultiplier[i] > _interestMultiplier[maxInterest])
                {
                    maxInterest = i;
                }
            }
            ClearConsiderations();
            return _interest[maxInterest];
        }

        public void ClearConsiderations()
        {
            _considerationDirections.Clear();
            _considerationDesirability.Clear();
        }

        /// <summary>
        /// Add a consideration vector, weighted by desiribility
        /// </summary>
        /// <param name="direction">Direction of desire to move toward, must be normalized</param>
        /// <param name="desiribility">Amount of desire to move towards</param>
        public void AddConsideration(Vector3 direction, float desiribility)
        {
            if (desiribility == 0)
            {
                return;
            }
            _considerationDirections.Add(direction);
            _considerationDesirability.Add(desiribility);
        }

        #endregion
        #region Debugging

        public void DebugGizmos(Transform transform)
        {
            if (_interestMultiplier.Length > 0)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_targetPosition, 0.1f);
                Gizmos.DrawLine(transform.position + (_targetPosition - transform.position).normalized, _targetPosition);
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, 1f);
                for (int i = 0; i < _considerationDirections.Count; i++)
                {
                    if (_considerationDesirability[i] < 0)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }
                    Gizmos.DrawSphere(transform.position + _considerationDirections[i] * 1.1f * _considerationDesirability[i], 0.05f);
                }

                for (int i = 0; i < _fixedConsiderationDirections.Length; i++)
                {
                    if (_fixedConsiderationDesirability[i] < 0)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }
                    Gizmos.DrawSphere(transform.position + _fixedConsiderationDirections[i] * 1.1f * _fixedConsiderationDesirability[i], 0.05f);
                }

                int maxInterest = 0;
                for (int i = 0; i < _interest.Length; i++)
                {
                    if (_interestMultiplier[i] < 0)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }
                    if (_interestMultiplier[i] > _interestMultiplier[maxInterest])
                    {
                        maxInterest = i;
                    }
                    Gizmos.DrawLine(transform.position, transform.position + _interest[i] * Mathf.Abs(_interestMultiplier[i]));
                }
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + _interest[maxInterest] * Mathf.Abs(_interestMultiplier[maxInterest]));
            }
        }

        #endregion
    }
}