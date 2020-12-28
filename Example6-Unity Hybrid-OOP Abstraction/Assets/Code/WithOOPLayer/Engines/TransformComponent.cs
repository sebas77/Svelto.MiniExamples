using Vector3 = UnityEngine.Vector3;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    struct TransformComponent : IEntityComponent
    {
        public Vector3 position;

        public TransformComponent(Vector3 vector3) { position = vector3; }
    }
}