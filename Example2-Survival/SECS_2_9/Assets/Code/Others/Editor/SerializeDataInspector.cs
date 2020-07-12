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
    
    [MenuItem("Assets/SetWebGLMemory")]
    static void SetWebGLMemory()
    {
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.memorySize = 512; // tweak this value for your project
    }
}


