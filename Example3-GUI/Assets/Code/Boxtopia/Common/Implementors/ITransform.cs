
using Svelto.ECS.Components;

namespace Common
{
    public interface ITransform
    {
        ECSVector3 position { get; set; }
        ECSVector3 localPosition { get; set; }

        ECSQuaternion rotation { get; set; }
        ECSQuaternion localRotation { get; set; }

        ECSVector3 eulerAngles { get; set; }
        ECSVector3 localEulerAngles { get; set; }

        ECSVector3 localScale { get; set; }
        ECSVector3 lossyScale { get; }

        ECSVector3 forward { get; }
        ECSVector3 right { get; }
        ECSVector3 up { get; }
    }
}