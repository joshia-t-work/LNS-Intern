using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS
{
    /// <summary>
    /// Singleton to manage game
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager s_inst;
        private void Awake()
        {
            if (s_inst == null)
            {
                s_inst = this;
            }
            else
            {
                Destroy(this);
            }
        }
    }
}