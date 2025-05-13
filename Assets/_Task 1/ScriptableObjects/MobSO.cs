using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MobSO", menuName = "ScriptableObjects/MobSO")]
public class MobSO : ScriptableObject
{
    public List<MobHandler> mobs = new();
}
