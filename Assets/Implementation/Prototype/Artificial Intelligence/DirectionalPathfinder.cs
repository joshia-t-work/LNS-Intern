using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.AI
{
    public class DirectionalPathfinder
    {
        #region Variables

        private float _collisionDetectionRange = 2f;
        private float _collisionDetectionSensitivity = 0.3f;

        List<Vector3> _gizmosConsiderationDirections = new List<Vector3>();
        List<float> _gizmosConsiderationDesirability = new List<float>();
        List<Vector3> _considerationDirections = new List<Vector3>();
        List<float> _considerationDesirability = new List<float>();
        Vector3[] _fixedConsiderationDirections = new Vector3[0];
        float[] _fixedConsiderationDesirability = new float[0];
        const int _interestCount = 32;
        Vector3[] _interest = new Vector3[0];
        float[] _interestMultiplier = new float[0];
        Vector3 _targetPosition;
        Vector3 _targetVec;

        const float KEEP_DIST_MIN = 0.4f;
        const float KEEP_DIST_MAX = 0.8f;
        private float _keepDistance;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new DirectionalAI
        /// </summary>
        /// <param name="behaviour">Whether returns a direction on reaching target to maintain distance or Vector.zero</param>
        /// <param name="collisionDetectionRange">Distance a collider is detected</param>
        /// <param name="collisionDetectionSensitivity">How much the object avoids collision, 1 means total stop upon detecting collider (avoid collision at all cost)</param>
        /// <param name="keepDistance">Maintain this amount of distance from target</param>
        public DirectionalPathfinder(float collisionDetectionRange, float collisionDetectionSensitivity, float keepDistance)
        {
            _collisionDetectionRange = collisionDetectionRange;
            _collisionDetectionSensitivity = collisionDetectionSensitivity;
            _keepDistance = keepDistance;

            _interest = new Vector3[_interestCount];
            _interestMultiplier = new float[_interestCount];
            _fixedConsiderationDirections = new Vector3[_interestCount];
            _fixedConsiderationDesirability = new float[_interestCount];
            for (int i = 0; i < _interestCount; i++)
            {
                _interest[i] = new Vector3(Mathf.Cos((float)i / _interestCount * Mathf.PI * 2), Mathf.Sin((float)i / _interestCount * Mathf.PI * 2), 0);
                _fixedConsiderationDirections[i] = -_interest[i];
            }
        }

        #endregion
        #region Class Methods

        public void SetKeepDistance(float keepDistance)
        {
            _keepDistance = keepDistance;
        }

        /// <summary>
        /// Returns normalized evaluated direction. Adding current movement direction as consideration.
        /// </summary>
        /// <param name="targetPosition">Position to move to</param>
        /// <param name="movedir">Current normalized movement direction</param>
        /// <returns></returns>
        public Vector3 EvaluateDirectionToTarget(Vector3 fromPosition, Vector3 targetPosition, Vector3 movedir)
        {
            _targetPosition = targetPosition;
            AddConsideration(movedir, 0.5f);

            _targetVec = (_targetPosition - fromPosition);
            float dist = _targetVec.magnitude;
            _targetVec = _targetVec.normalized;
            AddKeepDistanceConsideration(_targetVec, 1f, _keepDistance);

            return evaluateDirectionToTarget(fromPosition);
        }

        /// <summary>
        /// Returns normalized evaluated direction.
        /// </summary>
        /// <returns></returns>
        public Vector3 EvaluateDirectionToTarget(Vector3 fromPosition)
        {
            return evaluateDirectionToTarget(fromPosition);
        }

        /// <summary>
        /// Returns normalized evaluated direction.
        /// </summary>
        /// <param name="targetPosition">Position to move to</param>
        /// <returns></returns>
        private Vector3 evaluateDirectionToTarget(Vector3 fromPosition)
        {
            if (_considerationDesirability.Count == 0)
            {
                return Vector3.zero;
            }

            for (int i = 0; i < _interestCount; i++)
            {
                _interestMultiplier[i] = 1f;
            }

            for (int i = 0; i < _interestCount; i++)
            {
                _fixedConsiderationDesirability[i] = 0f;
                int layerMask = ~LayerMask.GetMask("Ball");
                bool hit = Physics.Raycast(fromPosition, _interest[i], _collisionDetectionRange, layerMask);
                if (hit)
                {
                    _fixedConsiderationDesirability[i] = _collisionDetectionSensitivity;
                    //_interestMultiplier[i] = 0;
                    //AddConsideration()
                }
            }

            Vector3 currentVec;
            for (int i = 0; i < _interestCount; i++)
            {
                if (_fixedConsiderationDesirability[i] == 0)
                {
                    continue;
                }
                currentVec = _fixedConsiderationDirections[i];
                for (int ii = 0; ii < _interestCount; ii++)
                {
                    _interestMultiplier[ii] *= 1f + _fixedConsiderationDesirability[i] * (Vector3.Dot(_interest[ii], currentVec) / 2f - 0.5f);
                }
            }
            for (int i = 0; i < _considerationDirections.Count; i++)
            {
                currentVec = _considerationDirections[i];
                for (int ii = 0; ii < _interestCount; ii++)
                {
                    _interestMultiplier[ii] *= 1f + _considerationDesirability[i] * (Vector3.Dot(_interest[ii], currentVec) / 2f - 0.5f);
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
            _gizmosConsiderationDirections = new List<Vector3>(_considerationDirections);
            _gizmosConsiderationDesirability = new List<float>(_considerationDesirability);
            ClearConsiderations();
            return _interest[maxInterest];
        }

        public void ClearConsiderations()
        {
            _considerationDirections.Clear();
            _considerationDesirability.Clear();
        }

        /// <summary>
        /// Add a consideration vector with decreasing desiribility the closer it is
        /// </summary>
        /// <param name="direction">Direction towards target position, do NOT normalize</param>
        /// <param name="desiribility">Amount of desire to move towards</param>
        public void AddKeepDistanceConsideration(Vector3 direction, float desiribility, float maintainDistance)
        {
            if (desiribility == 0)
            {
                return;
            }
            float dist = direction.magnitude;
            float distSq = direction.sqrMagnitude;
            float keepDist = maintainDistance * 1.414213f; // sqrt(2)
            float keepDistSq = maintainDistance * maintainDistance * 2;
            if (dist < keepDist * KEEP_DIST_MIN)
            {
                _considerationDirections.Add(-direction.normalized);
                _considerationDesirability.Add(desiribility);
            }
            else if (dist < keepDist * KEEP_DIST_MAX)
            {
                // Interpolation formula
                float multiplier = 1f / (KEEP_DIST_MAX - KEEP_DIST_MIN);
                float interpolatedDist = multiplier * (dist - KEEP_DIST_MIN * keepDist);
                float interpolatedDistSq = interpolatedDist * interpolatedDist;
                _considerationDirections.Add(direction.normalized * (2f * interpolatedDistSq / keepDistSq - 1f));
                _considerationDesirability.Add(2f * Mathf.Abs(0.5f - interpolatedDistSq / keepDistSq) * desiribility);
            }
            else
            {
                _considerationDirections.Add(direction.normalized);
                _considerationDesirability.Add(desiribility);
            }
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
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, 1f);
                for (int i = 0; i < _gizmosConsiderationDirections.Count; i++)
                {
                    if (_gizmosConsiderationDesirability[i] < 0)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }
                    Gizmos.DrawSphere(transform.position + _gizmosConsiderationDirections[i] * 1.1f * _gizmosConsiderationDesirability[i], 0.05f);
                }

                for (int i = 0; i < _fixedConsiderationDirections.Length; i++)
                {
                    if (_fixedConsiderationDesirability[i] > 0)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }
                    Gizmos.DrawSphere(transform.position + _fixedConsiderationDirections[i] * -1.1f * _fixedConsiderationDesirability[i], 0.05f);
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