using Svelto.ECS.Components;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Common
{
    public class TransformImplementer : MonoBehaviour, ITransform, IImplementor
    {
        public ECSVector3 position { get => _t.position.ToECSVector3(); set => _t.position = value.ToVector3(); }
        public ECSVector3 localPosition { get => _t.localPosition.ToECSVector3(); set => _t.localPosition = value.ToVector3(); }

        public ECSQuaternion rotation { get => _t.rotation.ToECSQuaternion(); set => _t.rotation = value.ToQuaternion(); }
        public ECSQuaternion localRotation { get => _t.localRotation.ToECSQuaternion(); set => _t.localRotation = value.ToQuaternion(); }

        public ECSVector3 eulerAngles { get => _t.eulerAngles.ToECSVector3(); set => _t.eulerAngles = value.ToVector3(); }
        public ECSVector3 localEulerAngles { get => _t.localEulerAngles.ToECSVector3(); set => _t.localEulerAngles = value.ToVector3(); }

        public ECSVector3 localScale { get => _t.localScale.ToECSVector3(); set => _t.localScale = value.ToVector3(); }
        public ECSVector3 lossyScale { get => _t.lossyScale.ToECSVector3(); }

        public ECSVector3 forward { get => _t.forward.ToECSVector3(); }
        public ECSVector3 right { get => _t.right.ToECSVector3(); }
        public ECSVector3 up { get => _t.up.ToECSVector3(); }

        Transform _t;

        void Awake()
        {
            _t = this.transform;
        }
    }
}
