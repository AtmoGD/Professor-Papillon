using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using Unity.Burst;

[BurstCompile]
public class PlacementController : MonoBehaviour
{
    [field: Header("Settings")]
    [field: SerializeField] public LayerMask GroundLayer { get; private set; } = 6;
    [field: SerializeField] public LayerMask PlantLayer { get; private set; } = 7;
    [field: SerializeField] public List<ButterflyData> ButterflyDatas { get; private set; } = new List<ButterflyData>();
    [field: SerializeField] public float PlantNearRadius { get; private set; } = 4f;
    [field: SerializeField] public Material placmentIndicatorMaterial { get; private set; } = null;


    public PlantData SelectedPlantData { get; private set; } = null;
    private PlantSelectionEntry currentPlantSelectionEntry = null;

    [field: SerializeField] public List<Plant> ActivePlants { get; set; } = new List<Plant>();
    [field: SerializeField] public List<ActiveCombination> ActiveCombinations { get; set; } = new List<ActiveCombination>();
    [field: SerializeField] public List<Butterfly> ActiveButterflies { get; set; } = new List<Butterfly>();

    private Plant placementIndicator = null;

    [BurstCompile]
    private void Update()
    {
        if (Game.Instance.CurrentState != GameState.Playing)
            return;

        if (!currentPlantSelectionEntry)
            return;

        Ray ray = Game.Instance.MainCamera.ScreenPointToRay(Game.Instance.InputController.MousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, GroundLayer))
        {
            if (placementIndicator == null)
            {
                placementIndicator = Instantiate(SelectedPlantData.Prefab).GetComponent<Plant>();
                placementIndicator.SetIsPlacementIndicator();
            }

            placementIndicator.transform.position = hit.point;
            placementIndicator.transform.eulerAngles = hit.normal;

        }
        else if (placementIndicator != null)
        {
            Destroy(placementIndicator.gameObject);
        }

        if (Game.Instance.InputController.LeftClick && placementIndicator != null && !placementIndicator.IsCollidingWithOtherPlants)
        {
            Game.Instance.InputController.ConsumeLeftClick();
            PlacePlant();
        }
    }

    [BurstCompile]
    private void PlacePlant()
    {
        if (placementIndicator == null)
            return;

        Plant newPlant = Instantiate(SelectedPlantData.Prefab).GetComponent<Plant>();
        newPlant.gameObject.transform.position = placementIndicator.transform.position;
        newPlant.gameObject.transform.eulerAngles = placementIndicator.transform.eulerAngles;

        ActivePlants.Add(newPlant);

        Destroy(placementIndicator.gameObject);
        placementIndicator = null;

        ImprovedCheckCombinations();
    }

    [BurstCompile]
    private void ImprovedCheckCombinations()
    {
        List<Butterfly> butterfliesToAdd = new List<Butterfly>(ActiveButterflies);
        ActiveButterflies.Clear();

        ActiveButterflies.Clear();

        List<ActiveCombination> newCombinations = new List<ActiveCombination>();

        ActivePlants.ForEach((plantToCheck) =>
        {
            List<Plant> plantsInReach = new List<Plant>();
            List<Collider> colliderInReach = new List<Collider>(Physics.OverlapSphere(plantToCheck.transform.position, PlantNearRadius, PlantLayer));

            colliderInReach.ForEach(collider =>
            {
                Plant otherPlant = collider.GetComponent<Plant>();
                if (otherPlant != plantToCheck && otherPlant != null && otherPlant != plantToCheck && !otherPlant.IsPlacementIndicator)
                {
                    plantsInReach.Add(otherPlant);
                }
            });

            plantsInReach.Add(plantToCheck);

            if (plantsInReach.Count > 0)
            {
                ButterflyData bestButterflyData = null;
                PlantCombination bestCombination = null;
                List<Plant> plantsForThisCombination = new List<Plant>();

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

                        isCombinationPossible = isCombinationPossible && combination.Plants.Exists(p => p == plantToCheck.Data);

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

                            if (bestCombination != null)
                            {
                                plantsForThisCombination.Clear();
                                List<Plant> plantsForThisCombinationLeft = new List<Plant>(plantsInReach);

                                plantsForThisCombination.Add(plantToCheck);
                                plantsForThisCombinationLeft.Remove(plantToCheck);

                                bool addedPlantToCheck = false;

                                bestCombination.Plants.ForEach(plantData =>
                                {
                                    Plant plantToAdd = plantsForThisCombinationLeft.Find(p => p.Data == plantData);
                                    if (plantToAdd != null)
                                    {
                                        if (plantToAdd.Data == plantToCheck.Data && !addedPlantToCheck)
                                        {
                                            addedPlantToCheck = true;
                                        }
                                        else
                                        {
                                            plantsForThisCombination.Add(plantToAdd);
                                            plantsForThisCombinationLeft.Remove(plantToAdd);
                                        }
                                    }
                                });
                            }
                        }
                    });
                });

                if (bestButterflyData != null && bestCombination != null)
                {
                    bool isCombinationAlreadyActive = newCombinations.Exists(combination =>
                    {
                        bool isSamePlants = combination.Plants.Count == bestCombination.Plants.Count;
                        if (isSamePlants)
                        {
                            plantsForThisCombination.ForEach(plantCheck =>
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
                        activeCombination.Butterflies = new List<Butterfly>();

                        plantsForThisCombination.ForEach(plantWhatEver =>
                        {
                            activeCombination.AddPlant(plantWhatEver);
                        });

                        int neededButterflies = bestCombination.Plants.Count;
                        for (int i = 0; i < neededButterflies; i++)
                        {
                            Butterfly butterfly = butterfliesToAdd.Find(b => b.Data == bestButterflyData);
                            bool isNewButterfly = false;
                            if (butterfly != null)
                            {
                                butterfliesToAdd.Remove(butterfly);
                                butterfly.ClearCombination();
                            }
                            else
                            {
                                butterfly = Instantiate(bestButterflyData.Prefab).GetComponent<Butterfly>();
                                isNewButterfly = true;
                            }

                            activeCombination.Plants.ForEach(plantComb =>
                            {
                                butterfly.AddParentPlant(plantComb);
                            });

                            if (isNewButterfly)
                                butterfly.EnterLevel();

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

                        newCombinations.Add(activeCombination);
                    }
                }
            }
        });

        ActiveCombinations.Clear();
        newCombinations.ForEach(combination =>
        {
            ActiveCombinations.Add(combination);
        });

        butterfliesToAdd.ForEach(butterfly =>
        {
            butterfly.ExitLevel();
        });
    }

    [BurstCompile]
    public void SelectPlant(PlantData plantData, PlantSelectionEntry plantSelectionEntry)
    {
        if (plantData == null)
            return;

        SelectedPlantData = plantData;

        if (currentPlantSelectionEntry != null)
        {
            currentPlantSelectionEntry.OnDeselect();
        }

        if (currentPlantSelectionEntry == plantSelectionEntry)
        {
            currentPlantSelectionEntry = null;
            return;
        }

        currentPlantSelectionEntry = plantSelectionEntry;

        if (currentPlantSelectionEntry != null)
        {
            currentPlantSelectionEntry.OnSelect();
        }
    }

    void OnDrawGizmos()
    {
        if (placementIndicator != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(placementIndicator.transform.position, PlantNearRadius);
        }
    }
}
