using UnityEngine;
using UnityEditor;
using System.Linq;

public class AlignTool
{
    [MenuItem("Tools/Align/Align X+")]
    private static void AlignXPositive() => Align(0, 1);

    [MenuItem("Tools/Align/Align X-")]
    private static void AlignXNegative() => Align(0, -1);

    [MenuItem("Tools/Align/Align Y+")]
    private static void AlignYPositive() => Align(1, 1);

    [MenuItem("Tools/Align/Align Y-")]
    private static void AlignYNegative() => Align(1, -1);

    [MenuItem("Tools/Align/Align Z+")]
    private static void AlignZPositive() => Align(2, 1);

    [MenuItem("Tools/Align/Align Z-")]
    private static void AlignZNegative() => Align(2, -1);

    [MenuItem("Tools/Align/Align Same X")]
    private static void AlignSameX() => AlignSame(0);

    [MenuItem("Tools/Align/Align Same Y")]
    private static void AlignSameY() => AlignSame(1);

    [MenuItem("Tools/Align/Align Same Z")]
    private static void AlignSameZ() => AlignSame(2);

    /// <summary>
    /// Aligns the selected objects in a chain based on their renderer bounds.
    /// </summary>
    /// <param name="axis">The axis to align on (0=X, 1=Y, 2=Z).</param>
    /// <param name="direction">The direction to align in (1 for positive, -1 for negative).</param>
    private static void Align(int axis, int direction)
    {
        // Get GameObjects from selection, preserving selection order.
        var gameObjects = Selection.objects.OfType<GameObject>().ToArray();

        if (gameObjects.Length < 2)
        {
            Debug.LogWarning("Select at least two objects to align.");
            return;
        }

        // Record all transforms for a single undo operation.
        Undo.RecordObjects(gameObjects.Select(g => g.transform).ToArray(), "Align Objects");

        var source = gameObjects[0];

        // Chain the alignment for all subsequent objects.
        for (int i = 1; i < gameObjects.Length; i++)
        {
            var target = gameObjects[i];

            if (!GetTotalBounds(source, out Bounds sourceBounds) || !GetTotalBounds(target, out Bounds targetBounds))
            {
                Debug.LogWarning($"Skipping alignment for '{target.name}' because either it or the previous object ('{source.name}') has no Renderer components in its hierarchy.");
                source = target; // Continue the chain from the current object.
                continue;
            }

            Vector3 newTargetPosition = target.transform.position;

            if (direction > 0)
            {
                // Align target's min edge to source's max edge.
                float offset = target.transform.position[axis] - targetBounds.min[axis];
                newTargetPosition[axis] = sourceBounds.max[axis] + offset;
            }
            else
            {
                // Align target's max edge to source's min edge.
                float offset = targetBounds.max[axis] - target.transform.position[axis];
                newTargetPosition[axis] = sourceBounds.min[axis] - offset;
            }

            target.transform.position = newTargetPosition;

            // The current target becomes the source for the next object in the chain.
            source = target;
        }
    }

    private static void AlignSame(int axis)
    {
        // Get GameObjects from selection, preserving selection order.
        var gameObjects = Selection.objects.OfType<GameObject>().ToArray();

        if (gameObjects.Length < 2)
        {
            Debug.LogWarning("Select at least two objects to align.");
            return;
        }

        Undo.RecordObjects(gameObjects.Select(g => g.transform).ToArray(), "Align Same Axis");

        var source = gameObjects[0];
        float alignValue = source.transform.position[axis];

        for (int i = 1; i < gameObjects.Length; i++)
        {
            Vector3 newPosition = gameObjects[i].transform.position;
            newPosition[axis] = alignValue;
            gameObjects[i].transform.position = newPosition;
        }
    }

    /// <summary>
    /// Calculates the combined bounding box of all Renderers in a GameObject's hierarchy.
    /// </summary>
    private static bool GetTotalBounds(GameObject go, out Bounds totalBounds)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            totalBounds = new Bounds();
            return false;
        }

        totalBounds = renderers[0].bounds;
        foreach (var renderer in renderers.Skip(1))
        {
            totalBounds.Encapsulate(renderer.bounds);
        }
        return true;
    }
}