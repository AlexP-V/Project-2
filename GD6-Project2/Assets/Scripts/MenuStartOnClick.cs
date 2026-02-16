using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuStartOnClick : MonoBehaviour
{
    [Tooltip("Scene build index to load when the player clicks/taps anywhere on the menu.")]
    public int targetSceneIndex = 1;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            enabled = false; // prevent multiple triggers
            AudioManager.Instance.PlayStartSound();
            SceneTransition.LoadSceneWithFade(targetSceneIndex);
        }
    }
}
