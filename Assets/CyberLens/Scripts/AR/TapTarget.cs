using System;
using System.Collections;
using UnityEngine;

// Put this on any model the user can tap.
// It stores the text shown when tapped, and builds a box collider that fits the model.
public class TapTarget : MonoBehaviour
{
    // Other scripts listen to this event to know which object was tapped.
    public static event Action<TapTarget> Tapped;

    [SerializeField] private string displayName = "Object";
    [SerializeField, TextArea(2, 6)] private string explanation = "Write the explanation here.";

    private Vector3 originalScale;
    private Coroutine pulseRoutine;

    public string DisplayName
    {
        get { return displayName; }
    }

    public string Explanation
    {
        get { return explanation; }
    }

    private void Awake()
    {
        originalScale = transform.localScale;

        if (GetComponent<Collider>() == null)
        {
            BuildBoxCollider();
        }
    }

    // Called by LabTouchController when this object is tapped.
    public void NotifyTapped()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            transform.localScale = originalScale;
        }

        pulseRoutine = StartCoroutine(Pulse());

        if (Tapped != null)
        {
            Tapped(this);
        }
    }

    private IEnumerator Pulse()
    {
        Vector3 bigScale = originalScale * 1.12f;
        float halfDuration = 0.12f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(originalScale, bigScale, timer / halfDuration);
            yield return null;
        }

        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(bigScale, originalScale, timer / halfDuration);
            yield return null;
        }

        transform.localScale = originalScale;
        pulseRoutine = null;
    }

    // Makes one box collider that covers all the mesh parts of this model.
    private void BuildBoxCollider()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        bool hasBounds = false;
        Bounds combined = new Bounds();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null)
            {
                continue;
            }

            Vector3 min = meshFilter.sharedMesh.bounds.min;
            Vector3 max = meshFilter.sharedMesh.bounds.max;

            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new Vector3(
                    (i & 1) == 0 ? min.x : max.x,
                    (i & 2) == 0 ? min.y : max.y,
                    (i & 4) == 0 ? min.z : max.z);

                Vector3 localPoint = transform.InverseTransformPoint(meshFilter.transform.TransformPoint(corner));

                if (!hasBounds)
                {
                    combined = new Bounds(localPoint, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    combined.Encapsulate(localPoint);
                }
            }
        }

        if (!hasBounds)
        {
            Debug.LogWarning("TapTarget: no meshes found on " + name);
            return;
        }

        BoxCollider box = gameObject.AddComponent<BoxCollider>();
        box.center = combined.center;
        box.size = combined.size;
    }
}
