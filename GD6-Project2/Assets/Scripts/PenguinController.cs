using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(SpriteRenderer))]
public class PenguinController : MonoBehaviour
{
    [Header("Hex Position")]
    public int q = 0;
    public int r = 0;
    public float hexRadius = 1f;

    [Header("Movement Timing")]
    public float animationCycleDuration = 0.25f; // seconds per cycle
    public int cyclesPerMovement = 2; // movement lasts animationCycleDuration * cyclesPerMovement

    [Header("Animation")]
    public Animator animator; // expects bool "isWalking" and trigger "ouch" optionally

    [Header("UI")]
    public Text stepCounterText; // optional
    public TextMeshProUGUI stepCounterTMP; // optional

    [Header("Cosmetic Drift")]
    public float driftRadius = 0.25f; // how far penguin can drift inside the tile
    public float driftSpeed = 5f;

    public int stepCount { get; private set; } = 0;

    private bool isMoving = false;

    private Vector3 idleOriginLocal = Vector3.zero;

    void Start()
    {
        // position to initial axial coordinates
        Vector2 world = HexGridUtility.AxialToWorld(q, r, hexRadius);
        transform.position = new Vector3(world.x, world.y, transform.position.z);
        UpdateStepUI();
        idleOriginLocal = Vector3.zero;
    }

    void Update()
    {
        if (!isMoving)
        {
            HandleMouseMoveAttempt();
            CosmeticDrift();
        }
    }

    private void CosmeticDrift()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 center = HexGridUtility.AxialToWorld(q, r, hexRadius);
        Vector2 dir = new Vector2(mouseWorld.x - center.x, mouseWorld.y - center.y);
        if (dir.sqrMagnitude > 0.0001f)
        {
            dir = dir.normalized * Mathf.Min(driftRadius, dir.magnitude);
        }
        Vector3 targetLocal = new Vector3(dir.x, dir.y, 0f);
        transform.position = Vector3.Lerp(transform.position, new Vector3(center.x, center.y, transform.position.z) + targetLocal, Time.deltaTime * driftSpeed);
    }

    private void HandleMouseMoveAttempt()
    {
        if (Input.GetMouseButtonDown(0)) // LMB
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 axialF = HexGridUtility.WorldToAxial(new Vector2(mouseWorld.x, mouseWorld.y), hexRadius);
            int tq, tr;
            HexGridUtility.AxialRound(axialF.x, axialF.y, out tq, out tr);

            if (IsNeighbor(tq, tr))
            {
                StartCoroutine(MoveToTile(tq, tr));
            }
            else
            {
                // not a neighbor; ignore
            }
        }
    }

    private bool IsNeighbor(int tq, int tr)
    {
        // axial distance via cube coords
        int x1 = q;
        int z1 = r;
        int y1 = -x1 - z1;

        int x2 = tq;
        int z2 = tr;
        int y2 = -x2 - z2;

        int dx = Mathf.Abs(x1 - x2);
        int dy = Mathf.Abs(y1 - y2);
        int dz = Mathf.Abs(z1 - z2);

        int dist = (dx + dy + dz) / 2;
        return dist == 1;
    }

    private IEnumerator MoveToTile(int tq, int tr)
    {
        isMoving = true;
        if (animator != null) animator.SetBool("isWalking", true);

        Vector2 start = HexGridUtility.AxialToWorld(q, r, hexRadius);
        Vector2 end = HexGridUtility.AxialToWorld(tq, tr, hexRadius);
        float duration = Mathf.Max(0.001f, animationCycleDuration * Mathf.Max(1, cyclesPerMovement));
        float t = 0f;

        // face direction (flip sprite if needed)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = (end.x < start.x);
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            Vector2 pos = Vector2.Lerp(start, end, p);
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);
            yield return null;
        }

        // finish
        q = tq;
        r = tr;
        if (animator != null) animator.SetBool("isWalking", false);

        stepCount++;
        UpdateStepUI();

        isMoving = false;
    }

    private void UpdateStepUI()
    {
        if (stepCounterText != null)
        {
            stepCounterText.text = stepCount.ToString();
        }
        if (stepCounterTMP != null)
        {
            stepCounterTMP.text = stepCount.ToString();
        }
    }

    // Public helper to set penguin to a specific axial coordinate instantly
    public void SetAxialPosition(int nq, int nr)
    {
        q = nq;
        r = nr;
        Vector2 world = HexGridUtility.AxialToWorld(q, r, hexRadius);
        transform.position = new Vector3(world.x, world.y, transform.position.z);
    }
}
