using UnityEngine;

public static class HexGridUtility
{
    // Flat-top hex utilities. "size" is the hex radius.

    // Convert axial coordinates (q, r) to world position (x, y)
    public static Vector2 AxialToWorld(int q, int r, float size)
    {
        float x = size * (3f / 2f * q);
        float y = size * (Mathf.Sqrt(3f) / 2f * q + Mathf.Sqrt(3f) * r);
        return new Vector2(x, y);
    }

    // Convert world position (x, y) to fractional axial coordinates (q, r)
    public static Vector2 WorldToAxial(Vector2 pos, float size)
    {
        float q = (2f / 3f * pos.x) / size;
        float r = (-1f / 3f * pos.x + Mathf.Sqrt(3f) / 3f * pos.y) / size;
        return new Vector2(q, r);
    }

    // Round fractional axial coordinates to nearest integer axial coordinates
    public static void AxialRound(float qf, float rf, out int q, out int r)
    {
        // convert to cube
        float x = qf;
        float z = rf;
        float y = -x - z;

        float rx = Mathf.Round(x);
        float ry = Mathf.Round(y);
        float rz = Mathf.Round(z);

        float xDiff = Mathf.Abs(rx - x);
        float yDiff = Mathf.Abs(ry - y);
        float zDiff = Mathf.Abs(rz - z);

        if (xDiff > yDiff && xDiff > zDiff)
        {
            rx = -ry - rz;
        }
        else if (yDiff > zDiff)
        {
            ry = -rx - rz;
        }
        else
        {
            rz = -rx - ry;
        }

        q = (int)rx;
        r = (int)rz;
    }
}
