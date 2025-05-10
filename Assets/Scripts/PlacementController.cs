using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlacementController : MonoBehaviour
{
    [field: Header("Settings")]
    [field: SerializeField] public LayerMask GroundLayer { get; private set; } = 6;
    [field: SerializeField] public LayerMask PlantLayer { get; private set; } = 7;
    [field: SerializeField] public List<ButterflyData> ButterflyDatas { get; private set; } = new List<ButterflyData>();
    [field: SerializeField] public float PlantNearRadius { get; private set; } = 4f;


    public PlantData SelectedPlantData { get; private set; } = null;
    private PlantSelectionEntry currentPlantSelectionEntry = null;

    [field: SerializeField] public List<Plant> ActivePlants { get; set; } = new List<Plant>();
    [field: SerializeField] public List<ActiveCombination> ActiveCombinations { get; set; } = new List<ActiveCombination>();
    [field: SerializeField] public List<Butterfly> ActiveButterflies { get; set; } = new List<Butterfly>();

    private Plant currentPlant = null;


    private void Update()
    {
        if (Game.Instance.CurrentState != GameState.Playing)
            return;

        if (!currentPlantSelectionEntry)
            return;

        Ray ray = Game.Instance.MainCamera.ScreenPointToRay(Game.Instance.InputController.MousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, GroundLayer))
        {
            if (currentPlant == null)
                currentPlant = Instantiate(SelectedPlantData.Prefab).GetComponent<Plant>();

            currentPlant.transform.position = hit.point;
            currentPlant.transform.eulerAngles = hit.normal;

        }
        else if (currentPlant != null)
        {
            Destroy(currentPlant.gameObject);
        }

        if (Game.Instance.InputController.LeftClick && currentPlant != null && !currentPlant.IsCollidingWithOtherPlants)
        {
            Game.Instance.InputController.ConsumeLeftClick();
            PlacePlant();
        }
    }

    private void PlacePlant()
    {
        if (currentPlant == null)
            return;

        Plant newPlant = Instantiate(SelectedPlantData.Prefab).GetComponent<Plant>();
        newPlant.gameObject.transform.position = currentPlant.transform.position;
        newPlant.gameObject.transform.eulerAngles = currentPlant.transform.eulerAngles;

        ActivePlants.Add(newPlant);

        Destroy(currentPlant.gameObject);
        currentPlant = null;

        CheckCombinations();
    }

    private void CheckCombinations()
    {
        ActiveCombinations.Clear();

        List<Butterfly> butterfliesToAdd = new List<Butterfly>(ActiveButterflies);
        ActiveButterflies.Clear();



        ActivePlants.ForEach(plant =>
        {
            List<Plant> plantsInReach = new List<Plant>();
            List<Collider> colliderInReach = new List<Collider>(Physics.OverlapSphere(plant.transform.position, PlantNearRadius, PlantLayer));

            colliderInReach.ForEach(collider =>
            {
                Plant otherPlant = collider.GetComponent<Plant>();
                if (otherPlant != null)
                {
                    plantsInReach.Add(otherPlant);
                }
            });

            if (plantsInReach.Count > 0)
            {
                ButterflyData bestButterflyData = null;
                PlantCombination bestCombination = null;

                ButterflyDatas.ForEach(butterflyData =>
                {
                    butterflyData.Likes.ForEach(combination =>
                    {
                        bool isCombinationPossible = true;
                        combination.Plants.ForEach(plantData =>
                        {
                            if (!plantsInReach.Exists(p => p.Data == plantData))
                            {
                                isCombinationPossible = false;
                            }
                        });

                        if (isCombinationPossible)
                        {
                            if (bestButterflyData == null || bestCombination == null)
                            {
                                bestButterflyData = butterflyData;
                                bestCombination = combination;
                            }
                            else if (bestCombination.Plants.Count < combination.Plants.Count)
                            {
                                bestButterflyData = butterflyData;
                                bestCombination = combination;
                            }
                        }
                    });
                });

                if (bestButterflyData != null && bestCombination != null)
                {
                    List<Plant> plantsToAdd = new List<Plant>();

                    bestCombination.Plants.ForEach(plantData =>
                    {
                        Plant plantToAdd = plantsInReach.Find(p => p.Data == plantData);
                        if (plantToAdd != null)
                        {
                            plantsToAdd.Add(plantToAdd);
                        }
                    });

                    bool isCombinationAlreadyActive = ActiveCombinations.Exists(combination =>
                    {
                        bool isSamePlants = combination.Plants.Count == plantsToAdd.Count;
                        if (isSamePlants)
                        {
                            plantsToAdd.ForEach(plantCheck =>
                            {
                                if (!combination.Plants.Contains(plantCheck))
                                {
                                    isSamePlants = false;
                                }
                            });
                        }

                        return isSamePlants;
                    });

                    if (!isCombinationAlreadyActive)
                    {
                        ActiveCombination activeCombination = new ActiveCombination();
                        activeCombination.Plants = new List<Plant>();
                        plantsToAdd.ForEach(plantWhatEver =>
                        {
                            activeCombination.AddPlant(plantWhatEver);
                        });
                        activeCombination.Butterflies = new List<Butterfly>();

                        int neededButterflies = bestCombination.Plants.Count;
                        for (int i = 0; i < neededButterflies; i++)
                        {
                            Butterfly butterfly = butterfliesToAdd.Find(b => b.Data == bestButterflyData);
                            if (butterfly != null)
                            {
                                butterfliesToAdd.Remove(butterfly);
                            }
                            else
                            {
                                butterfly = Instantiate(bestButterflyData.Prefab).GetComponent<Butterfly>();
                            }
                            activeCombination.Butterflies.Add(butterfly);
                            ActiveButterflies.Add(butterfly);
                        }

                        activeCombination.Butterflies.ForEach(butterfly =>
                        {
                            butterfly.ClearCombination();

                            activeCombination.Plants.ForEach(plantComb =>
                            {
                                butterfly.AddParentPlant(plantComb);
                            });
                        });

                        ActiveCombinations.Add(activeCombination);
                    }
                }
            }
        });

        Debug.Log($"Active Plants: {ActivePlants.Count}");
        Debug.Log($"Active Combinations: {ActiveCombinations.Count}");
        Debug.Log($"Active Butterflies: {ActiveButterflies.Count}");
    }

    public void SelectPlant(PlantData plantData, PlantSelectionEntry plantSelectionEntry)
    {
        if (plantData == null)
            return;

        SelectedPlantData = plantData;

        if (currentPlantSelectionEntry != null)
        {
            currentPlantSelectionEntry.OnDeselect();
        }

        currentPlantSelectionEntry = plantSelectionEntry;

        if (currentPlantSelectionEntry != null)
        {
            currentPlantSelectionEntry.OnSelect();
        }
    }
}
