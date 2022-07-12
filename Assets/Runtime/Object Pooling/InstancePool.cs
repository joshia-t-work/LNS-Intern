using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.ObjectPooling
{
    /// <summary>
    /// Singleton to handle object pooling
    /// </summary>
    public class InstancePool : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private Transform[] _transforms;

        private Dictionary<string, Queue<Poolable>> _inactivePool = new Dictionary<string, Queue<Poolable>>();
        private Dictionary<string, List<Poolable>> _activePool = new Dictionary<string, List<Poolable>>();
        private Dictionary<string, Transform> _transformList = new Dictionary<string, Transform>();
        public static InstancePool s_inst;

        #endregion
        #region MonoBehaviour

        private void Awake()
        {
            if (s_inst == null)
            {
                s_inst = this;
                for (int i = 0; i < _transforms.Length; i++)
                {
                    Poolable poolable = _transforms[i].GetComponent<Poolable>();
                    _inactivePool.Add(poolable.PoolableType.ObjectName, new Queue<Poolable>());
                    _activePool.Add(poolable.PoolableType.ObjectName, new List<Poolable>());
                    _transformList.Add(poolable.PoolableType.ObjectName, _transforms[i]);
                }
            }
            else
            {
                Destroy(this);
            }
        }

        #endregion
        #region Class Methods

        public static Poolable[] GetInstances(string type)
        {
            return s_inst._activePool[type].ToArray();
        }
        /// <summary>
        /// Creates an instance of the type from pool or instantiate a new one if it does not exist
        /// </summary>
        /// <param name="type">Type of instance</param>
        /// <returns>A Poolable object</returns>
        public static Poolable TryInstantiate(string type)
        {
            try
            {
                Poolable poolable;
                if (s_inst._inactivePool[type].Count > 0)
                {
                    poolable = s_inst._inactivePool[type].Dequeue();
                }
                else
                {
                    poolable = Instantiate(s_inst._transformList[type]).GetComponent<Poolable>();
                }
                AddToPool(poolable);
                return poolable;
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Missing reference for " + type, s_inst);
                return null;
            }
        }
        /// <summary>
        /// Used to know if the object is spawned manually or from pool (e.g. editor)
        /// </summary>
        /// <param name="poolable">pooled object</param>
        /// <returns>True or false</returns>
        public static bool IsPooled(Poolable poolable)
        {
            return s_inst._activePool[poolable.PoolableType.ObjectName].Contains(poolable);
        }
        /// <summary>
        /// Adds the poolable to the active pool, also fires OnPoolCreate
        /// </summary>
        /// <param name="poolable">pooled object</param>
        public static void AddToPool(Poolable poolable)
        {
            Reactivate(poolable);
            poolable.OnPoolCreate();
        }
        /// <summary>
        /// Moves the poolable to the inactive pool from active pool, also fires OnPoolRemove
        /// </summary>
        /// <param name="poolable">pooled object</param>
        public static void RemoveFromPool(Poolable poolable)
        {
            try
            {
                Deactivate(poolable);
                s_inst._inactivePool[poolable.PoolableType.ObjectName].Enqueue(poolable);
                poolable.OnPoolRemove();
            }
            catch (System.Exception)
            {
                Debug.LogError("Error removing object not pooled: " + poolable, s_inst);
            }
        }
        /// <summary>
        /// Removes the poolable from activepool
        /// </summary>
        /// <param name="poolable">pooled object</param>
        public static void Deactivate(Poolable poolable)
        {
            try
            {
                s_inst._activePool[poolable.PoolableType.ObjectName].Remove(poolable);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Missing reference for " + poolable.PoolableType.ObjectName, s_inst);
            }
        }
        /// <summary>
        /// Adds the poolable to activepool
        /// </summary>
        /// <param name="poolable">pooled object</param>
        public static void Reactivate(Poolable poolable)
        {
            try
            {
                s_inst._activePool[poolable.PoolableType.ObjectName].Add(poolable);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Missing reference for " + poolable.PoolableType.ObjectName, s_inst);
            }
        }

        #endregion
    }
}