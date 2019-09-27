using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Morko
{
    public class WindowLoadingSystem : MonoBehaviour
    {        
        public GameObject mainMenu;
        public GameObject menuWindow;
        public GameObject hostWindow;
        public GameObject pauseWindow;

        private int numberOfScenes;
        private string[] sceneNames;

        private Dropdown dropdown = null;

        void Start()
        {
            numberOfScenes = SceneManager.sceneCountInBuildSettings;
            sceneNames = new string[numberOfScenes];
            for(int i = 0; i < numberOfScenes; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                sceneNames[i] = sceneName;
            }
            mainMenu.SetActive(true);
            menuWindow.SetActive(true);
            hostWindow.SetActive(false);
            pauseWindow.SetActive(false);
        }

        void Update()
        {
            if(Input.GetKeyUp(KeyCode.Escape))
            {
                TogglePauseWindow();
            }
        }

        public void ToggleMainMenu()
        {
            mainMenu.SetActive(!mainMenu.activeInHierarchy);
        }

        public void ToggleHostWindow()
        {
            hostWindow.SetActive(!hostWindow.activeInHierarchy);
        }

        public void ToggleMenuWindow()
        {
            menuWindow.SetActive(!menuWindow.activeInHierarchy);
        }

        public void TogglePauseWindow()
        {
            if(!hostWindow.activeInHierarchy && !mainMenu.activeInHierarchy)
                pauseWindow.SetActive(!pauseWindow.activeInHierarchy);
        }

        public void BackToMainMenu()
        {
            if(hostWindow.activeInHierarchy == false)
                SceneManager.UnloadSceneAsync(dropdown.value+1);
            mainMenu.SetActive(true);
            menuWindow.SetActive(true);
            hostWindow.SetActive(false);
            pauseWindow.SetActive(false);
        }

        public int GetSceneIndexByName(string sceneName)
        {
            for(int i = 1; i < sceneNames.Length; i++)
            {
                if(sceneName == sceneNames[i])
                    return i;
            }
            return -1;
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void HostGame()
        {
            ToggleHostWindow();
            ToggleMenuWindow();

            dropdown = GameObject.FindObjectOfType<Dropdown>();
            dropdown.ClearOptions();
            for(int i = 1; i < sceneNames.Length; i++)
            {
                Dropdown.OptionData data = new Dropdown.OptionData(sceneNames[i]);
                dropdown.options.Add(data);
            }
        }

        public void StartGame()
        {
            SceneManager.LoadSceneAsync((dropdown.value+1),LoadSceneMode.Additive);
            ToggleMainMenu();
            ToggleHostWindow();
        }

        public void ExitCurrentGame()
        {
            SceneManager.UnloadSceneAsync(dropdown.value+1);
            BackToMainMenu();
        }
    }
}