using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject deathScreen;

    void Awake()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        winScreen.SetActive(false);
        pauseMenu.SetActive(true);
        settingsPanel.SetActive(false);
        deathScreen.SetActive(false);
    }

    public void ResumeGame()
    {
        EventSystem.current.SetSelectedGameObject(null);
        playerInput.SwitchCurrentActionMap("Player");
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ShowDeathScreen()
    {
        pausePanel.SetActive(false);
        deathScreen.SetActive(true);
        playerInput.SwitchCurrentActionMap("UI");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
