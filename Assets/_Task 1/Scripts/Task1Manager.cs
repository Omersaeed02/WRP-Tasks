using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Task1Manager : MonoBehaviour
{
    public bool makeObstaclesUnique;
    
    public PlayerController playerController;
    public CameraHandler cameraHandler;
    public Transform mobDeleteTrigger;
    
    public PlaneSO planeSO;
    public MobSO rideableMobSO;

    private int _score = 0;
    
    public int Score
    {
        get => _score;
        set
        {
            _score = value;
            UiManager.Instance.scoreText.text = "SCORE : " + _score;
        }
    }
    
    private float _lastTriggerTime = -Mathf.Infinity;
    private float _triggerCooldown = 1f;
    private int _currentPlaneIndex;
    
    public List<PlaneHandler> spawnedPlanes { get; private set; }
    public List<MobHandler> spawnedMobs { get; private set; }
    
    
    private void Awake()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        Application.targetFrameRate = 120;
        #endif
        
        playerController.SetTaskManager(this);
        
        spawnedPlanes = new List<PlaneHandler>();
        spawnedMobs = new List<MobHandler>();
        
        for (var i = 0; i < 9; i++)
        {
            var plane = Instantiate(planeSO.planes[i % 3]);
            
            plane.planeIndex = i;
            plane.RedoObstacles();
            
            if (_currentPlaneIndex == 0) plane.transform.position = new Vector3(0, 0, -20);
            else plane.transform.position = spawnedPlanes[_currentPlaneIndex - 1].transform.position + new Vector3(0, 0, 20f);
            
            _currentPlaneIndex++;
            
            spawnedPlanes.Add(plane);
        }

        _currentPlaneIndex = 1;
    }
    
    private void OnEnable()
    {
        PlayerController.OnPlayerDowned += PlayerDowned;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerDowned -= PlayerDowned;
    }

    private void Start()
    {
        AudioManager.Instance?.PlayTask1BackgroundMusic();
    }

    private void PlayerDowned()
    {
        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.DeathSound();
        Invoke(nameof(DelayedDeathFunctions), 0.5f);
    }

    private void DelayedDeathFunctions()
    {
        UiManager.Instance.ShowGameOverMenu();
    }
    
    public void PlayerTriggeredPlaneEnd()
    {
        if (Time.time - _lastTriggerTime < _triggerCooldown)
            return;

        _lastTriggerTime = Time.time;
        
        SpawnMobs();
        
        ChangePosition();
        _currentPlaneIndex++;
    }

    private void SpawnMobs()
    {
        var mobIndex = Random.Range(0, rideableMobSO.mobs.Count);

        var mob = Instantiate(rideableMobSO.mobs[mobIndex]);
        mob.transform.position = new Vector3(Random.Range(-4.5f, 5.5f), 0f, 60 + mobDeleteTrigger.position.z);
        spawnedMobs.Add(mob);
    }

    private void ChangePosition()
    {
        if (_currentPlaneIndex < 4) return;

        spawnedPlanes.ForEach(t => t.planeEndTrigger.enabled = true);
        
        _currentPlaneIndex = 1;
        
        var lastPosition = spawnedPlanes[spawnedPlanes.Count - 1].transform.position;

        var planesToMove = spawnedPlanes.GetRange(0, 3);

        for (var i = 0; i < planesToMove.Count; i++)
        {
            var randomIndex = Random.Range(i, planesToMove.Count);

            (planesToMove[i], planesToMove[randomIndex]) = (planesToMove[randomIndex], planesToMove[i]);
        }

        spawnedPlanes.RemoveRange(0, 3);

        foreach (var plane in planesToMove)
        {
            lastPosition += new Vector3(0, 0, 20f);
            plane.transform.position = lastPosition;
            if (makeObstaclesUnique) plane.RedoObstacles();
            else plane.ChangeAllPositions();
            spawnedPlanes.Add(plane);
        }

    }

    private void Update()
    {
        mobDeleteTrigger.position = playerController.mobDeleteTrigger.position;

        Score = (int)playerController.transform.position.z;
    }
}

#region Helpers

public static class Probability
{
    public struct RandomSelection
    {
        private int _minValue;
        private int _maxValue;
        public float _probability;

        public RandomSelection(int minValue, int maxValue, float probability)
        {
            _minValue = minValue;
            _maxValue = maxValue;
            _probability = probability;
        }

        public RandomSelection(int singleValue, float probability)
        {
            _minValue = singleValue;
            _maxValue = singleValue;
            _probability = probability;
        }
        
        public int GetValue()
        {
            return Random.Range(_minValue, _maxValue + 1);
        }

    }


    public static int GetRandomValue(params RandomSelection[] selections)
    {
        var rand = Random.value;
        var currentProb = 0f;
        foreach (var selection in selections)
        {
            currentProb += selection._probability;
            if (rand <= currentProb)
                return selection.GetValue();
        }

        //will happen if the input's probabilities sums to less than 1
        //throw error here if that's appropriate
        return -1;
    }
}

#endregion
