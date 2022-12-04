using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    [DisallowMultipleComponent]
    public class EntityReferenceHolder : MonoBehaviour
    {
        public EntityReference reference { get; set; }
    }
}
