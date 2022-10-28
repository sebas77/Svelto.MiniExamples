#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    // "New Svelto GUI Patterns are now available"
    public static class SveltoGUIHelper
    {
        public static EntityInitializer Create<T>(EGID ID, Transform contextHolder, IEntityFactory factory, out T holder
            , bool searchImplementorsInChildren = false, IECSManager manager = null)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            holder = contextHolder.GetComponentInChildren<T>(true);
            if (holder == null)
            {
                throw new Exception($"Could not find holder {typeof(T).Name} in {contextHolder.name}");
            }

            var implementors = searchImplementorsInChildren == false
                ? holder.GetComponents<IImplementor>()
                : holder.GetComponentsInChildren<IImplementor>(true);

            if (manager != null)
            {
                foreach (var implementor in implementors)
                {
                    if (implementor is IUseResourceManagerImplementor castedImplementor)
                    {
                        castedImplementor.resourceManager = manager;                       
                    }
                }
            }

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }

        public static EntityInitializer Create<T>
            (EGID ID, Transform contextHolder, IEntityFactory factory, bool searchImplementorsInChildren = false
            , IECSManager manager = null)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            return Create<T>(ID, contextHolder, factory, out _, searchImplementorsInChildren, manager);
        }

        static string Path(Transform go)
        {
            string s = go.name;
            while (go.parent != null)
            {
                go = go.parent;
                s  = go.name + "/" + s;
            }

            return s;
        }
    }
}
#endif