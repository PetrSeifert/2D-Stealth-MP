using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneIntegration
{
#if UNITY_EDITOR 
    public static int otherScene = -2;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitLoadingScene()
    {
        Debug.Log("InitLoadingScene()");
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex == 0) return;

        Debug.Log("Loading _preload scene");
        otherScene = sceneIndex;
        SceneManager.LoadScene(0);
    }
#endif
}