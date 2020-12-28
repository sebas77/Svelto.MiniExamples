using Svelto.DataStructures;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    public class OOPManager
    {
        public uint RegisterSphere()
        {
            var sphereObject      = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            
            objects.Add(sphereObject.transform);

            return (uint) (objects.count - 1);
        }

        public uint RegisterCube()
        {
            var cubeObject      = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            objects.Add(cubeObject.transform);
            
            return (uint) (objects.count - 1);
        }

        public void SetPotion(uint index, in Vector3 position)
        {
            objects[index].localPosition = position;
        }

        public void SetParent(uint index, in uint parent)
        {
            objects[index].SetParent(objects[parent], false);
        }
        
        readonly FasterList<Transform> objects = new FasterList<Transform>();
    }
}