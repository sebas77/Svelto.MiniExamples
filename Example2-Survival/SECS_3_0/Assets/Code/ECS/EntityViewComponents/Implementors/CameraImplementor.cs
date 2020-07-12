using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    public class CameraImplementor : MonoBehaviour, ITransformComponent
    {
        Transform cameraTransform;

        public Vector3 position { get => cameraTransform.position; set => cameraTransform.position = value; }

        public Quaternion rotation { set => cameraTransform.rotation = value; }

        void Awake() { cameraTransform = transform; }
    }
}