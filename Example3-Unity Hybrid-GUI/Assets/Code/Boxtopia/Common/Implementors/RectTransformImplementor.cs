using Svelto.ECS.Components;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Common
{
    public class RectTransformImplementor : MonoBehaviour, IRectTransform, IImplementor
    {
        public RectTransform _rectTransform;

        public ECSVector2 sizeDelta
        {
            get { return _rectTransform.sizeDelta.ToECSVector2(); }
            set { _rectTransform.sizeDelta = value.ToVector2(); }
        }
        
        public ECSVector3 position { get => _rectTransform.position.ToECSVector3(); set => _rectTransform.position = value.ToVector3(); }
        public ECSVector3 localPosition { get => _rectTransform.localPosition.ToECSVector3(); set => _rectTransform.localPosition = value.ToVector3(); }

        public ECSQuaternion rotation { get => _rectTransform.rotation.ToECSQuaternion(); set => _rectTransform.rotation = value.ToQuaternion(); }
        public ECSQuaternion localRotation { get => _rectTransform.localRotation.ToECSQuaternion(); set => _rectTransform.localRotation = value.ToQuaternion(); }

        public ECSVector3 eulerAngles { get => _rectTransform.eulerAngles.ToECSVector3(); set => _rectTransform.eulerAngles = value.ToVector3(); }
        public ECSVector3 localEulerAngles { get => _rectTransform.localEulerAngles.ToECSVector3(); set => _rectTransform.localEulerAngles = value.ToVector3(); }

        public ECSVector3 localScale { get => _rectTransform.localScale.ToECSVector3(); set => _rectTransform.localScale = value.ToVector3(); }
        public ECSVector3 lossyScale { get => _rectTransform.lossyScale.ToECSVector3(); }

        public ECSVector3 forward { get => _rectTransform.forward.ToECSVector3(); }
        public ECSVector3 right { get => _rectTransform.right.ToECSVector3(); }
        public ECSVector3 up { get => _rectTransform.up.ToECSVector3(); }

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
    }
}
