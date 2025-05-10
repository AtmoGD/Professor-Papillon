using System.ComponentModel;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [field: Header("Camera Settings")]
    [field: SerializeField] public Transform OuterCameraHolder { get; private set; }
    [field: SerializeField] public Transform InnerCameraHolder { get; private set; }
    [field: SerializeField] public Transform CameraHolder { get; private set; } = null;

    [field: Header("Start Position")]
    [field: SerializeField] public float StartRotation { get; private set; } = 0f;
    [field: SerializeField] public float StartHeight { get; private set; } = 0f;
    [field: SerializeField] public float StartZoom { get; private set; } = 0f;

    [field: Header("Rotation")]
    [field: SerializeField] public float RotationSpeed { get; private set; } = 5f;
    [field: SerializeField] public float RotationLerpSpeed { get; private set; } = 5f;
    [field: SerializeField] public float RotationThreshold { get; private set; } = 0.1f;

    [field: Header("Height")]
    [field: SerializeField] public float MinHeight { get; private set; } = 5f;
    [field: SerializeField] public float MaxHeight { get; private set; } = 20f;
    [field: SerializeField] public float HeightSpeed { get; private set; } = 5f;
    [field: SerializeField] public float HeightLerpSpeed { get; private set; } = 5f;
    [field: SerializeField] public float HeightThreshold { get; private set; } = 0.1f;
    [field: SerializeField] public bool InvertY { get; private set; } = false;

    [field: Header("Zoom")]
    [field: SerializeField] public float MinZoom { get; private set; } = 5f;
    [field: SerializeField] public float MaxZoom { get; private set; } = 20f;
    [field: SerializeField] public float ZoomSpeed { get; private set; } = 5f;
    [field: SerializeField] public float ZoomLerpSpeed { get; private set; } = 5f;
    [field: SerializeField] public bool InvertZoom { get; private set; } = false;

    [field: Header("Target Movement")]
    [field: SerializeField] public float TargetMaxDistance { get; private set; } = 5f;
    [field: SerializeField] public float TargetMovementSpeed { get; private set; } = 5f;
    [field: SerializeField] public float TargetMovementLerpSpeed { get; private set; } = 5f;
    [field: SerializeField] public float TargetMovementThreshold { get; private set; } = 0.1f;
    [field: SerializeField] public bool InvertXTargetMovement { get; private set; } = false;
    [field: SerializeField] public bool InvertYTargetMovement { get; private set; } = false;


    [field: Header("Debug")]
    [SerializeField] private float currentAngle;
    [SerializeField] private float targetAngle;
    [SerializeField] private float currentHeight;
    [SerializeField] private float targetHeight;
    [SerializeField] private float currentZoom;
    [SerializeField] private float targetZoom;
    [SerializeField] private Vector3 currentFocusPosition = Vector3.zero;
    [SerializeField] private Vector3 targetFocusPosition = Vector3.zero;

    private Transform cameraTransform = null;

    private void Start()
    {
        cameraTransform = Game.Instance.MainCamera.transform;

        SetStartValues();
    }

    private void Update()
    {
        if (Game.Instance.CurrentState == GameState.Playing)
            UpdateTargets();

        UpdateCurrent();
    }

    private void SetStartValues()
    {
        currentFocusPosition = OuterCameraHolder.localPosition;
        targetFocusPosition = currentFocusPosition;

        currentAngle = OuterCameraHolder.localEulerAngles.y;
        targetAngle = StartRotation;

        currentHeight = InnerCameraHolder.localEulerAngles.x;
        targetHeight = StartHeight;

        currentZoom = CameraHolder.localScale.x;
        targetZoom = StartZoom;
    }

    private void UpdateTargets()
    {
        if (Game.Instance.InputController.MiddleClick)
            UpdateTargetFocusPosition();

        if (Game.Instance.InputController.RightClick)
        {
            UpdateTargetRotation();
            UpdateTargetHeight();
        }

        if (Game.Instance.InputController.MouseScroll != 0)
            UpdateTargetZoom();
    }

    private void UpdateTargetFocusPosition()
    {
        if (!IsOverRotationTheshold() && !IsOverHeightTheshold()) return;

        Vector3 movement = Vector3.zero;
        movement += cameraTransform.forward * Game.Instance.InputController.MoveDelta.y * (InvertYTargetMovement ? -1 : 1);
        movement += cameraTransform.right * Game.Instance.InputController.MoveDelta.x * (InvertXTargetMovement ? -1 : 1);
        targetFocusPosition += movement * TargetMovementSpeed;
        targetFocusPosition.x = Mathf.Clamp(targetFocusPosition.x, -TargetMaxDistance, TargetMaxDistance);
        targetFocusPosition.z = Mathf.Clamp(targetFocusPosition.z, -TargetMaxDistance, TargetMaxDistance);
    }

    private void UpdateTargetRotation()
    {
        if (!IsOverRotationTheshold()) return;

        targetAngle += Game.Instance.InputController.MoveDelta.x * RotationSpeed;
    }

    private bool IsOverRotationTheshold()
    {
        return Mathf.Abs(Game.Instance.InputController.MousePosition.x - Game.Instance.InputController.MiddleClickStart.x) > RotationThreshold;
    }

    private void UpdateTargetHeight()
    {
        if (!IsOverHeightTheshold()) return;

        targetHeight += Game.Instance.InputController.MoveDelta.y * HeightSpeed * (InvertY ? -1 : 1);
        targetHeight = Mathf.Clamp(targetHeight, MinHeight, MaxHeight);
    }

    private bool IsOverHeightTheshold()
    {
        return Mathf.Abs(Game.Instance.InputController.MousePosition.y - Game.Instance.InputController.MiddleClickStart.y) > HeightThreshold;
    }

    private void UpdateTargetZoom()
    {
        targetZoom += Game.Instance.InputController.MouseScroll * ZoomSpeed * (InvertZoom ? -1 : 1);
        targetZoom = Mathf.Clamp(targetZoom, MinZoom, MaxZoom);
    }

    private void UpdateCurrent()
    {
        currentFocusPosition = Vector3.Lerp(currentFocusPosition, targetFocusPosition, Time.deltaTime * TargetMovementLerpSpeed);
        Vector3 newTargetPosition = OuterCameraHolder.localPosition;
        newTargetPosition.x = currentFocusPosition.x;
        newTargetPosition.z = currentFocusPosition.z;
        OuterCameraHolder.localPosition = newTargetPosition;

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * RotationLerpSpeed);
        Vector3 outerAngles = OuterCameraHolder.localEulerAngles;
        outerAngles.y = currentAngle;
        OuterCameraHolder.localEulerAngles = outerAngles;

        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * HeightLerpSpeed);
        Vector3 innerAngles = InnerCameraHolder.localEulerAngles;
        innerAngles.x = currentHeight;
        InnerCameraHolder.localEulerAngles = innerAngles;

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * ZoomLerpSpeed);
        Vector3 newPosition = CameraHolder.localPosition;
        newPosition.z = currentZoom;
        CameraHolder.localScale = new Vector3(currentZoom, currentZoom, currentZoom);
    }
}