using Svelto.ECS;
using Unity.Mathematics;

namespace Svelto.ECS.MiniExamples.Example1
{
    static class GAME_GROUPS
    {
        public static readonly ExclusiveGroup BOIDS_GROUP = new ExclusiveGroup();
    }
    
    public class BoidEntityDecriptor : GenericEntityDescriptor<BoidEntityStruct>
    {
    }

    public struct BoidEntityStruct : IEntityStruct
    {
        public SVector3 position;
        public SVector4 rotation;
        public SVector3 velocity;
        public SVector3 acceleration;

        public EGID ID { get; set; }
    }

    public struct SVector3
    {
        public float x, y, z;
    }

    public static class ExtensionMethods
    {
        public static float3 ToFloat3(this SVector3 vector)
        {
            return new float3(vector.x, vector.y, vector.z);
        }
    }
    
    public struct SVector4
    {
        public float x, y, z, w;
    }
}