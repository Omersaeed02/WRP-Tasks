using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlaneSO", menuName = "ScriptableObjects/PlaneSO")]
public class PlaneSO : ScriptableObject
{
    public List<PlaneHandler> planes = new();
}
