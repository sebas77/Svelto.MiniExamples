using UnityEngine;

static class UnityUtilities
{
    public static bool MouseToPosition(out Vector3 position)
    {
        var        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            position = hit.point;
            return true;
        }

        position = default;
        return false;
    }
}