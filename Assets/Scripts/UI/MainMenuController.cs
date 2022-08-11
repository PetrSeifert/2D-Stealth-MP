using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class MainMenuController : MonoBehaviour, IPointerExitHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject levelButtonPrefab;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject levelsMenu;
    [SerializeField] private List<string> levels;

    private Vector2 startPosition = new Vector2(60, 195);
    private Vector2 lastPosition = new Vector2(420, 75);
    private Vector2 currentPosition = new Vector2(60, 195);
    private Vector2 offsetVector = new Vector2(240, 135);
    private float shiftLength = 60;
    private float scaleFactor = Screen.width / 480f;

    private void Awake()
    {
        scaleFactor = Screen.width / 480f;
        startPosition = new Vector2(startPosition.x * scaleFactor, startPosition.y * scaleFactor);
        lastPosition = new Vector2(lastPosition.x * scaleFactor, lastPosition.y * scaleFactor);
        currentPosition = new Vector2(currentPosition.x * scaleFactor, currentPosition.y * scaleFactor);
        offsetVector = new Vector2(offsetVector.x * scaleFactor, offsetVector.y * scaleFactor);
        shiftLength *= scaleFactor;
        mainMenu.SetActive(true);
        levelsMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    void Start()
    {
        Debug.Log($"{Screen.width}, {scaleFactor}, {startPosition.x}, {startPosition.y}");
        int levelNumber = 1;
        foreach (var level in levels)
        {
            GameObject levelButton = Instantiate(levelButtonPrefab, currentPosition, Quaternion.identity, levelsMenu.transform);
            levelButton.GetComponentInChildren<TMP_Text>().text = levelNumber.ToString();
            StartLevel startLevel = levelButton.GetComponent<StartLevel>();
            startLevel.levelName = level;
            if (currentPosition.x < lastPosition.x) currentPosition += new Vector2(shiftLength, 0);
            else currentPosition = new Vector2(startPosition.x, currentPosition.y - shiftLength);
            levelNumber++;
        }
    }


    public void OpenLevelsMenu()
    {
        mainMenu.SetActive(false);
        levelsMenu.SetActive(true);
    }

    public void OpenSettings()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void BackToMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        levelsMenu.SetActive(false);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        eventSystem.SetSelectedGameObject(null);
    }
}
