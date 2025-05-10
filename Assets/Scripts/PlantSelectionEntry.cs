using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
public class PlantSelectionEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [field: Header("References")]
    [field: SerializeField] public PlantData Data { get; private set; } = null;
    [field: SerializeField] public Animator Animator { get; private set; } = null;
    [field: SerializeField] public UnityEngine.UI.Image PlantImage { get; private set; } = null;
    [field: SerializeField] public TMP_Text PlantName { get; private set; } = null;
    [field: SerializeField] public StudioEventEmitter EventReference { get; private set; } = null;


    private void Start()
    {
        if (Data == null)
            return;

        PlantImage.sprite = Data.Icon;
        PlantName.text = Data.Name;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Animator.SetTrigger("Open");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Animator.SetTrigger("Close");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Game.Instance.CurrentState != GameState.Playing)
            return;

        Game.Instance.PlacementController.SelectPlant(Data, this);
        EventReference.Play();
    }

    public void OnSelect()
    {
        Animator.SetBool("Selected", true);
    }

    public void OnDeselect()
    {
        Animator.SetBool("Selected", false);
    }
}