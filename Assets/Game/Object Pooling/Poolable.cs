using LNS.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.ObjectPooling
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
        [Header("Poolable")]
        [SerializeField] private PoolableScriptableObject poolableType;
        public PoolableScriptableObject PoolableType
        {
            get { return poolableType; }
        }

        public virtual void OnPoolCreate()
        {

        }
        public virtual void OnPoolRemove()
        {

        }
    }
}