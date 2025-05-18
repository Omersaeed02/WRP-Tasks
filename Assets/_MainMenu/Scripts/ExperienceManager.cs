using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperienceManager : MonoBehaviour
{
    public SceneField taskOne;
    public SceneField taskTwo;

    private void Awake()
    {
#if UNITY_ANDROID
        Application.targetFrameRate = 120;
#endif
    }

    public void LoadTaskOne()
    {
        SceneManager.LoadScene(taskOne);
    }

    public void LoadTaskTwo()
    {
        SceneManager.LoadScene(taskTwo);
    }
}
