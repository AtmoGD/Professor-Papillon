using UnityEngine;

public class Plant : MonoBehaviour
{

    [field: Header("References")]
    [field: SerializeField] public PlantData Data { get; private set; } = null;
    [field: SerializeField] public Animator Animator { get; private set; } = null;
}
