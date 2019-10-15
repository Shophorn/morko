using System.Collections;
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
		public GameObject loadWindow;
		public GameObject lobbyHost;
		public GameObject lobbyPlayer;
		public GameObject joinWindow;
		public GameObject optionsWindow;
		public GameObject creditsWindow;

		public ScrollContent scrollContent;

		//private int currentActiveSelection;
		//private bool isAxisInUse;

		public GameObject menuDisabler;

        void Start()
        {
            mainMenu.SetActive(true);
            menuWindow.SetActive(true);
			menuDisabler.SetActive(false);
            hostWindow.SetActive(false);
            pauseWindow.SetActive(false);
			loadWindow.SetActive(false);
			lobbyHost.SetActive(false);
			lobbyPlayer.SetActive(false);
			joinWindow.SetActive(false);
			optionsWindow.SetActive(false);
			creditsWindow.SetActive(false);

			//currentActiveSelection = 1;
			//ChangeSelection(currentActiveSelection);
		}

        void Update()
        {
			if (Input.GetKeyUp(KeyCode.Escape) || Input.GetButtonUp("Cancel"))
            {
                TogglePauseWindow();
            }
			//if(menuWindow.activeInHierarchy)
			//	MoveInMenuWindow();
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

		public void ToggleLoadWindow()
		{
			loadWindow.SetActive(!loadWindow.activeInHierarchy);
		}

		public void ToggleLobbyHost()
		{
			lobbyHost.SetActive(!lobbyHost.activeInHierarchy);
		}

		public void ToggleLobbyPlayer()
		{
			lobbyPlayer.SetActive(!lobbyPlayer.activeInHierarchy);
		}

		public void ToggleJoinWindow()
		{
			joinWindow.SetActive(!joinWindow.activeInHierarchy);
		}

		public void ToggleOptionsWindow()
		{
			optionsWindow.SetActive(!optionsWindow.activeInHierarchy);
		}
		public void ToggleCreditsWindow()
		{
			creditsWindow.SetActive(!creditsWindow.activeInHierarchy);
		}

		public void BackToMainMenu()
        {
			//currentActiveSelection = 1;
			//ChangeSelection(currentActiveSelection);
            //if(hostWindow.activeInHierarchy == false)
            //    SceneManager.UnloadSceneAsync(scrollContent.currentItem.id+1);
            mainMenu.SetActive(true);
            menuWindow.SetActive(true);
			hostWindow.SetActive(false);
			joinWindow.SetActive(false);
            pauseWindow.SetActive(false);
			menuDisabler.SetActive(false);
			lobbyHost.SetActive(false);
			lobbyPlayer.SetActive(false);
			loadWindow.SetActive(false);
			optionsWindow.SetActive(false);
			creditsWindow.SetActive(false);

		}
        public void QuitGame()
        {
            Application.Quit();
        }

        public void HostGame()
        {
            ToggleHostWindow();
			menuDisabler.SetActive(true);
			//currentActiveSelection = 3;
			//ChangeSelection(currentActiveSelection);
        }

		public void JoinWindow()
		{
			ToggleJoinWindow();
			menuDisabler.SetActive(true);
		}

		public void OptionsWindow()
		{
			ToggleOptionsWindow();
			menuDisabler.SetActive(true);
		}

		public void CreditsWindow()
		{
			ToggleCreditsWindow();
			menuDisabler.SetActive(true);
		}

		public void CreateRoom()
		{
			ToggleMainMenu();
			ToggleHostWindow();
			ToggleLobbyHost();
		}

		public void JoinRoom()
		{
			ToggleMainMenu();
			ToggleJoinWindow();
			ToggleLobbyPlayer();
		}

		public void StartGame()
        {
            StartCoroutine(LoadScene(scrollContent.currentItem.id + 1));
            ToggleMainMenu();
            ToggleLobbyHost();
			ToggleLobbyPlayer();
        }

        IEnumerator LoadScene(int sceneIndex) // This async operation initiates character instantiation after the correct scene has been loaded and set as active
        {
			ToggleLoadWindow();
            AsyncOperation load = SceneManager.LoadSceneAsync((sceneIndex),LoadSceneMode.Additive);
            while (!load.isDone)
            {
                if (load.progress >= 0.95f)
                {

                }
                yield return null;
            }
            // After scene has been loaded
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIndex));
            ProtoGameCreator.Instance.StartScene();
			ToggleLoadWindow();
        }

        public void ExitCurrentGame()
        {
            SceneManager.UnloadSceneAsync(scrollContent.currentItem.id + 1);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
            BackToMainMenu();
        }
    }
}
