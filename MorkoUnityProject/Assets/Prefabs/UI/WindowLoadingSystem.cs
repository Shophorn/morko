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

		public GameObject[] menuWindowItems;
		public GameObject[] hostWindowItems;

		public ScrollContent scrollContent;

        //private int numberOfScenes;
        //private string[] sceneNames;

        private Dropdown dropdown = null;

		private int currentActiveSelection;
		private bool isAxisInUse;
		//private bool dropdownShown;

        void Start()
        {
            //numberOfScenes = SceneManager.sceneCountInBuildSettings;
            //sceneNames = new string[numberOfScenes];
            //for(int i = 0; i < numberOfScenes; i++)
            //{
            //    string path = SceneUtility.GetScenePathByBuildIndex(i);
            //    string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
            //    sceneNames[i] = sceneName;
            //}
            mainMenu.SetActive(true);
            menuWindow.SetActive(true);
            hostWindow.SetActive(false);
            pauseWindow.SetActive(false);
			loadWindow.SetActive(false);

			currentActiveSelection = 1;
			ChangeSelection(currentActiveSelection);
        }

        void Update()
        {
			if (Input.GetKeyUp(KeyCode.Escape) || Input.GetButtonUp("Cancel"))
            {
                TogglePauseWindow();
            }
			if(menuWindow.activeInHierarchy)
				MoveInMenuWindow();
			//if (hostWindow.activeInHierarchy && !dropdownShown)
				//MoveInHostWindow();

			//if (!dropdownShown && Input.GetButtonDown("Cancel"))
				//BackToMainMenu();
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

        public void BackToMainMenu()
        {
			currentActiveSelection = 1;
			ChangeSelection(currentActiveSelection);
            if(hostWindow.activeInHierarchy == false)
                SceneManager.UnloadSceneAsync(scrollContent.currentItem.id+1);
            mainMenu.SetActive(true);
            menuWindow.SetActive(true);
            hostWindow.SetActive(false);
            pauseWindow.SetActive(false);
        }

        //public int GetSceneIndexByName(string sceneName)
        //{
        //    for(int i = 1; i < sceneNames.Length; i++)
        //    {
        //        if(sceneName == sceneNames[i])
        //            return i;
        //    }
        //    return -1;
        //}

        public void QuitGame()
        {
            Application.Quit();
        }

        public void HostGame()
        {
            ToggleHostWindow();
            ToggleMenuWindow();
			currentActiveSelection = 3;
			ChangeSelection(currentActiveSelection);
            //dropdown = GameObject.FindObjectOfType<Dropdown>();
            //dropdown.ClearOptions();
   //         for(int i = 1; i < sceneNames.Length; i++)
   //         {
			//	Dropdown.OptionData data = new Dropdown.OptionData(sceneNames[i]);
			//	dropdown.options.Add(data);
			//}
        }

        public void StartGame()
        {
            // Dropdown.value is used to identify scene from Build Settings: Scenes in Build
            // scene 0 on Menu
            StartCoroutine(LoadScene(scrollContent.currentItem.id + 1));
            ToggleMainMenu();
            ToggleHostWindow();
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
            //SceneManager.UnloadSceneAsync(dropdown.value+1);
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(0));
            BackToMainMenu();
        }

		public void ChangeSelection(int currentSelection)
		{
			switch (currentSelection)
			{
				case 1:
					Button hostButton = menuWindow.transform.Find("Host Button").GetComponent<Button>();
					hostButton.Select();
					break;
				case 2:
					Button joinButton = menuWindow.transform.Find("Join Button").GetComponent<Button>();
					joinButton.Select();
					break;
				case 3:
					Button startButton = hostWindow.transform.Find("Start Game Button").GetComponent<Button>();
					startButton.Select();
					break;
				case 4:
					Button quitButton = menuWindow.transform.Find("Quit Button").GetComponent<Button>();
					quitButton.Select();
					break;
				case 5:
					Button backToMenuButton = hostWindow.transform.Find("Back To Menu Button").GetComponent<Button>();
					backToMenuButton.Select();
					break;
				case 6:
					InputField nameInput = hostWindow.transform.Find("Host Name Field").GetComponent<InputField>();
					nameInput.Select();
					break;
			}
		}


		private void MoveInMenuWindow()
		{
			if (Input.GetAxisRaw("Vertical") == -1)
			{
				if (!isAxisInUse)
				{

					currentActiveSelection++;
					if (currentActiveSelection == 5)
						currentActiveSelection = 1;
					if (currentActiveSelection == 3)
						currentActiveSelection = 4;
					ChangeSelection(currentActiveSelection);
					isAxisInUse = true;
				}
			}
			if (Input.GetAxisRaw("Vertical") == 1)
			{
				if (!isAxisInUse)
				{
					currentActiveSelection--;
					if (currentActiveSelection == 0)
						currentActiveSelection = 4;
					if (currentActiveSelection == 3)
						currentActiveSelection = 2;
					ChangeSelection(currentActiveSelection);
					isAxisInUse = true;
				}
			}
			if (Input.GetAxisRaw("Vertical") == 0)
				isAxisInUse = false;
		}

		private void MoveInHostWindow()
		{
			if (Input.GetAxisRaw("Vertical") == -1)
			{
				if (!isAxisInUse)
				{
					currentActiveSelection++;
					if (currentActiveSelection == 4)
						currentActiveSelection = 5;
					if (currentActiveSelection > 6)
						currentActiveSelection = 3;
					if (currentActiveSelection < 3)
						currentActiveSelection = 6;
					ChangeSelection(currentActiveSelection);
					isAxisInUse = true;
				}
			}
			if (Input.GetAxisRaw("Vertical") == 1)
			{
				if (!isAxisInUse)
				{
					currentActiveSelection--;
					if (currentActiveSelection == 4)
						currentActiveSelection = 3;
					if (currentActiveSelection > 6)
						currentActiveSelection = 3;
					if (currentActiveSelection < 3)
						currentActiveSelection = 6;
					ChangeSelection(currentActiveSelection);
					isAxisInUse = true;
				}
			}
			if (Input.GetAxisRaw("Vertical") == 0)
				isAxisInUse = false;
		}
    }
}
