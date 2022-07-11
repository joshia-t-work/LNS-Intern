using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.Entities
{
    [CreateAssetMenu(fileName = "PoolableObject", menuName = "Entities/Poolable")]
    public class PoolableScriptableObject : ScriptableObject
    {
        [SerializeField] string _objectName;
        public string ObjectName
        {
            get
            {
                return _objectName;
            }
        }
    }
}