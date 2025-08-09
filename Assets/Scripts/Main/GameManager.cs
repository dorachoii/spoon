using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static event Action OnGameReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    public void StartNewGame()
    {
        PersistenceManager.Instance?.ClearSave();
        SceneManager.LoadScene(SceneNames.GAME_SCENE_NAME);
        AudioManager.Instance.ChangeBGM(BGMType.Game);
    }

    public void StartFromSavedGame()
    {
        PersistenceManager.Instance?.LoadGame();
        SceneManager.LoadScene(SceneNames.GAME_SCENE_NAME);
        AudioManager.Instance.ChangeBGM(BGMType.Game);
    }


    public void BackToTitle()
    {
        SceneManager.LoadScene(SceneNames.INTRO_SCENE_NAME);
        AudioManager.Instance.ChangeBGM(BGMType.Intro);
    }
}
