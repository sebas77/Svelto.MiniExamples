using System.IO;
using Svelto.ECS.Example.Survive;
using UnityEngine;

[ExecuteInEditMode]
public class SpawningData : MonoBehaviour
{
    static bool serializedSpawnDataOnce;
    static bool serializedAttackDataOnce;

    void Awake()
    {
        Init();       
    }
    
    public void SerializeSpawnData()
    {
        serializedSpawnDataOnce = true;
        
        var data = GetComponents<EnemyData>();
        JSonEnemySpawnData[] spawningdata = new JSonEnemySpawnData[data.Length];

        for (int i = 0; i < data.Length; i++)
            spawningdata[i] = new JSonEnemySpawnData(data[i].spawnData);

        var json = JsonHelper.arrayToJson(spawningdata);

        Svelto.Console.Log(json);

        File.WriteAllText("EnemySpawningData.json", json);
    }
    
    public void SerializeAttackData()
    {
        var data = GetComponents<EnemyData>();
        JSonEnemyAttackData[] attackData = new JSonEnemyAttackData[data.Length];
        
        serializedAttackDataOnce = true;

        for (int i = 0; i < data.Length; i++)
            attackData[i] = new JSonEnemyAttackData(data[i].attackData);

        var json = JsonHelper.arrayToJson(attackData);

        Svelto.Console.Log(json);

        File.WriteAllText("EnemyAttackData.json", json);
    }

    public void Init()
    {
        if (serializedSpawnDataOnce == false)
            SerializeSpawnData();

        if (serializedAttackDataOnce == false)
            SerializeAttackData();
    }
}