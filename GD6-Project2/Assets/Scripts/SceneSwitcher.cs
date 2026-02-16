using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Called by UI button OnClick to start the main scene (index 1)
    public void StartGame()
    {
        SceneTransition.LoadSceneWithFade(1);
    }

    // Optional: call this to return to menu explicitly
    public void ReturnToMenu()
    {
        SceneTransition.LoadSceneWithFade(0);
    }
}
