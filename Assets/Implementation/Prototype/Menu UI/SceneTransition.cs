using LNS.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LNS.UI
{
    public class SceneTransition : MonoBehaviour
    {
        public void LoadPrototype()
        {
            SceneManager.LoadScene("Prototype");
        }
        public void LoadGame()
        {
            SceneManager.LoadScene("Game");
        }
        public void LoadMenu()
        {
            SceneManager.LoadScene("Menu");
        }
        public void Exit()
        {
            Application.Quit();
        }
    }
}