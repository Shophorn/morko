using UnityEngine;
using UnityEngine.SceneManagement;

namespace Morko
{
    public class PauseWindow : MonoBehaviour
    {

        public WindowLoader windowLoader;
    
        public Texture pauseImage;
        private int textureWidth = 150;
        private int textureHeight = 150;
        private void Start()
        {

        }

        private void OnGUI()
        {
            GUI.DrawTexture(new Rect((Screen.width/2)-textureWidth/2, (Screen.height/2)-textureHeight/2, 300,300), pauseImage);

            if(GUI.Button(new Rect(Screen.width/2, Screen.height/2, 160,40),"Exit"))
            {
                SceneManager.UnloadSceneAsync(windowLoader.activeSceneIndex);
                windowLoader.BackToMainMenu();
            }
        }

    }
}
