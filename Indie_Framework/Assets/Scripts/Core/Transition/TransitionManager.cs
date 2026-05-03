using System.Collections;
using Core.Tools;
using Saveable;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Transition
{
    public class TransitionManager : SingletonMono<TransitionManager>, ISaveable
    {
        public CanvasGroup fadeCanvasGroup;
        public float fadeDuration;

        private bool isFade;
        private string curScene;
        [SceneName]
        [SerializeField]
        private string startScene;
        private bool canTransition;

        private void Start()
        {
            ISaveable saveable = this;
            saveable.Register();
        }

        private void OnEnable()
        {
            // EventModule.AddListener(EventName.EvtUpdateGameState, OnUpdateGameStateEvent);
            // EventModule.AddListener(EventName.EvtStartGameEvent, OnStartNewGameEvent);
        }

        private void OnDisable()
        {
            // EventModule.RemoveListener(EventName.EvtUpdateGameState, OnUpdateGameStateEvent);
            // EventModule.RemoveListener(EventName.EvtStartGameEvent, OnStartNewGameEvent);
        }

        /// <summary>
        /// 新游戏进入的场景
        /// </summary>
        /// <param name="week"></param>
        private void OnStartNewGameEvent(object obj)
        {
            if (obj is not int) return;
            StartCoroutine(TransitionScene("Main", startScene));
        }

        // private void OnUpdateGameStateEvent(object obj)
        // {
        //     if (obj is not GameState gameState) return;
        //     canTransition = gameState == GameState.GamePlay;
        // }

        public void Transition(string from, string to)
        {
            if (isFade == false && canTransition)
            {
                StartCoroutine(TransitionScene(from, to));
            }
        }

        private IEnumerator TransitionScene(string from, string to)
        {
            if (from != string.Empty)
            {
                EventModule.Dispatch(EventName.EvtBeforeUnloadScene);
                yield return Fade(1.0f);
                yield return SceneManager.UnloadSceneAsync(from);
            }

            yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);

            var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            SceneManager.SetActiveScene(scene);
            // if (IsPersistableScene(scene.name))
            // {
            //     curScene = scene.name;
            // }

            EventModule.Dispatch(EventName.EvtAfterLoadScene);
            // Let the loaded scene finish one rendered frame before lifting the fade.
            yield return new WaitForEndOfFrame();
            yield return Fade(0f);
        }

        private IEnumerator Fade(float targetAlpha)
        {
            isFade = true;
            fadeCanvasGroup.blocksRaycasts = true;

            var speed = Mathf.Abs(fadeCanvasGroup.alpha - targetAlpha) / fadeDuration;

            while (Mathf.Approximately(fadeCanvasGroup.alpha, targetAlpha) == false)
            {
                fadeCanvasGroup.alpha = Mathf.MoveTowards(fadeCanvasGroup.alpha, targetAlpha, speed * Time.deltaTime);
                yield return null;
            }

            isFade = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        public SaveData GenerateSaveData()
        {
            SaveData data = new SaveData();
            var activeSceneName = SceneManager.GetActiveScene().name;
            // if (IsPersistableScene(activeSceneName))
            // {
            //     curScene = activeSceneName;
            // }
            // else if (IsPersistableScene(curScene) == false &&
            //          SaveLoadManager.Instance.TryGetSavedData<TransitionManager>(out var savedData) &&
            //          IsPersistableScene(savedData.curScene))
            // {
            //     curScene = savedData.curScene;
            // }
            curScene = activeSceneName;

            data.curScene = curScene ?? string.Empty;
            return data;
        }

        public void ReadGameData(SaveData gameData)
        {
            curScene = gameData.curScene;
            // if (IsPersistableScene(curScene) == false)
            // {
            //     return;
            // }

            Transition("Main", curScene);
        }

        // public static bool IsPersistableScene(string sceneName)
        // {
        //     return string.IsNullOrEmpty(sceneName) == false && sceneName != SceneName.Main;
        // }
    }
}