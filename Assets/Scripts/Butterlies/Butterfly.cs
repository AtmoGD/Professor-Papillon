using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public class Butterfly : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public ButterflyData Data { get; private set; } = null;
    [field: SerializeField] public Animator Animator { get; private set; } = null;
    [field: SerializeField] public Transform ModelHolder { get; private set; } = null;

    [field: Header("Pathfinding")]
    [field: SerializeField] public float PlantOrbitRadius { get; private set; } = 3f;
    [field: SerializeField] public float PlantOrbitMinHeight { get; private set; } = 1f;
    [field: SerializeField] public float PlantOrbitMaxHeight { get; private set; } = 2f;
    [field: SerializeField] public float PathGenerationThreshold { get; private set; } = 1;
    [field: SerializeField] public float PathReachedThreshold { get; private set; } = 2f;
    [field: SerializeField] public int MaxPathBreaks { get; private set; } = 10;
    [field: SerializeField] public float MinSpeed { get; private set; } = 1f;
    [field: SerializeField] public float MaxSpeed { get; private set; } = 2f;
    [field: SerializeField] public float RandomSpeedIntervallMin { get; private set; } = 0.5f;
    [field: SerializeField] public float RandomSpeedIntervallMax { get; private set; } = 1f;
    [field: SerializeField] public float RotationSpeed { get; private set; } = 1f;
    [field: SerializeField] public float AnimationSpeed { get; private set; } = 1f;
    [field: SerializeField] public float AnimationSpeedMin { get; private set; } = 1f;
    [field: SerializeField] public float AnimationSpeedMax { get; private set; } = 4f;
    [field: SerializeField] public float noiseScale { get; private set; } = 1f;
    [field: SerializeField] public float LevelMinRadius { get; private set; } = 10f;
    [field: SerializeField] public float LevelMaxRadius { get; private set; } = 15f;
    [field: SerializeField] public float LevelMinHeight { get; private set; } = 0.1f;
    [field: SerializeField] public float LevelMaxHeight { get; private set; } = 3f;
    [field: SerializeField] public int FadeOutBeforeDestroy { get; private set; } = 3;


    [SerializeField] private List<Plant> parentPlants = new List<Plant>();
    private Vector3 _targetPosition = Vector3.zero;
    [SerializeField] private List<Vector3> _path = new List<Vector3>();

    private bool isExitingLevel = false;
    private float currentSpeed = 0f;
    private Vector3 currentTarget = Vector3.zero;
    private float checkSpeedIn = 0.5f;
    private float checkAnimationSpeedIn = 0.5f;

    public void EnterLevel()
    {
        transform.position = GetRandomPositionOutsideOfLevel();

        SetSpeed();

        GeneratePath();

        FadeIn();
    }

    public void ExitLevel()
    {
        isExitingLevel = true;
        GeneratePath();
    }

    public void FadeIn()
    {
        Animator.SetBool("FadeIn", true);
    }

    public void FadeOut()
    {
        Animator.SetBool("FadeOut", true);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    [BurstCompile]
    private Vector3 GetRandomPositionOutsideOfLevel()
    {
        Vector2 randomPosition = Random.insideUnitCircle * Random.Range(LevelMinRadius, LevelMaxRadius);
        float randomHeight = Random.Range(LevelMinHeight, LevelMaxHeight);
        return new Vector3(randomPosition.x, randomHeight, randomPosition.y);
    }

    [BurstCompile]
    private void Update()
    {
        UpdateSpeed();

        if (parentPlants.Count == 0)
            return;

        if (_path.Count == 0 && !isExitingLevel)
            GeneratePath();

        if (isExitingLevel && _path.Count == FadeOutBeforeDestroy)
            FadeOut();

        if (isExitingLevel && _path.Count == 0)
            DestroySelf();

        if (_path.Count > 0)
        {
            UpdatePosition();

            UpdateRotation();

            if (Vector3.Distance(transform.position, currentTarget) < PathReachedThreshold)
            {
                _path.RemoveAt(0);

                if (_path.Count > 0)
                    currentTarget = _path[0];
            }
        }
    }

    [BurstCompile]
    private void UpdatePosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, currentSpeed * Time.deltaTime);
    }

    [BurstCompile]
    private void UpdateRotation()
    {
        Vector3 targetDirection = currentTarget - transform.position;
        targetDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion rotationOffset = Quaternion.Euler(0, 0, 0);
        ModelHolder.rotation = Quaternion.Slerp(ModelHolder.rotation, targetRotation * rotationOffset, Time.deltaTime * RotationSpeed);
    }

    [BurstCompile]
    private void UpdateSpeed()
    {
        checkSpeedIn -= Time.deltaTime;
        checkAnimationSpeedIn -= Time.deltaTime;

        if (checkSpeedIn <= 0)
        {
            SetSpeed();
            checkSpeedIn = Random.Range(RandomSpeedIntervallMin, RandomSpeedIntervallMax);
        }

        if (checkAnimationSpeedIn <= 0)
        {
            SetAnimationSpeed();
            checkAnimationSpeedIn = Random.Range(RandomSpeedIntervallMin, RandomSpeedIntervallMax);
        }
    }

    [BurstCompile]
    private void SetSpeed()
    {
        currentSpeed = Random.Range(MinSpeed, MaxSpeed);
    }

    [BurstCompile]
    private void SetAnimationSpeed()
    {
        Animator.SetFloat("Speed", Random.Range(AnimationSpeedMin, AnimationSpeedMax));
    }

    [BurstCompile]
    public void ClearCombination()
    {
        parentPlants.Clear();
    }

    [BurstCompile]
    public void AddParentPlant(Plant plant)
    {
        if (plant == null)
        {
            Debug.LogError("Plant is null");
            return;
        }

        if (parentPlants.Contains(plant))
            return;

        parentPlants.Add(plant);
    }

    [BurstCompile]
    public void SetTargetPosition(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
    }

    [BurstCompile]
    public void GeneratePath()
    {
        if (parentPlants.Count == 0)
            return;

        _path.Clear();

        _targetPosition = isExitingLevel ? GetRandomPositionOutsideOfLevel() : GetTargetPosition();

        Vector3 lastPosition = transform.position;

        float distanceToTarget = Vector3.Distance(lastPosition, _targetPosition);

        while (distanceToTarget > PathGenerationThreshold && _path.Count < MaxPathBreaks)
        {
            Vector3 direction = (_targetPosition - lastPosition).normalized;

            Vector3 noise = new Vector3(
                Mathf.PerlinNoise(lastPosition.x * noiseScale, lastPosition.y * noiseScale),
                Mathf.PerlinNoise(lastPosition.x * noiseScale, lastPosition.z * noiseScale),
                Mathf.PerlinNoise(lastPosition.y * noiseScale, lastPosition.z * noiseScale)
            );

            lastPosition += direction + noise;

            _path.Add(lastPosition);

            distanceToTarget = Vector3.Distance(lastPosition, _targetPosition);
        }

        if (_path.Count == 0)
            return;

        currentTarget = _path[0];
    }

    [BurstCompile]
    private Vector3 GetTargetPosition()
    {
        Transform randomPlant = parentPlants[Random.Range(0, parentPlants.Count)].transform;

        Vector2 randomPosition = Random.insideUnitCircle * PlantOrbitRadius;
        float randomHeight = Random.Range(PlantOrbitMinHeight, PlantOrbitMaxHeight);

        Vector3 targetPosition = randomPlant.position + new Vector3(randomPosition.x, randomHeight, randomPosition.y);

        return targetPosition;
    }

    private void OnDrawGizmos()
    {
        if (_path == null)
            return;

        Gizmos.color = Color.red;

        for (int i = 0; i < _path.Count; i++)
        {
            Gizmos.DrawSphere(_path[i], 0.1f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_targetPosition, 0.1f);
    }


}
