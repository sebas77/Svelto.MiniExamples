using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabSerializer))]
public class PrefabSerializerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PrefabSerializer myScript = (PrefabSerializer)target;
        if (GUILayout.Button("Export Json file"))
        {
            myScript.SerializeData();
        }
    }
}
