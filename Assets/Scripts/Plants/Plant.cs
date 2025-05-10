using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Plant : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public PlantData Data { get; private set; } = null;
    [field: SerializeField] public Animator Animator { get; private set; } = null;

    [field: SerializeField] public LayerMask PlantLayer { get; private set; } = 7;
    [field: SerializeField] public Renderer Renderer { get; private set; } = null;
    [field: SerializeField] public Material placmentIndicatorMaterial { get; private set; } = null;

    private List<GameObject> currentCollisions = new List<GameObject>();

    public bool IsCollidingWithOtherPlants => currentCollisions.Count > 0;

    public bool IsPlacementIndicator { get; private set; } = false;
    public List<Material> Materials { get; private set; } = new List<Material>();

    void Update()
    {
        if (IsPlacementIndicator)
        {
            Materials.ForEach(material => material.SetInt("_Error", IsCollidingWithOtherPlants ? 1 : 0));
        }
    }

    public void SetIsPlacementIndicator()
    {
        IsPlacementIndicator = true;

        List<Material> materials = new List<Material>();
        foreach (var material in Renderer.materials)
        {
            materials.Add(placmentIndicatorMaterial);
        }
        Renderer.SetMaterials(materials);
        Materials = materials;
    }

    void OnTriggerEnter(Collider other)
    {
        if ((PlantLayer & (1 << other.gameObject.layer)) != 0 && !currentCollisions.Contains(other.gameObject))
        {
            currentCollisions.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        currentCollisions.Remove(other.gameObject);
    }
}
