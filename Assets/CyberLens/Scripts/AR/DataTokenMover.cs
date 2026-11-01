using System.Collections;
using UnityEngine;

// Moves a data token between waypoints (Computer, Server, Database).
// The token and the waypoints must all be children of the same parent (CyberLab).
public class DataTokenMover : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private Transform tokenVisual;
    [SerializeField] private float moveSpeed = 1.2f;
    [SerializeField] private float pauseSeconds = 0.4f;
    [SerializeField] private float spinDegreesPerSecond = 90f;
    [SerializeField] private Material safeMaterial;
    [SerializeField] private Material unsafeMaterial;

    private Renderer gemRenderer;
    private Coroutine moveRoutine;
    private bool tokenVisible = true;

    private void Awake()
    {
        if (tokenVisual == null)
        {
            return;
        }

        foreach (Renderer partRenderer in tokenVisual.GetComponentsInChildren<Renderer>())
        {
            if (partRenderer.name == "Token_Gem")
            {
                gemRenderer = partRenderer;
            }
        }
    }

    private void Update()
    {
        if (tokenVisual != null)
        {
            tokenVisual.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
        }
    }

    // Loops the token from the first waypoint to the last (used by the Send data button).
    public void Play()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("DataTokenMover: add at least two waypoints.");
            return;
        }

        Stop();
        SetTokenVisible(true);
        moveRoutine = StartCoroutine(MoveLoop());
    }

    public void Stop()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }

    // Hides the token (used by lesson steps that have no token).
    public void HideToken()
    {
        Stop();
        SetTokenVisible(false);
    }

    // Moves the token to one waypoint: 0 = Computer, 1 = Server, 2 = Database.
    // instant = true jumps there at once. A hidden token also appears there at once.
    public void MoveToWaypoint(int index, bool instant)
    {
        if (waypoints == null || index < 0 || index >= waypoints.Length)
        {
            HideToken();
            return;
        }

        Stop();

        Vector3 target = waypoints[index].localPosition;
        bool wasHidden = !tokenVisible;
        SetTokenVisible(true);

        if (instant || wasHidden)
        {
            transform.localPosition = target;
            return;
        }

        moveRoutine = StartCoroutine(SlideTo(target));
    }

    // Cyan token = safe data.
    public void ShowSafe()
    {
        SetGemMaterial(safeMaterial);
    }

    // Magenta token = unsafe data.
    public void ShowUnsafe()
    {
        SetGemMaterial(unsafeMaterial);
    }

    private void SetTokenVisible(bool visible)
    {
        tokenVisible = visible;
        if (tokenVisual != null)
        {
            tokenVisual.gameObject.SetActive(visible);
        }
    }

    private void SetGemMaterial(Material material)
    {
        if (gemRenderer != null && material != null)
        {
            gemRenderer.sharedMaterial = material;
        }
    }

    private IEnumerator SlideTo(Vector3 target)
    {
        while ((transform.localPosition - target).sqrMagnitude > 0.0001f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        moveRoutine = null;
    }

    private IEnumerator MoveLoop()
    {
        transform.localPosition = waypoints[0].localPosition;
        int targetIndex = 0;

        while (true)
        {
            targetIndex = (targetIndex + 1) % waypoints.Length;
            Vector3 target = waypoints[targetIndex].localPosition;

            if (targetIndex == 0)
            {
                transform.localPosition = target;
                continue;
            }

            while ((transform.localPosition - target).sqrMagnitude > 0.0001f)
            {
                transform.localPosition = Vector3.MoveTowards(
                    transform.localPosition, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(pauseSeconds);
        }
    }
}