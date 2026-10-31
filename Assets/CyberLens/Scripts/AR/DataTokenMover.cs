using System.Collections;
using UnityEngine;

// Moves a data token along a list of waypoints (Computer -> Server -> Database).
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

    // Starts moving the token from the first waypoint to the last, then repeats.
    public void Play()
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            Debug.LogWarning("DataTokenMover: add at least two waypoints.");
            return;
        }

        Stop();
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

    private void SetGemMaterial(Material material)
    {
        if (gemRenderer != null && material != null)
        {
            gemRenderer.sharedMaterial = material;
        }
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