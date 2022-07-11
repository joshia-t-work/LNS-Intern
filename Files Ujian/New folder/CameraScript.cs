using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class CameraScript : MonoBehaviour
    {
        private static CameraScript inst;
        private static Transform target;
        private void Awake()
        {
            if (inst == null)
            {
                inst = this;
            } else
            {
                Destroy(this);
            }
        }
        public static void SetTarget(Transform transform)
        {
            target = transform;
        }
        private void FixedUpdate()
        {
            if (target != null)
            {
                transform.position = Vector3.Lerp(transform.position, target.position + Vector3.forward * -10f, Time.deltaTime * 2f);
            }
        }
    }
}
