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
    [field: SerializeField] public bool UserRightClickForHeight { get; private set; } = false;

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
        cameraTransform = Game.Instance.MainCamera;

        SetStartValues();
    }

    private void Update()
    {
        // if (Game.Instance.CurrentState != GameState.Playing)
        //     return;

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
        if (Game.Instance.InputController.ShiftClick && Game.Instance.InputController.MiddleClick)
        {
            UpdateTargetFocusPosition();
        }
        else
        {
            if (Game.Instance.InputController.MiddleClick)
                UpdateTargetRotation();

            if ((UserRightClickForHeight && Game.Instance.InputController.RightClick) || Game.Instance.InputController.MiddleClick)
                UpdateTargetHeight();
        }

        if (Game.Instance.InputController.MouseScroll != 0)
            UpdateTargetZoom();
    }

    private void UpdateTargetFocusPosition()
    {
        if (!IsOverRotationTheshold() && !IsOverHeightTheshold()) return;

        // Calculate the new target Position based on the main camera right Vector3
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



































































//  Vector3 newPosition = Vector3.zero;

//         // Rotate the camera around the target position based on the angle and the distance
//         Vector3 targetPosition = enviromentCenterTransform.position + Quaternion.Euler(0, targetPositionAngle, 0) * new Vector3(0, 0, -targetZoom);
//         targetPosition = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime * RotationSpeed);
//         newPosition.x = targetPosition.x;
//         newPosition.z = targetPosition.z;

//         // Set the camera height based on the target height
//         Vector3 targetHeightPosition = new Vector3(cameraTransform.position.x, targetHeight, cameraTransform.position.z);
//         targetHeightPosition = Vector3.Lerp(cameraTransform.position, targetHeightPosition, Time.deltaTime * HeightSpeed);
//         newPosition.y = targetHeightPosition.y;

//         cameraTransform.position = newPosition;

//         // // Move the camera forward and backward based on zoom
//         // Vector3 direction = transform.forward * targetZoom;
//         // cameraTransform.position = Vector3.Lerp(transform.position, direction, Time.deltaTime * ZoomSpeed);

//         // // Tilt the camera up and down
//         // Vector3 tiltRotation = new Vector3(targetTilt, 0, 0);
//         // cameraTransform.eulerAngles = Vector3.Lerp(transform.eulerAngles, tiltRotation, Time.deltaTime * TiltSpeed);
