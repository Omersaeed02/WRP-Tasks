using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenu;

    private Task2Manager _task2Manager;
    
    private void Start()
    {
        _task2Manager = GetComponent<Task2Manager>();
    }

    public void PauseGame()
    {
        _task2Manager.task2CameraManager.isCameraLocked = true;
        AudioManager.Instance?.PlayButtonSound();
        GetComponent<ObjectManipulation>().DeselectObject();
        pauseMenu.SetActive(true);
        pauseMenu.transform.DOScale(1f, 0.75f);
    }

    public void ResumeGame()
    {
        _task2Manager.task2CameraManager.isCameraLocked = false;
        AudioManager.Instance?.PlayButtonSound();
        pauseMenu.transform.DOScale(0f, 0.75f);
        Invoke(nameof(ResumeGameWithDelay), 0.75f);
    }
    
    private void ResumeGameWithDelay()
    {
        pauseMenu.SetActive(false);
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
