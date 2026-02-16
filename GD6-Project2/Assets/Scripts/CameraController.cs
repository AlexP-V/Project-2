using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Camera cam;
    // Camera starts zoomed out. This value is used at startup for initial zoom-out.
    public float zoomOutSize = 7f;
    // Tracks whether we've performed the initial zoom-in after the player's first click.
    [HideInInspector]
    public bool hasZoomedIn = false;

    void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        // Ensure camera starts zoomed OUT before player interaction.
        if (cam != null)
        {
            if (cam.orthographic)
                cam.orthographicSize = zoomOutSize;
            else
                cam.fieldOfView = zoomOutSize;
        }
    }

    // Centers camera on axial coordinate (flat-top) and optionally adjusts orthographic size to targetZoom.
    public IEnumerator CenterOnAxialCoroutine(int q, int r, float hexRadius, float duration, float targetZoom)
    {
        // Mark that we've zoomed in (or at least triggered a zoom); used to prevent
        // triggering the initial zoom multiple times.
        hasZoomedIn = true;

        Vector2 world = HexGridUtility.AxialToWorld(q, r, hexRadius);
        Vector3 startPos = cam.transform.position;
        Vector3 targetPos = new Vector3(world.x, world.y, startPos.z);

        float startZoom = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
        float elapsed = 0f;
        duration = Mathf.Max(0.001f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            if (cam.orthographic)
                cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, t);
            else
                cam.fieldOfView = Mathf.Lerp(startZoom, targetZoom, t);
            yield return null;
        }

        cam.transform.position = targetPos;
        if (cam.orthographic) cam.orthographicSize = targetZoom;
        else cam.fieldOfView = targetZoom;
    }
}
