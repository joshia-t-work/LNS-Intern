using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Generic poolable instance
    /// </summary>
    public abstract class Poolable : MonoBehaviour
    {
        /// <summary>
        /// Used to know if the object is spawned manually or from pool (e.g. editor)
        /// </summary>
        [HideInInspector] public bool isPooled;

        public abstract InstancePool.PoolableType poolableType { get; }

        public virtual void OnPoolCreate()
        {

        }
        public virtual void OnPoolRemove()
        {

        }
    }
}