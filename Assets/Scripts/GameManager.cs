using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Singleton to manage game
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager inst;
        private void Awake()
        {
            if (inst == null)
            {
                inst = this;
            }
            else
            {
                Destroy(this);
            }
        }
    }
}