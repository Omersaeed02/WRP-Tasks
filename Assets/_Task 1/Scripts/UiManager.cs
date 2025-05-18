using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;
    
    public GameObject gameOverMenu;
    public GameObject scoreHolder;
    public TMP_Text scoreText;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowGameOverMenu()
    {
        gameOverMenu.SetActive(true);
        gameOverMenu.transform.DOScale(1f, 0.75f);
    }
    
    public void RestartGame()
    {
        AudioManager.Instance?.PlayButtonSound();
        Invoke(nameof(RestartGameWithDelay), 0.5f);
    }
    
    public void BackToMainMenu()
    {
        AudioManager.Instance?.PlayButtonSound();
        Invoke(nameof(BackToMainMenuWithDelay), 0.5f);
    }
    
    public void QuitGame()
    {
        AudioManager.Instance?.PlayButtonSound();
        Invoke(nameof(QuitGameWithDelay), 0.5f);
    }
    
    private void RestartGameWithDelay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void BackToMainMenuWithDelay()
    {
        SceneManager.LoadScene(0);
    }
    
    private void QuitGameWithDelay()
    {
        Application.Quit();
    }
}
