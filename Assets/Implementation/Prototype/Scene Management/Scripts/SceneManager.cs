using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LNS.SceneManagement
{
    public class SceneManager : MonoBehaviour
    {
        #region Variables

        [SerializeField]
        private GameObject _cover;

        [SerializeField]
        private Slider _progressBar;

        [SerializeField]
        private Canvas _canvas;

        public static SceneManager s_inst;

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            if (s_inst == null)
            {
                s_inst = this;
                DontDestroyOnLoad(this);
            } else {
                Destroy(this);
            }
        }

        #endregion
        #region Class Methods

        public static void LoadScene(string sceneName)
        {
            s_inst.StartCoroutine(s_inst.LoadSceneCoroutine(sceneName));
        }
        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            _cover.SetActive(true);
            _progressBar.value = 0f;
            AsyncOperation scene = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            while (scene.progress < 0.9f)
            {
                _progressBar.value = scene.progress;
                yield return null;
            }
            _cover.SetActive(false);
        }

        #endregion
    }
}