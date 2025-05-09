using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlantData", menuName = "Papillon/Plant")]
public class PlantData : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; } = "New Plant";
    [field: SerializeField] public GameObject Prefab { get; private set; } = null;
    [field: SerializeField] public Sprite Icon { get; private set; } = null;
    [field: SerializeField] public List<PlantCombination> Dislikes { get; private set; } = null;
}
