using UnityEngine;
using UnityEngine.SceneManagement;

public class StartLevel : MonoBehaviour
{
    [HideInInspector] public string levelName;

    public void PlayLevel()
    {
        SceneManager.LoadScene(levelName);
    }
}
