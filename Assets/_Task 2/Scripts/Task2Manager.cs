using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Task2Manager : MonoBehaviour
{
    public Task2CameraManager task2CameraManager;
    
    private void Awake()
    {
        AudioManager.Instance?.PlayTask2BackgroundMusic();
        Debug.Log("Task 2");
    }
    
    public static Vector3 ConvertOriginWrtObject(Transform origin, Transform change)
    {
        var offset = origin.position - change.position;

        var inverseBRotation = MyInverse(change.rotation);

        var localPosInB = RotateVectorByQuaternion(offset, inverseBRotation);

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
        var u = new Vector3(q.x, q.y, q.z);
        var s = q.w;

        var uCrossV = Vector3.Cross(u, v);
        var rotated = v + 2f * Vector3.Cross(u, s * v + uCrossV);

        return rotated;
    }
}
