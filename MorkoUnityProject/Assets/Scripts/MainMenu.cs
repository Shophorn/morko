using UnityEngine;
using UnityEngine.SceneManagement;

namespace Morko
{
public class MainMenu : MonoBehaviour
    {
        public WindowLoader windowLoader;

        void Start()
        {
            
        }

        private void OnGUI()
        {
            if(GUI.Button(new Rect(Screen.width/2, Screen.height/2,160,40), "Host Game"))
            {
                //int sceneToLoad = 0;
                //SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            }

            if(GUI.Button(new Rect(Screen.width/2, Screen.height/2+60,160,40), "Host Game"))
            {
                windowLoader.ToggleHostWindow();
                windowLoader.ToggleMenu();
            }

            if(GUI.Button(new Rect(Screen.width/2, Screen.height/2 +120,160,40),"Quit Game"))
                Application.Quit();
        }
    }
}