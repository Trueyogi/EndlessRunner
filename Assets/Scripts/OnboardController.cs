using UnityEngine;

public class OnboardController : MonoBehaviour
{
    public SceneLoader sceneLoader;

    public void OnNextButtonClicked()
    {
        sceneLoader.LoadScene("Game");
    }
}