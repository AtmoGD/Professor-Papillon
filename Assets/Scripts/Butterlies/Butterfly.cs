using System.Collections.Generic;
using UnityEngine;

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
    [field: SerializeField] public float PathThreshold { get; private set; } = 2f;
    [field: SerializeField] public int MaxPathBreaks { get; private set; } = 10;
    [field: SerializeField] public float MinSpeed { get; private set; } = 1f;
    [field: SerializeField] public float MaxSpeed { get; private set; } = 2f;
    [field: SerializeField] public float RotationSpeed { get; private set; } = 1f;
    [field: SerializeField] public float AnimationSpeed { get; private set; } = 1f;
    [field: SerializeField] public float noiseScale { get; private set; } = 1f;


    [SerializeField] public ActiveCombination activeCombination = null;


    private Vector3 _targetPosition = Vector3.zero;
    [SerializeField] private List<Vector3> _path = new List<Vector3>();

    private void Start()
    {
        if (activeCombination == null)
            return;

        GeneratePath();
    }

    private void Update()
    {
        if (activeCombination == null)
            return;

        if (_path.Count == 0)
            GeneratePath();

        if (_path.Count > 0)
        {
            Vector3 target = _path[0];
            float speed = Random.Range(MinSpeed, MaxSpeed);

            Animator.SetFloat("Speed", speed * AnimationSpeed);

            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            Vector3 targetDirection = target - transform.position;
            targetDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion rotationOffset = Quaternion.Euler(0, 180, 0);
            ModelHolder.rotation = Quaternion.Slerp(ModelHolder.rotation, targetRotation * rotationOffset, Time.deltaTime * RotationSpeed);

            if (Vector3.Distance(transform.position, target) < PathThreshold)
            {
                _path.RemoveAt(0);
            }
        }
    }


    public void SetActiveCombination(ActiveCombination activeCombination)
    {
        this.activeCombination = activeCombination;
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        _targetPosition = targetPosition;
    }

    public void GeneratePath()
    {
        if (activeCombination == null)
            return;

        _path.Clear();

        _targetPosition = GetTargetPosition();

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
    }

    private Vector3 GetTargetPosition()
    {
        Plant randomPlant = activeCombination.Plants[Random.Range(0, activeCombination.Plants.Count)];

        Vector2 randomPosition = Random.insideUnitCircle * PlantOrbitRadius;
        float randomHeight = Random.Range(PlantOrbitMinHeight, PlantOrbitMaxHeight);

        Vector3 targetPosition = randomPlant.transform.position + new Vector3(randomPosition.x, randomHeight, randomPosition.y);

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
