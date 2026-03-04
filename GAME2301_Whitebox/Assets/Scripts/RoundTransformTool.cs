using UnityEngine;
using UnityEditor;

public class RoundTransformTool
{
    [MenuItem("Tools/Round Transforms")]
    public static void RoundSelectedTransforms()
    {
        // Get all selected transforms
        Transform[] transforms = Selection.transforms;

        if (transforms.Length == 0)
        {
            Debug.LogWarning("No transforms selected.");
            return;
        }

        // Register undo operation so you can Ctrl+Z if needed
        Undo.RecordObjects(transforms, "Round Transforms");

        foreach (Transform t in transforms)
        {
            t.localPosition = Round(t.localPosition);
            t.localEulerAngles = Round(t.localEulerAngles);
        }
    }

    private static Vector3 Round(Vector3 v)
    {
        return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
    }
}