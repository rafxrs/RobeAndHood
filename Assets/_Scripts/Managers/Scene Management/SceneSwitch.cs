using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Managers.Scene_Management
{
    public class SceneSwitch : MonoBehaviour
    {
        public static string currentScene;
        public static string previousScene;
        // Start is called before the first frame update
        public virtual void Start()
        {
            currentScene = SceneManager.GetActiveScene().name;
        }

        // Method to load new scene
        public void SwitchScene(string sceneName)
        {
            previousScene = currentScene;
            SceneManager.LoadScene(sceneName);
        } 
    }
}
