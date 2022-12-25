using UnityEngine;

namespace Svelto.ECS.Example.Survive.OOPLayer
{
    /// <summary>
    /// exploit this MB to get the reference of the entity from the gameobject. Used for example when using
    /// Unity standard raycast
    /// </summary>
    [DisallowMultipleComponent]
    public class EntityReferenceHolder : MonoBehaviour
    {
        public ulong reference;
    }
}
