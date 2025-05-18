using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


[ExecuteAlways]
public class OverlapDetector : MonoBehaviour
{
    public ShapeHandler sphereReference;
    public ShapeHandler cuboidReference;

    public Material valid;
    public Material invalid;
    
    private void Update()
    {
        if (sphereReference == null || cuboidReference == null) return;

        var overlap = DoesOverlap(sphereReference.gameObject, cuboidReference.gameObject);
        
        if (overlap)
        {
            sphereReference.GetComponent<MeshRenderer>().material = valid;
            cuboidReference.GetComponent<MeshRenderer>().material = valid;
        }
        else
        {
            sphereReference.GetComponent<MeshRenderer>().material = invalid;
            cuboidReference.GetComponent<MeshRenderer>().material = invalid;
        }
    }

    public bool DoesOverlap(GameObject sphere, GameObject box)
    {
        var spherePos = sphere.transform.position;
        var sphereRadius = sphere.transform.lossyScale.x * 0.5f; 

        var boxPos = box.transform.position;
        var axes = new[] {
            box.transform.right,
            box.transform.up,
            box.transform.forward
        };
        
        var boxSize = box.transform.lossyScale * 0.5f;

        var shapesDelta = spherePos - boxPos;
        var closestPoint = boxPos;
        
        for (var i = 0; i < 3; i++)
        {
            var dotDist = GetDotProduct(shapesDelta, axes[i]);
            var clampedDist = GetClampedValue(dotDist, -boxSize[i], boxSize[i]);
            closestPoint += axes[i] * clampedDist;
        }

        var sqrDistance = (spherePos - closestPoint).sqrMagnitude;

        var isOverlap = (sqrDistance <= sphereRadius * sphereRadius);
        
        return isOverlap;
    }

    private static float GetClampedValue(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private static float GetDotProduct(Vector3 a, Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }
}