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

    [Header("Spawn")]
    [Tooltip("If true, the player GameObject will be moved to the tile marked `isStart` on scene start.")]
    public bool spawnAtStartTile = true;

    [Header("Field Bounds")]
    [Tooltip("Maximum axial distance from center allowed (inclusive). Example: 1 -> center + its 6 neighbors.")]
    public int fieldRadius = 3;
    [Tooltip("Axial coordinates of field center (default 0,0)")]
    public int fieldCenterQ = 0;
    public int fieldCenterR = 0;

    [Header("Movement Timing")]
    public float animationCycleDuration = 0.25f; // seconds per cycle (used if moveDuration == 0)
    public int cyclesPerMovement = 2; // movement lasts animationCycleDuration * cyclesPerMovement
    [Tooltip("Explicit movement duration in seconds. If > 0 it overrides animationCycleDuration * cyclesPerMovement.")]
    public float moveDuration = 0f;

    [Header("Camera")]
    public float cameraMoveDuration = 0.3f;
    public float cameraZoom = 5f;
    [Tooltip("Seconds to wait on reaching a finish tile before returning to the menu (for win animation)")]
    public float winDelay = 2f;
    [Tooltip("Seconds to wait after trap activation before forcing the player back")]
    public float trapDelay = 2f;

    [Header("Footprints")]
    [Tooltip("Prefab for footprint. Should have a SpriteRenderer. A footprint will be instantiated midway between tiles and faded in during movement.")]
    public GameObject footprintPrefab;
    [Tooltip("Optional angle offset (degrees) applied to instantiated footprint so it matches sprite artwork orientation.")]
    public float footprintRotationOffset = 0f;
    [Tooltip("Maximum random jitter (world units) applied to footprint position on both X and Y axes.")]
    public float footprintJitter = 0.05f;

    [Header("Animation")]
    public Animator animator; // expects int 'direction' and bool 'isWalking'

    [Header("UI")]
    public Text stepCounterText; // optional
    public TextMeshProUGUI stepCounterTMP; // optional

    [Header("Cosmetic Drift")]
    public float driftRadius = 0.25f; // how far penguin can drift inside the tile
    public float driftSpeed = 5f;

    public int stepCount { get; private set; } = 0;

    private bool isMoving = false;

    void Start()
    {
        // Optionally move the player to the tile marked as start in the scene before positioning.
        if (spawnAtStartTile)
        {
            var tiles = FindObjectsOfType<HexTile>();
            foreach (var t in tiles)
            {
                if (t != null && t.isStart)
                {
                    q = t.q;
                    r = t.r;
                    break;
                }
            }
        }

        Vector2 world = HexGridUtility.AxialToWorld(q, r, hexRadius);
        transform.position = new Vector3(world.x, world.y, transform.position.z);
        UpdateStepUI();
    }

    void Update()
    {
        if (!isMoving)
        {
            PreviewHoverDirection();
            HandleMouseMoveAttempt();
            CosmeticDrift();
        }
    }

    private void CosmeticDrift()
    {
        if (Camera.main == null) return;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 center = HexGridUtility.AxialToWorld(q, r, hexRadius);
        Vector2 dir = new Vector2(mouseWorld.x - center.x, mouseWorld.y - center.y);
        if (dir.sqrMagnitude > 0.0001f)
        {
            dir = dir.normalized * Mathf.Min(driftRadius, dir.magnitude);
        }
        Vector3 target = new Vector3(center.x + dir.x, center.y + dir.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * driftSpeed);
    }

    private void HandleMouseMoveAttempt()
    {
        if (Camera.main == null) return;
        if (Input.GetMouseButtonDown(0)) // LMB
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 axialF = HexGridUtility.WorldToAxial(new Vector2(mouseWorld.x, mouseWorld.y), hexRadius);
            int tq, tr;
            HexGridUtility.AxialRound(axialF.x, axialF.y, out tq, out tr);

            if (IsNeighbor(tq, tr))
            {
                if (!IsWithinField(tq, tr))
                {
                    // outside field
                    return;
                }

                // set animator direction immediately and force the walk transition
                int dq = tq - q;
                int dr = tr - r;
                if (animator != null)
                {
                    // toggle off then on to force Animator to retrigger transitions even if already true
                    animator.SetBool("isWalking", false);
                    SetDirectionFromDelta(dq, dr);
                    animator.SetBool("isWalking", true);
                }

                StartCoroutine(MoveToTile(tq, tr));
            }
        }
    }

    private void SetDirectionFromDelta(int dq, int dr)
    {
        if (animator == null) return;
        int dir = -1;
        if (dq == 0 && dr == 1) dir = 0;        // Up
        else if (dq == 1 && dr == 0) dir = 1;   // Right Up
        else if (dq == 1 && dr == -1) dir = 2;  // Right Down
        else if (dq == 0 && dr == -1) dir = 3;  // Down
        else if (dq == -1 && dr == 0) dir = 4;  // Left Down
        else if (dq == -1 && dr == 1) dir = 5;  // Left Up

        if (dir >= 0) animator.SetInteger("direction", dir);
    }

    private bool IsNeighbor(int tq, int tr)
    {
        int x1 = q; int z1 = r; int y1 = -x1 - z1;
        int x2 = tq; int z2 = tr; int y2 = -x2 - z2;
        int dx = Mathf.Abs(x1 - x2);
        int dy = Mathf.Abs(y1 - y2);
        int dz = Mathf.Abs(z1 - z2);
        int dist = (dx + dy + dz) / 2;
        return dist == 1;
    }

    private bool IsWithinField(int tq, int tr)
    {
        int x1 = fieldCenterQ; int z1 = fieldCenterR; int y1 = -x1 - z1;
        int x2 = tq; int z2 = tr; int y2 = -x2 - z2;
        int dx = Mathf.Abs(x1 - x2);
        int dy = Mathf.Abs(y1 - y2);
        int dz = Mathf.Abs(z1 - z2);
        int dist = (dx + dy + dz) / 2;
        return dist <= fieldRadius;
    }

    private IEnumerator MoveToTile(int tq, int tr, bool isForced = false)
    {
        // remember where we started so we can force-walk back if this is a trap
        int fromQ = q; int fromR = r;

        isMoving = true;
        // animator.isWalking is set before starting this coroutine so animation runs during movement

        // use current transform position as start so we don't 'snap' from a drift offset
        Vector2 start = new Vector2(transform.position.x, transform.position.y);
        Vector2 end = HexGridUtility.AxialToWorld(tq, tr, hexRadius);
        float duration;
        if (moveDuration > 0f)
            duration = moveDuration;
        else
            duration = Mathf.Max(0.001f, animationCycleDuration * Mathf.Max(1, cyclesPerMovement));
        float t = 0f;

        // Instantiate a footprint at the midpoint and start it fully transparent; it will fade in over the movement.
        GameObject footprintInstance = null;
        SpriteRenderer footprintSR = null;
        Color originalFootprintColor = Color.white;
        float originalFootprintAlpha = 1f;
        if (footprintPrefab != null)
        {
            Vector2 mid = (start + end) * 0.5f;
            // apply small random jitter so footprints look natural
            float jx = Random.Range(-footprintJitter, footprintJitter);
            float jy = Random.Range(-footprintJitter, footprintJitter);
            mid += new Vector2(jx, jy);
            Vector3 mid3 = new Vector3(mid.x, mid.y, transform.position.z);
            footprintInstance = Instantiate(footprintPrefab, mid3, Quaternion.identity);
            // rotate footprint so it's parallel to movement direction
            float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg + footprintRotationOffset;
            footprintInstance.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            footprintSR = footprintInstance.GetComponent<SpriteRenderer>();
            if (footprintSR != null)
            {
                originalFootprintColor = footprintSR.color;
                originalFootprintAlpha = originalFootprintColor.a;
                var c = originalFootprintColor;
                c.a = 0f;
                footprintSR.color = c;
            }
            else
            {
                Debug.LogWarning("Footprint prefab has no SpriteRenderer: " + footprintPrefab.name);
            }
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            Vector2 pos = Vector2.Lerp(start, end, p);
            transform.position = new Vector3(pos.x, pos.y, transform.position.z);

            // Fade footprint in to fully visible by end of movement
            if (footprintSR != null)
            {
                var c = originalFootprintColor;
                c.a = Mathf.Lerp(0f, originalFootprintAlpha, Mathf.Clamp01(p));
                footprintSR.color = c;
            }
            yield return null;
        }

        q = tq; r = tr;
        if (animator != null) animator.SetBool("isWalking", false);

        stepCount++;
        UpdateStepUI();

        var camCtrl = FindObjectOfType<CameraController>();
        if (camCtrl != null)
        {
            yield return StartCoroutine(camCtrl.CenterOnAxialCoroutine(q, r, hexRadius, cameraMoveDuration, cameraZoom));
        }

        // If we've landed on a finish tile, wait `winDelay` seconds then return to the menu (scene 0).
        var landedTile = HexTileRegistry.GetAt(q, r);
        if (landedTile != null && landedTile.isFinish)
        {
            yield return new WaitForSeconds(winDelay);
            SceneTransition.LoadSceneWithFade(0);
            // keep coroutine ending here; scene will change
            yield break;
        }


        // If this tile is a trap (and this movement isn't already a forced return), activate trap visual,
        // disable input, wait `trapDelay`, then force-walk back.
        if (landedTile != null && landedTile.isTrap && !isForced)
        {
            landedTile.SetTrapVisual(true);

            // keep input disabled while trap sequence runs
            isMoving = true;

            // wait so player can see trap (e.g., play ouch animation)
            yield return new WaitForSeconds(trapDelay);

            // forcefully walk the penguin back to where it came from; this counts as another step
            yield return StartCoroutine(MoveToTile(fromQ, fromR, true));

            // hide trap visual now that we've returned
            landedTile.SetTrapVisual(false);

            // after forced walk completes, re-enable input
            isMoving = false;

            yield break;
        }

        isMoving = false;
    }

    private void UpdateStepUI()
    {
        if (stepCounterText != null) stepCounterText.text = stepCount.ToString();
        if (stepCounterTMP != null) stepCounterTMP.text = stepCount.ToString();
    }

    public void SetAxialPosition(int nq, int nr)
    {
        q = nq; r = nr;
        Vector2 world = HexGridUtility.AxialToWorld(q, r, hexRadius);
        transform.position = new Vector3(world.x, world.y, transform.position.z);
    }

    private void PreviewHoverDirection()
    {
        if (Camera.main == null || animator == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 axialF = HexGridUtility.WorldToAxial(new Vector2(mouseWorld.x, mouseWorld.y), hexRadius);

        int tq, tr;
        HexGridUtility.AxialRound(axialF.x, axialF.y, out tq, out tr);
        bool isHoveringCurrentTile = (tq == q && tr == r);
        if (IsNeighbor(tq, tr) && IsWithinField(tq, tr) && !isHoveringCurrentTile)
        {
            int dq = tq - q;
            int dr = tr - r;
            SetDirectionFromDelta(dq, dr);
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }
}
