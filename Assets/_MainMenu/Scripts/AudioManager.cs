using System;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(AudioManager), true)]
public class AudioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var audioManager = (AudioManager)target;
        serializedObject.Update();
        
        GUI.enabled = false;
        var prop = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField(prop, true);
        GUI.enabled = true;
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonSound"));
        
        switch (SceneManager.GetActiveScene().buildIndex)
        {
            case 0:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("task1BackgroundMusic"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("task2BackgroundMusic"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("deathSound"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("selectSound"));
                break;
            
            case 1:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("task1BackgroundMusic"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("deathSound"));
                break;
            
            case 2:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("task2BackgroundMusic"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("selectSound"));
                break;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
}

#endif

public class AudioManager : MonoBehaviour
{

    #region Singleton

    public static AudioManager Instance;
    private AudioSource _audioSource;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    #endregion

    public AudioClip task1BackgroundMusic;
    public AudioClip task2BackgroundMusic;
    public AudioClip buttonSound;
    public AudioClip deathSound;
    public AudioClip selectSound;

    public void PlayTask1BackgroundMusic()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
        _audioSource.clip = task1BackgroundMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }

    public void PlayTask2BackgroundMusic()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
        _audioSource.clip = task2BackgroundMusic;
        _audioSource.loop = true;
        _audioSource.Play();
    }
    
    public void StopMusic()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }

    public void PlaySelectSound()
    {
        _audioSource.PlayOneShot(selectSound);
    }
    
    public void DeathSound()
    {
        _audioSource.PlayOneShot(deathSound);
    }
    
    public void PlayButtonSound()
    {
        _audioSource.PlayOneShot(buttonSound);
    }
    
}
