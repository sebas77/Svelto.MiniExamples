using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    public class CameraImplementor : MonoBehaviour, ITransformComponent
    {
        Transform cameraTransform;

        void Awake()
        {
            cameraTransform = transform;
        }

        public Vector3 position
        {
            get { return cameraTransform.position; }
            set { cameraTransform.position = value; }
        }

        public Quaternion rotation
        {
            set { cameraTransform.rotation = value; }
        }
    }
}