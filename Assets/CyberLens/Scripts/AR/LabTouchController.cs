using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

// Put this on the CyberLab root object.
// One finger drag = rotate. Two finger pinch = zoom. Quick tap = select an object.
// In the Unity editor you can use the mouse: drag, scroll wheel, click.
public class LabTouchController : MonoBehaviour
{
    [SerializeField] private float rotateDegreesPerPixel = 0.4f;
    [SerializeField] private float minZoom = 0.5f;
    [SerializeField] private float maxZoom = 2.5f;
    [SerializeField] private float tapMaxMovePixels = 30f;
    [SerializeField] private float tapMaxSeconds = 0.4f;

    private Vector3 baseScale;
    private float zoomFactor = 1f;

    // Touch state
    private bool gestureActive;
    private bool gestureStartedOverUi;
    private bool gestureWasMultiTouch;
    private float lastPinchDistance;

    // Mouse state (for testing in the editor)
    private bool mouseDragging;
    private Vector2 mouseStartPosition;
    private float mouseStartTime;
    private float mouseTotalMove;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    // Call this after changing the lab's scale from code (used by AR placement in Stage 6B).
    public void ResetZoomBase()
    {
        baseScale = transform.localScale;
        zoomFactor = 1f;
    }

    private void Update()
    {
        int touchCount = Touch.activeTouches.Count;

        if (touchCount > 0)
        {
            HandleTouch(touchCount);
        }
        else
        {
            gestureActive = false;
            gestureWasMultiTouch = false;
            lastPinchDistance = 0f;
            HandleMouse();
        }
    }

    private void HandleTouch(int touchCount)
    {
        Touch firstTouch = Touch.activeTouches[0];

        if (!gestureActive)
        {
            gestureActive = true;
            gestureWasMultiTouch = false;
            gestureStartedOverUi = IsPointerOverUi(firstTouch.touchId);
        }

        if (gestureStartedOverUi)
        {
            return;
        }

        if (touchCount >= 2)
        {
            gestureWasMultiTouch = true;
            float distance = Vector2.Distance(
                Touch.activeTouches[0].screenPosition,
                Touch.activeTouches[1].screenPosition);

            if (lastPinchDistance > 0f)
            {
                ZoomLab(distance / lastPinchDistance);
            }
            lastPinchDistance = distance;
            return;
        }

        lastPinchDistance = 0f;

        if (firstTouch.phase == TouchPhase.Moved)
        {
            RotateLab(firstTouch.delta.x);
        }

        if (firstTouch.phase == TouchPhase.Ended && !gestureWasMultiTouch)
        {
            float moved = Vector2.Distance(firstTouch.screenPosition, firstTouch.startScreenPosition);
            double duration = firstTouch.time - firstTouch.startTime;

            if (moved < tapMaxMovePixels && duration < tapMaxSeconds)
            {
                TrySelect(firstTouch.screenPosition);
            }
        }
    }

    private void HandleMouse()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            mouseStartPosition = mouse.position.ReadValue();
            mouseStartTime = Time.unscaledTime;
            mouseTotalMove = 0f;
            mouseDragging = !IsMouseOverUi();
        }

        if (mouseDragging && mouse.leftButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue();
            mouseTotalMove += delta.magnitude;
            RotateLab(delta.x);
        }

        if (mouseDragging && mouse.leftButton.wasReleasedThisFrame)
        {
            bool quick = Time.unscaledTime - mouseStartTime < tapMaxSeconds;
            if (mouseTotalMove < tapMaxMovePixels && quick)
            {
                TrySelect(mouse.position.ReadValue());
            }
            mouseDragging = false;
        }

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f && !IsMouseOverUi())
        {
            ZoomLab(1f + scroll * 0.0005f);
        }
    }

    private void RotateLab(float pixelsMovedRight)
    {
        transform.Rotate(0f, -pixelsMovedRight * rotateDegreesPerPixel, 0f, Space.World);
    }

    private void ZoomLab(float factor)
    {
        zoomFactor = Mathf.Clamp(zoomFactor * factor, minZoom, maxZoom);
        transform.localScale = baseScale * zoomFactor;
    }

    private void TrySelect(Vector2 screenPosition)
    {
        Camera viewCamera = Camera.main;
        if (viewCamera == null)
        {
            Debug.LogWarning("LabTouchController: no camera tagged MainCamera.");
            return;
        }

        Ray ray = viewCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 100f))
        {
            return;
        }

        TapTarget target = hit.collider.GetComponentInParent<TapTarget>();
        if (target != null)
        {
            target.NotifyTapped();
        }
    }

    private bool IsPointerOverUi(int pointerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    private bool IsMouseOverUi()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}