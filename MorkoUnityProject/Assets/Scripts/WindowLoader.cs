using UnityEngine;
using UnityEngine.SceneManagement;

namespace Morko
{
    public class WindowLoader : MonoBehaviour
    {
        public Canvas menuCanvas;
        public GameObject mainMenu;
        public GameObject hostWindow;
        public GameObject pauseWindow;
        public int activeSceneCount = 1;
        public int activeSceneIndex = 0;

        private int numberOfScenes;
        private string[] sceneNames;

        void Start()
        {
            numberOfScenes = SceneManager.sceneCountInBuildSettings;
            sceneNames = new string[numberOfScenes];
            for(int i = 0; i < numberOfScenes; i++)
            {
                sceneNames[i] = SceneManager.GetSceneByBuildIndex(i).name;
            }
            menuCanvas.enabled = true;
            mainMenu.SetActive(true);
            hostWindow.SetActive(false);
            pauseWindow.SetActive(false);
        }

        void Update()
        {
            Debug.Log(activeSceneCount);
            if(Input.GetKeyUp(KeyCode.Escape) && activeSceneCount > 1)
            {
                TogglePauseWindow();
            }
        }

        public void ToggleMenu()
        {
            mainMenu.SetActive(!mainMenu.activeInHierarchy);
        }

        public void ToggleHostWindow()
        {
            hostWindow.SetActive(!hostWindow.activeInHierarchy);
        }

        public void ToggleMenuCanvas()
        {
            menuCanvas.enabled = !menuCanvas.enabled;
        }

        public void TogglePauseWindow()
        {
            pauseWindow.SetActive(!pauseWindow.activeInHierarchy);
        }

        public void BackToMainMenu()
        {
            activeSceneCount = 1;
            menuCanvas.enabled = true;
            mainMenu.SetActive(true);
            hostWindow.SetActive(false);
            pauseWindow.SetActive(false);
        }

        public int GetSceneIndexByName(string sceneName)
        {
            for(int i = 0; i < sceneNames.Length; i++)
            {
                if(sceneName == sceneNames[i])
                    return i;
            }
            return -1;
        }
    }
}