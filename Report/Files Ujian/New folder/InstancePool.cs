using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Singleton to handle object pooling
    /// </summary>
    public class InstancePool : MonoBehaviour
    {
        [SerializeField] private Transform[] transforms;
        public Dictionary<PoolableType, Queue<Poolable>> inactivePool = new Dictionary<PoolableType, Queue<Poolable>>();
        public Dictionary<PoolableType, List<Poolable>> activePool = new Dictionary<PoolableType, List<Poolable>>();
        public Dictionary<PoolableType, Transform> transformList = new Dictionary<PoolableType, Transform>();
        public static InstancePool inst;
        private void Awake()
        {
            if (inst == null)
            {
                inst = this;
                for (int i = 0; i < transforms.Length; i++)
                {
                    Poolable poolable = transforms[i].GetComponent<Poolable>();
                    inactivePool.Add(poolable.poolableType, new Queue<Poolable>());
                    activePool.Add(poolable.poolableType, new List<Poolable>());
                    transformList.Add(poolable.poolableType, transforms[i]);
                }
            }
            else
            {
                Destroy(this);
            }
        }
        public enum PoolableType
        {
            Player,
            Enemy,
        }
        public static Poolable[] GetInstances(PoolableType type)
        {
            return inst.activePool[type].ToArray();
        }
        /// <summary>
        /// Creates an instance of the type from pool or instantiate a new one if it does not exist
        /// </summary>
        /// <param name="type">Type of instance</param>
        public static void TryInstantiate(PoolableType type)
        {
            try
            {
                Poolable poolable;
                if (inst.inactivePool[type].Count > 0)
                {
                    poolable = inst.inactivePool[type].Dequeue();
                }
                else
                {
                    poolable = Instantiate(inst.transformList[type]).GetComponent<Poolable>();
                }
                AddToPool(poolable);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Missing reference for " + type, inst);
            }
        }
        /// <summary>
        /// Adds the poolable to the active pool, also fires OnPoolCreate
        /// </summary>
        /// <param name="poolable">pooled object</param>
        public static void AddToPool(Poolable poolable)
        {
            Reactivate(poolable);
            poolable.isPooled = true;
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
                poolable.OnPoolRemove();
            }
            catch (System.Exception)
            {
                Debug.LogError("Error removing object not pooled: " + poolable, inst);
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
                inst.activePool[poolable.poolableType].Remove(poolable);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Missing reference for " + poolable.poolableType, inst);
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
                inst.activePool[poolable.poolableType].Add(poolable);
            }
            catch (KeyNotFoundException)
            {
                Debug.LogError("Missing reference for " + poolable.poolableType, inst);
            }
        }
    }
}