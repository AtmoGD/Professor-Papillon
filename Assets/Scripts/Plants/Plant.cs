using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Plant : MonoBehaviour
{

    [field: Header("References")]
    [field: SerializeField] public PlantData Data { get; private set; } = null;
    [field: SerializeField] public Animator Animator { get; private set; } = null;
    [field: SerializeField] public Collider PlacementCollider { get; private set; } = null;
    [field: SerializeField] public bool IsCollidingWithOtherPlants { get; private set; } = false;
    [field: SerializeField] public LayerMask PlantLayer { get; private set; } = 7;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == PlantLayer)
        {
            IsCollidingWithOtherPlants = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == PlantLayer)
        {
            IsCollidingWithOtherPlants = false;
        }
    }
}
