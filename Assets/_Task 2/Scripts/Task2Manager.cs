using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Task2Manager : MonoBehaviour
{
    private void Awake()
    {
        AudioManager.Instance?.PlayTask2BackgroundMusic();
        Debug.Log("Task 2");
    }
    
    public static Vector3 ConvertOriginWrtObject(Transform origin, Transform change)
    {
        var offset = origin.position - change.position;

        // 2. Manually compute inverse rotation (conjugate)
        var inverseBRotation = MyInverse(change.rotation);

        // 3. Apply inverse rotation to the offset (manual quaternion-vector rotation)
        var localPosInB = RotateVectorByQuaternion(offset, inverseBRotation);

        // (Optional) Adjust for scale
        localPosInB.x /= change.localScale.x;
        localPosInB.y /= change.localScale.y;
        localPosInB.z /= change.localScale.z;

        
        return localPosInB;
    }
    
    public static Quaternion MyInverse(Quaternion quaternion)
    {
        return new Quaternion(-quaternion.x, -quaternion.y, -quaternion.z, quaternion.w);
    }
    
    public static Vector3 RotateVectorByQuaternion(Vector3 v, Quaternion q)
    {
        // Extract the vector part of the quaternion
        var u = new Vector3(q.x, q.y, q.z);
        var s = q.w;

        // Apply rotation formula: v' = v + 2u × (s v + u × v)
        var uCrossV = Vector3.Cross(u, v);
        var rotated = v + 2f * Vector3.Cross(u, s * v + uCrossV);

        return rotated;
    }
}
