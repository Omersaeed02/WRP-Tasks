using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Task1Manager : MonoBehaviour
{
    public bool makeObstaclesUnique;
    
    public PlayerController playerController;
    public Transform mobDeleteTrigger;
    
    public PlaneSO planeSO;
    // public List<PlaneHandler> planes = new();
    
    public List<PlaneHandler> spawnedPlanes { get; private set; }

    public MobSO rideableMobSO;
    // public List<GameObject> rideableMobs = new();
    
    private int _currentPlaneIndex;

    
    private void Awake()
    {
        playerController.SetTaskManager(this);
        
        spawnedPlanes = new List<PlaneHandler>();
        
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
        if (playerController != null)
        {
            PlayerController.OnPlayerDowned += PlayerDowned;
        }
    }
    
    private void OnDisable()
    {
        if (playerController != null)
        {
            PlayerController.OnPlayerDowned -= PlayerDowned;
        }
    }

    public void PlayerDowned()
    {
        
    }
    
    public void PlayerTriggeredPlaneEnd()
    {
        Debug.Log("Change Plane Position");
        ChangePosition();
        _currentPlaneIndex++;
    }
    
    public void ChangePosition()
    {
        if (_currentPlaneIndex < 4) return;

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

        _currentPlaneIndex = 1;
    }

    private void Update()
    {
        mobDeleteTrigger.position = playerController.mobDeleteTrigger.position;
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
