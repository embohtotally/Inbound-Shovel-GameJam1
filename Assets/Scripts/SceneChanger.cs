using UnityEngine;
using UnityEngine.SceneManagement; // Required for changing scenes

public class SceneChanger : MonoBehaviour
{
    // This public method can be called by a UI Button.
    // It takes the name of the scene to load as a parameter.
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}