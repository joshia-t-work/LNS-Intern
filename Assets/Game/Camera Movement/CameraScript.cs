using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.CameraMovement
{
    public class CameraScript : MonoBehaviour
    {
        private Transform _target;
        public void SetTarget(Transform transform)
        {
            _target = transform;
        }
        private void FixedUpdate()
        {
            if (_target != null)
            {
                transform.position = Vector3.Lerp(transform.position, _target.position + Vector3.forward * -10f, Time.deltaTime * 2f);
            }
        }
    }
}
