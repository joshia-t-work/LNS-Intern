using LNS.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.CameraMovement
{
    public class CameraScript : MonoBehaviour
    {
        [SerializeField]
        private Entity _target;
        public void SetTarget(Entity transform)
        {
            _target = transform;
        }
        private void FixedUpdate()
        {
            if (_target != null)
            {
                Vector3 pos = _target.transform.position + Vector3.forward * -10f;
                pos += (Vector3)_target.Rb.velocity * 0.5f;
                if (_target is Soldier)
                {
                    Soldier sold = (Soldier)_target;
                    if (sold != null)
                    {
                        pos += (Vector3)sold.AimDirection.normalized * 2f;
                    }
                }
                transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 2f);
            }
        }
    }
}
