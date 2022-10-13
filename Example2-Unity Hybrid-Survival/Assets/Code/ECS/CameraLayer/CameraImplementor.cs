using UnityEngine;

namespace Svelto.ECS.Example.Survive.Camera
{
    public class CameraImplementor
    {
        public Vector3    position { get => _cameraTransform.position; set => _cameraTransform.position = value; }
        public Quaternion rotation { set => _cameraTransform.rotation = value; }

        public Ray camRay => _camRay;

        public Vector3 camRayInput { set => _camRay = _camera.ScreenPointToRay(value); }
        public Vector3 offset      { get; set; }

        Transform          _cameraTransform;
        UnityEngine.Camera _camera;
        Ray                _camRay;
    }
}