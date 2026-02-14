using UnityEngine;

public class TileHoverManager : MonoBehaviour
{
    [Tooltip("Hex radius used to compute axial from mouse world position. If 0, will try to use first HexTile or PenguinController.")]
    public float hexRadius = 0f;

    private HexTile currentHighlight = null;
    private PenguinController cachedPenguin = null;

    void Update()
    {
        if (Camera.main == null) return;
        Vector3 mw = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseWorld = new Vector2(mw.x, mw.y);

        float size = hexRadius;
        if (size <= 0f)
        {
            // try to find any HexTile to get radius
            var any = FindObjectOfType<HexTile>();
            if (any != null) size = any.hexRadius;
            else
            {
                var p = FindObjectOfType<PenguinController>();
                if (p != null) size = p.hexRadius;
            }
        }
        if (size <= 0f) size = 1f; // fallback

        // Determine axial under mouse
        Vector2 axialF = HexGridUtility.WorldToAxial(mouseWorld, size);
        int mq, mr;
        HexGridUtility.AxialRound(axialF.x, axialF.y, out mq, out mr);

        // cache penguin reference
        if (cachedPenguin == null)
            cachedPenguin = FindObjectOfType<PenguinController>();

        HexTile tile = HexTileRegistry.GetAt(mq, mr);

        // only highlight if the tile exists and is a neighbor of the penguin
        bool allowHighlight = false;
        if (tile != null && cachedPenguin != null)
        {
            // do not highlight the tile the penguin currently stands on
            if (!(tile.q == cachedPenguin.q && tile.r == cachedPenguin.r))
            {
                // compute axial/cube distance between penguin and this tile
                int x1 = cachedPenguin.q;
                int z1 = cachedPenguin.r;
                int y1 = -x1 - z1;

                int x2 = tile.q;
                int z2 = tile.r;
                int y2 = -x2 - z2;

                int dx = Mathf.Abs(x1 - x2);
                int dy = Mathf.Abs(y1 - y2);
                int dz = Mathf.Abs(z1 - z2);

                int dist = (dx + dy + dz) / 2;
                if (dist == 1) allowHighlight = true;
            }
        }

        HexTile newHighlight = allowHighlight ? tile : null;
        if (newHighlight != currentHighlight)
        {
            if (currentHighlight != null) currentHighlight.SetHighlighted(false);
            currentHighlight = newHighlight;
            if (currentHighlight != null) currentHighlight.SetHighlighted(true);
        }
    }
}
