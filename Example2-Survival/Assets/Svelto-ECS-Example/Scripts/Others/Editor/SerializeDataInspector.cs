using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawningData))]
public class SerializeDataInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpawningData myScript = (SpawningData)target;
        if (GUILayout.Button("Export Json file"))
        {
            myScript.SerializeSpawnData();
            myScript.SerializeAttackData();
        }
    }
}
