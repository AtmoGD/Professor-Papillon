using UnityEngine;
using System.Collections.Generic;

public class PlacementController : MonoBehaviour
{
    public PlantData SelectedPlantData { get; private set; } = null;

    public List<Plant> ActivePlants { get; set; } = new List<Plant>();
    public List<ActiveCombination> ActiveCombinations { get; set; } = new List<ActiveCombination>();
    public List<Butterfly> ActiveButterflies { get; set; } = new List<Butterfly>();

    public void SelectPlant(PlantData plantData)
    {
        if (plantData == null)
            return;

        SelectedPlantData = plantData;
    }
}
