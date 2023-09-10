using System;
using Svelto.ECS.GUI.Commands;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.GUI.Extensions.Unity
{
    public abstract class BaseWidgetDescriptorHolder : MonoBehaviour, IWidgetDescriptorHolder, IWidgetIDHolder
    {
        [SerializeField] string _name;

        public uint WidgetID { get; set; }

        public EntityReference entityReference { get; set; }

        public abstract IEntityDescriptor GetDescriptor();

        public T GetImplementor<T>() where T : IImplementor
        {
            return GetComponent<T>();
        }

        // todo: change all usages of this to GetGUIImplementors
        [Obsolete]
        public IImplementor[] GetImplementors() => GetComponents<IImplementor>();

        public IGUIImplementor[] GetGUIImplementors() => GetComponents<IGUIImplementor>();

        public string GetName() => _name;

        public SerializedCommandData[] GetInitEventCommands(WidgetDataSource dataSource)
        {
            var eventComponent = GetComponent<GUIFrameworkEventCommands>();
            if (eventComponent)
            {
                return eventComponent.InitEvent;
            }
            return Array.Empty<SerializedCommandData>();
        }

        public SerializedCommandData[] GetDataEventCommands(WidgetDataSource dataSource)
        {
            var eventComponent = GetComponent<GUIFrameworkEventCommands>();
            if (eventComponent)
            {
                return eventComponent.DataEvent;
            }
            return Array.Empty<SerializedCommandData>();
        }
    }
}