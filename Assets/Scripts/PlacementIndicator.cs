using System.Collections.Generic;
using UnityEngine;

public class PlacementIndicator : MonoBehaviour
{
    [field: SerializeField] public float CheckRadius { get; set; } = 0.5f;
    [field: SerializeField] private LayerMask PlantLayer { get; set; } = 7;
    public bool IsCollidingWithOtherPlants { get; private set; } = false;


    public void CheckCollisions()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, CheckRadius, PlantLayer);
        IsCollidingWithOtherPlants = colliders.Length > 0;
    }
}
