using UnityEngine;
using UnityEngine.SceneManagement;

namespace Morko
{
    public class HostWindow : MonoBehaviour
    {

        public WindowLoader windowLoader;
        public Texture hostImage;
        private int textureWidth = 150;
        private int textureHeight = 150;

        private void OnGUI()
        {
            GUI.DrawTexture(new Rect((Screen.width/2)-textureWidth/2, (Screen.height/2)-textureHeight/2, 300,300), hostImage);

            if(GUI.Button(new Rect(Screen.width/2, Screen.height/2, 160,40),"1"))
            {
                windowLoader.activeSceneCount = 2;
                windowLoader.activeSceneIndex = 1;
                SceneManager.LoadSceneAsync(1,LoadSceneMode.Additive);
                windowLoader.ToggleMenuCanvas();
                windowLoader.ToggleHostWindow();
            }

            
            
            if(GUI.Button(new Rect(Screen.width/2, Screen.height/2 +60, 160,40),"2"))
            {
                windowLoader.activeSceneCount = 2;
                windowLoader.activeSceneIndex = 2;
                SceneManager.LoadSceneAsync(2,LoadSceneMode.Additive);
                windowLoader.ToggleMenuCanvas();
                windowLoader.ToggleHostWindow();
            }

            if(GUI.Button(new Rect(Screen.width/2, Screen.height/2 +120, 160,40),"Back"))
            {
                windowLoader.ToggleMenu();
                windowLoader.ToggleHostWindow();
            }
        }
    }
}

