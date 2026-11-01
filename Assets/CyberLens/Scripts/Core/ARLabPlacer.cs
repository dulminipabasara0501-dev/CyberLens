using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Scans for a flat surface, then places the CyberLab prefab where the user taps.
// Also connects the token buttons to the lab after it appears.
public class ARLabPlacer : MonoBehaviour
{
    private enum PlacementState
    {
        Scanning,
        Placed
    }

    [Header("AR (both are on the XR Origin object)")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Camera arCamera;

    [Header("Lab")]
    [SerializeField] private GameObject labPrefab;
    [SerializeField] private float labScale = 0.12f;
    [SerializeField] private float placeInFrontDistance = 0.9f;
    [SerializeField] private float placeInFrontHeightOffset = -0.4f;

    [Header("UI")]
    [SerializeField] private GameObject scanPanel;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject labUiRoot;
    [SerializeField] private Button placeInFrontButton;
    [SerializeField] private Button moveLabButton;
    [SerializeField] private Button sendTokenButton;
    [SerializeField] private Button safeButton;
    [SerializeField] private Button unsafeButton;

    private static readonly List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    private PlacementState state = PlacementState.Scanning;
    private GameObject labInstance;
    private DataTokenMover tokenMover;
    private float ignoreTapsUntil;

    private void Awake()
    {
        AddListener(placeInFrontButton, OnPlaceInFrontClicked);
        AddListener(moveLabButton, OnMoveLabClicked);
        AddListener(sendTokenButton, OnSendTokenClicked);
        AddListener(safeButton, OnSafeClicked);
        AddListener(unsafeButton, OnUnsafeClicked);
    }

    private void OnDestroy()
    {
        RemoveListener(placeInFrontButton, OnPlaceInFrontClicked);
        RemoveListener(moveLabButton, OnMoveLabClicked);
        RemoveListener(sendTokenButton, OnSendTokenClicked);
        RemoveListener(safeButton, OnSafeClicked);
        RemoveListener(unsafeButton, OnUnsafeClicked);
    }

    private void Start()
    {
        EnterScanning();
    }

    private void Update()
    {
        if (state != PlacementState.Scanning)
        {
            return;
        }

        UpdateStatusText();

        if (Time.unscaledTime < ignoreTapsUntil || raycastManager == null)
        {
            return;
        }

        Vector2 tapPosition;
        if (!TryGetTap(out tapPosition))
        {
            return;
        }

        if (raycastManager.Raycast(tapPosition, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            PlaceLab(raycastHits[0].pose.position);
        }
    }

    // ----- Placement -----

    private void PlaceLab(Vector3 position)
    {
        if (labPrefab == null)
        {
            Debug.LogError("ARLabPlacer: assign the CyberLab prefab in the Inspector.");
            return;
        }

        Camera viewCamera = GetCamera();
        if (viewCamera == null)
        {
            Debug.LogError("ARLabPlacer: no camera found. Tag the AR Camera as MainCamera.");
            return;
        }

        // The lab's front faces -Z, so point its back away from the user.
        Vector3 awayFromUser = position - viewCamera.transform.position;
        awayFromUser.y = 0f;
        if (awayFromUser.sqrMagnitude < 0.0001f)
        {
            awayFromUser = viewCamera.transform.forward;
            awayFromUser.y = 0f;
        }
        Quaternion rotation = Quaternion.LookRotation(awayFromUser.normalized, Vector3.up);

        if (labInstance != null)
        {
            Destroy(labInstance);
        }

        labInstance = Instantiate(labPrefab, position, rotation);
        labInstance.transform.localScale = Vector3.one * labScale;

        LabTouchController touchController = labInstance.GetComponent<LabTouchController>();
        if (touchController != null)
        {
            touchController.ResetZoomBase();
        }

        tokenMover = labInstance.GetComponentInChildren<DataTokenMover>();
        if (tokenMover != null)
        {
            tokenMover.ShowSafe();
        }

        state = PlacementState.Placed;
        ApplyState();
    }

    private void OnPlaceInFrontClicked()
    {
        Camera viewCamera = GetCamera();
        if (viewCamera == null)
        {
            return;
        }

        Vector3 forward = viewCamera.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }
        forward.Normalize();

        Vector3 position = viewCamera.transform.position + forward * placeInFrontDistance;
        position.y = viewCamera.transform.position.y + placeInFrontHeightOffset;
        PlaceLab(position);
    }

    private void OnMoveLabClicked()
    {
        if (labInstance != null)
        {
            Destroy(labInstance);
            labInstance = null;
            tokenMover = null;
        }

        EnterScanning();
    }

    private void EnterScanning()
    {
        state = PlacementState.Scanning;
        ignoreTapsUntil = Time.unscaledTime + 0.4f; // Ignore the tap that pressed the button.
        ApplyState();
    }

    private void ApplyState()
    {
        bool scanning = state == PlacementState.Scanning;

        if (scanPanel != null)
        {
            scanPanel.SetActive(scanning);
        }
        if (labUiRoot != null)
        {
            labUiRoot.SetActive(!scanning);
        }

        if (planeManager != null)
        {
            if (scanning)
            {
                planeManager.enabled = true;
                planeManager.SetTrackablesActive(true);
            }
            else
            {
                planeManager.SetTrackablesActive(false);
                planeManager.enabled = false;
            }
        }
    }

    // ----- Token buttons -----

    private void OnSendTokenClicked()
    {
        if (tokenMover != null)
        {
            tokenMover.Play();
        }
    }

    private void OnSafeClicked()
    {
        if (tokenMover != null)
        {
            tokenMover.ShowSafe();
        }
    }

    private void OnUnsafeClicked()
    {
        if (tokenMover != null)
        {
            tokenMover.ShowUnsafe();
        }
    }

    // ----- Status text -----

    private void UpdateStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        if (ARSession.state == ARSessionState.Unsupported)
        {
            statusText.text = "This phone does not support AR. You can still place the lab in front of you.";
            return;
        }

        if (AnyPlaneFound())
        {
            statusText.text = "Surface found. Tap it to place the lab.";
        }
        else
        {
            statusText.text = "Move your phone slowly and point at a flat, well-lit surface.";
        }
    }

    private bool AnyPlaneFound()
    {
        if (planeManager == null)
        {
            return false;
        }

        foreach (ARPlane plane in planeManager.trackables)
        {
            return true;
        }
        return false;
    }

    // ----- Input helpers -----

    private bool TryGetTap(out Vector2 position)
    {
        position = Vector2.zero;

        Touchscreen touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame)
        {
            position = touchscreen.primaryTouch.position.ReadValue();
            int touchId = touchscreen.primaryTouch.touchId.ReadValue();
            return !IsOverUi(touchId);
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            position = mouse.position.ReadValue();
            return !IsOverUi(-1);
        }

        return false;
    }

    private bool IsOverUi(int pointerId)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        if (pointerId < 0)
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
        return EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    private Camera GetCamera()
    {
        return arCamera != null ? arCamera : Camera.main;
    }

    private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }
}