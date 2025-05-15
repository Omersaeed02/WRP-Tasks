using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Probability;
using Random = UnityEngine.Random;

public class PlaneHandler : MonoBehaviour
{
    public int planeIndex;
    
    public Transform obstacleParent;
    public Collider planeEndTrigger;
    public ObstacleSO obstacleSO;
    // public List<Transform> obstacles;
    
    private readonly (float negX, float posX, float negZ, float posZ) _obstacleSpawnRange = (-2.5f, 3.5f, -4f, 4f);

    private List<Transform> _spawnedObstacles = new();

    public void RedoObstacles()
    {
        foreach (var t in _spawnedObstacles) Destroy(t.gameObject);
        _spawnedObstacles.Clear();

        SpawnObstacles();
    }

    private void SpawnObstacles()
    {
        if (planeIndex < 2) return;
        
        var spawnAmount = GetRandomValue(new RandomSelection(1, 2, 0.8f), new RandomSelection(3, 0.2f));
        
        for (var i = 0; i < spawnAmount; i++)
        {
            var x = Random.Range(_obstacleSpawnRange.negX, _obstacleSpawnRange.posX);
            var z = Random.Range(_obstacleSpawnRange.negZ, _obstacleSpawnRange.posZ);
            

            var obstacle = Instantiate(obstacleSO.obstacles[Random.Range(0, obstacleSO.obstacles.Count)], obstacleParent);
            _spawnedObstacles.Add(obstacle);
            
            obstacle.localPosition = new Vector3(x, 0f, z);
        }
    }
    
    public void ChangeAllPositions()
    {
        foreach (var t in _spawnedObstacles)
        {
            var x = Random.Range(_obstacleSpawnRange.negX, _obstacleSpawnRange.posX);
            var z = Random.Range(_obstacleSpawnRange.negZ, _obstacleSpawnRange.posZ);
            
            t.localPosition = new Vector3(x, 0.1f, z);
        }
    }
}

