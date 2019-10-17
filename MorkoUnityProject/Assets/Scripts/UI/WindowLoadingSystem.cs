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
		}

		void Update()
		{
			if (Input.GetKeyUp(KeyCode.Escape) || Input.GetButtonUp("Cancel"))
			{
				pauseWindow.SetActive(!pauseWindow.activeInHierarchy);
			}
		}

		public void BackToMainMenu()
		{
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
			hostWindow.SetActive(true);
			menuDisabler.SetActive(true);
		}

		public void JoinWindow()
		{
			joinWindow.SetActive(true);
			menuDisabler.SetActive(true);
		}

		public void OptionsWindow()
		{
			optionsWindow.SetActive(true);
			menuDisabler.SetActive(true);
		}

		public void CreditsWindow()
		{
			creditsWindow.SetActive(true);
			menuDisabler.SetActive(true);
		}

		public void CreateRoom()
		{
			mainMenu.SetActive(false);
			hostWindow.SetActive(false);
			lobbyHost.SetActive(true);
		}

		public void JoinRoom()
		{
			mainMenu.SetActive(false);
			joinWindow.SetActive(false);
			lobbyPlayer.SetActive(true);
		}

		public void StartGame()
		{
			StartCoroutine(LoadScene(scrollContent.currentItem.id + 1));
			lobbyHost.SetActive(false);
			lobbyPlayer.SetActive(false);
		}

		IEnumerator LoadScene(int sceneIndex) // This async operation initiates character instantiation after the correct scene has been loaded and set as active
		{
			loadWindow.SetActive(true);
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
			loadWindow.SetActive(false);
		}

		public void ExitCurrentGame()
		{
			SceneManager.UnloadSceneAsync(scrollContent.currentItem.id + 1);
			SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
			BackToMainMenu();
		}
	}
}
