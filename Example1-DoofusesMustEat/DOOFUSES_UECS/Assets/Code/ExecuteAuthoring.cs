using UnityEngine;
using Unity.Entities;

/* To ensure that the Prefabs namespace systems only run in the Prefabs scene,
 the Prefabs systems require the existence of an Execute singleton to update. */
public class ExecuteAuthoring: MonoBehaviour
{
    public class Baker: Baker<ExecuteAuthoring>
    {
        public override void Bake(ExecuteAuthoring authoring)
        {
            AddComponent<Execute>();
        }
    }
}

public struct Execute: IComponentData { }