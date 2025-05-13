using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleSO", menuName = "ScriptableObjects/ObstacleSO")]
public class ObstacleSO : ScriptableObject
{
    public List<Transform>  obstacles = new();
}
