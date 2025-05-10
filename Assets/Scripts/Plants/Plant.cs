using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public class Plant : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public PlantData Data { get; private set; } = null;
    [field: SerializeField] public Animator Animator { get; private set; } = null;

    [field: SerializeField] public LayerMask PlantLayer { get; private set; } = 7;
    [field: SerializeField] public List<Renderer> Renderer { get; private set; } = null;
    [field: SerializeField] public Material placmentIndicatorMaterial { get; private set; } = null;
    [field: SerializeField] public float CheckWelkingInterval { get; private set; } = 4f;
    [field: SerializeField] public Color BaseColor { get; private set; } = Color.white;
    [field: SerializeField] public Color WelkingColor { get; private set; } = Color.red;
    [field: SerializeField] public float WelkingSpeed { get; private set; } = 1f;
    [field: SerializeField] public List<ParticleSystem> particleSystems { get; private set; } = new List<ParticleSystem>();

    private List<GameObject> currentCollisions = new List<GameObject>();

    public bool IsCollidingWithOtherPlants => currentCollisions.Count > 0;

    public bool IsPlacementIndicator { get; private set; } = false;
    public List<Material> Materials { get; private set; } = new List<Material>();


    [SerializeField] private bool isWelking = false;
    [SerializeField] private bool wasWelking = false;
    private float welkProgress = 0f;
    private float currentWelkingInterval = 0f;

    private bool isDeleted = false;

    void Update()
    {
        if (IsPlacementIndicator)
        {
            Materials.ForEach(material => material.SetInt("_Error", IsCollidingWithOtherPlants ? 1 : 0));
        }
        else
        {
            CheckWelking();
            if (isWelking || wasWelking)
                UpdateWelking();
        }
    }

    private void Start()
    {
        if (!IsPlacementIndicator && particleSystems.Count > 0)
        {
            foreach (var particleSystem in particleSystems)
                particleSystem.Play();
        }
    }

    private void CheckWelking()
    {
        currentWelkingInterval -= Time.deltaTime;

        if (currentWelkingInterval <= 0)
        {
            currentWelkingInterval = CheckWelkingInterval;

            FindPlantsForWelking();
        }
    }

    private void FindPlantsForWelking()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, Game.Instance.PlantNearRadius, PlantLayer);

        bool found = false;

        if (colliders.Length > 0)
        {
            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject && collider.TryGetComponent<Plant>(out Plant plant))
                {
                    if (!plant.IsPlacementIndicator)
                    {
                        Data.Dislikes.ForEach(dislike =>
                        {
                            if (dislike.Plants.Contains(plant.Data))
                            {
                                found = true;
                            }
                        });
                    }
                }
            }
        }

        if (found)
        {
            isWelking = true;
            wasWelking = true;
        }
        else
        {
            isWelking = false;
        }
    }

    private void UpdateWelking()
    {
        welkProgress += Time.deltaTime * WelkingSpeed * (isWelking ? 1 : -1);
        welkProgress = Mathf.Clamp01(welkProgress);

        Color currentColor;

        if (isWelking)
            currentColor = Color.Lerp(BaseColor, WelkingColor, welkProgress);
        else
            currentColor = Color.Lerp(WelkingColor, BaseColor, welkProgress);

        foreach (var renderer in Renderer)
        {
            foreach (var material in renderer.materials)
            {
                material.SetColor("_BaseColor", currentColor);
            }
        }

        if (welkProgress >= 1f)
        {
            DeleteThisPLant();
        }
        else if (welkProgress <= 0f)
        {
            wasWelking = false;
            isWelking = false;
        }
    }

    private void DeleteThisPLant()
    {
        if (isDeleted) return;

        isDeleted = true;

        Game.Instance.PlacementController.ActivePlants.Remove(this);
        Game.Instance.PlacementController.CheckCombinations();

        if (particleSystems.Count > 0)
        {
            foreach (var particleSystem in particleSystems)
                particleSystem.Play();
        }

        Animator.SetTrigger("PopOut");

        Destroy(gameObject, 2f);
    }

    public void SetIsPlacementIndicator()
    {
        IsPlacementIndicator = true;



        foreach (var renderer in Renderer)
        {
            List<Material> materials = new List<Material>();

            foreach (var material in renderer.materials)
            {
                materials.Add(placmentIndicatorMaterial);
                Materials.Add(material);
            }

            renderer.SetMaterials(materials);
        }
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
