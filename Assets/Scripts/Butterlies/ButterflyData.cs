using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlantCombination
{
    [field: SerializeField] public List<PlantData> Plants { get; private set; } = null;
}

[CreateAssetMenu(fileName = "Butterfly_", menuName = "Papillon/Butterfly")]
public class ButterflyData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; } = "New Butterfly";
    [field: SerializeField] public GameObject Prefab { get; private set; } = null;
    [field: SerializeField] public List<PlantCombination> Likes { get; private set; } = null;
}
