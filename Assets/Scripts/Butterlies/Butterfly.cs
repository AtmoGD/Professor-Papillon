using System.Collections.Generic;
using UnityEngine;

public class Butterfly : MonoBehaviour
{
    [field: Header("References")]
    [field: SerializeField] public ButterflyData Data { get; private set; } = null;
    [field: SerializeField] public Animator Animator { get; private set; } = null;

    [field: Header("Pathfinding")]
    [field: SerializeField] public float PlantOrbitRadius { get; private set; } = 3f;
    [field: SerializeField] public float MinPathThreshold { get; private set; } = 1;
    [field: SerializeField] public int MaxPathBreaks { get; private set; } = 10;
    [field: SerializeField] public float MinSpeed { get; private set; } = 1f;
    [field: SerializeField] public float MaxSpeed { get; private set; } = 2f;
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

        Plant randomPlant = activeCombination.Plants[Random.Range(0, activeCombination.Plants.Count)];

        this._targetPosition = randomPlant.transform.position + Random.insideUnitSphere * PlantOrbitRadius;

        Vector3 lastPosition = transform.position;

        float distanceToTarget = Vector3.Distance(lastPosition, this._targetPosition);

        while (distanceToTarget > MinPathThreshold && _path.Count < MaxPathBreaks)
        {
            // Get a direction to the target position and add noise to it
            Vector3 direction = (this._targetPosition - lastPosition).normalized;

            // Add noise to the direction
            Vector3 noise = new Vector3(
                Mathf.PerlinNoise(lastPosition.x * noiseScale, lastPosition.y * noiseScale),
                Mathf.PerlinNoise(lastPosition.x * noiseScale, lastPosition.z * noiseScale),
                Mathf.PerlinNoise(lastPosition.y * noiseScale, lastPosition.z * noiseScale)
            );

            lastPosition += direction + noise;
            _path.Add(lastPosition);

            distanceToTarget = Vector3.Distance(lastPosition, this._targetPosition);

            // Debug.Log($"Path point {i}: {_path[i]}");
        }
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
