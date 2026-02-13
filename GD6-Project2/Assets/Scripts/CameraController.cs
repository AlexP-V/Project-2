using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Camera cam;

    void Awake()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    // Centers camera on axial coordinate (flat-top) and optionally adjusts orthographic size to targetZoom.
    public IEnumerator CenterOnAxialCoroutine(int q, int r, float hexRadius, float duration, float targetZoom)
    {
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
