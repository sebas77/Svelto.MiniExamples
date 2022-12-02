#if UNITY_5 || UNITY_5_3_OR_NEWER
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    // "New Svelto GUI Patterns are now available"
    public static class SveltoGUIHelper
    {
        public static EntityInitializer Create<T>(EGID ID, Transform contextHolder, IEntityFactory factory, bool searchImplementorsInChildren = false)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            T holder;
            holder = contextHolder.GetComponentInChildren<T>(true);

            var implementors = searchImplementorsInChildren == false
                ? holder.GetComponents<IImplementor>()
                : holder.GetComponentsInChildren<IImplementor>(true);

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }
    }
}
#endif