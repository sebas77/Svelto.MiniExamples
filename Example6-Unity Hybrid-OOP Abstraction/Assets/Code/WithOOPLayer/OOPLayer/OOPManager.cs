using Svelto.DataStructures;
using UnityEngine;

namespace Svelto.ECS.Example.OOPAbstraction.OOPLayer
{
    ///The class that wrap the OOP library must be known by the package only as much as possible. 
    ///what cannot be use through the package, is exposed through a public interface. Note though that this may
    ///be considered a work around to better design. In this case, no methods are exposed.  
    class OOPManager
    {
        internal uint RegisterEntity(PrimitiveType type)
        {
            var cubeObject = GameObject.CreatePrimitive(type);

            objects.Add(cubeObject.transform);

            return (uint) (objects.count - 1);
        }

        internal void SetParent(uint index, in uint parent)
        {
            objects[index].SetParent(objects[parent], false);
        }

        internal void SetPosition(uint index, in Vector3 position) { objects[index].localPosition = position; }

        readonly FasterList<Transform> objects = new FasterList<Transform>();
    }
}