using System;
using System.Collections.Generic;
using Svelto.ECS.GUI.Resources;
using UnityEngine;

namespace Svelto.ECS.GUI.Extensions.Unity
{
    public class UnityGUIBuilder : IGUIBuilder
    {
        public UnityGUIBuilder(Transform root, IGUIPrefabManager prefabManager)
        {
            _root          = root;
            _prefabManager = prefabManager;
        }

        [Obsolete]
        public EntityReference DynamicBuild(WidgetDataSource dataSource)
        {
            var prefabName = dataSource.GetValue<string>(PrefabKey);
            var containerName = dataSource.GetValue<string>(ContainerKey);

            return BuildWidgetEntity(prefabName, containerName, dataSource);
        }

        public WidgetDefinition[] GetStaticWidgets()
        {
            var childList = new List<IWidgetDescriptorHolder>();
            var rootWidgets = _root.GetComponentsInChildren<GUIStaticBuild>(true);
            var defintions = new WidgetDefinition[rootWidgets.Length];
            var dIndex = 0;
            foreach (var rootWidget in rootWidgets)
            {
                var rootWidgetHolder = rootWidget.GetComponent<BaseWidgetDescriptorHolder>();
                GetChildWidgets(rootWidgetHolder, childList);

                // create definition.
                defintions[dIndex++] = new WidgetDefinition
                {
                    root = rootWidgetHolder,
                    childWidgets = childList.ToArray()
                };
            }

            return defintions;
        }

        public void GetChildWidgets(BaseWidgetDescriptorHolder rootWidget, List<IWidgetDescriptorHolder> childWidgets)
        {
            rootWidget.GetComponentsInChildren(true, childWidgets);
            // Prune other static widgets that might be inside of this. We start with index 1 because the first one is
            // always the root widget.
            childWidgets.RemoveAt(0);
            for (var i = 0; i < childWidgets.Count;)
            {
                var iChild = (BaseWidgetDescriptorHolder)childWidgets[i];
                if (iChild.GetComponent<GUIStaticBuild>())
                {
                    // Remove all it's children.
                    for (var j = i + 1; j < childWidgets.Count;)
                    {
                        var jChild = (BaseWidgetDescriptorHolder)childWidgets[j];
                        // Unity get components in children requests are sorted in a hierarchy.
                        if (jChild.transform.IsChildOf(iChild.transform))
                        {
                            childWidgets.RemoveAt(j);
                        }
                        else
                        {
                            // It's safe to break out. All children have been removed.
                            break;
                        }
                    }

                    childWidgets.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        public void CreateInstance(string widgetName, IWidgetDescriptorHolder container, out WidgetDefinition widget,
            out RecyclableGUIComponent recyclable)
        {
            var prefabId = _prefabManager.GetPrefabId(widgetName);
            var widgetGo = _prefabManager.InstantiatePrefab(prefabId, ((BaseWidgetDescriptorHolder)container).transform, out var instance);
            var widgetHolder = widgetGo.GetComponent<BaseWidgetDescriptorHolder>();

            // Get child widgets.
            var childWidgets = new List<IWidgetDescriptorHolder>();
            GetChildWidgets(widgetHolder, childWidgets);

            recyclable = new RecyclableGUIComponent{ instanceId = instance, typeId = prefabId };
            widget = new WidgetDefinition
            {
                root = widgetHolder,
                childWidgets = childWidgets.ToArray()
            };

            // Set game object as active.
            widgetHolder.gameObject.SetActive(true);
        }

        public void FreeInstance(in RecyclableGUIComponent recyclable)
        {
            _prefabManager.Recycle(recyclable.typeId, recyclable.instanceId);
        }

        public void Dispose()
        {
        }

        const string PrefabKey          = "Prefab";
        const string ContainerKey       = "Container";

        readonly IGUIPrefabManager _prefabManager;
        readonly Transform         _root;

        public IGUIWidgetBuildersReader         buildersMap { private get; set; }
        public IGUIWidgetMapWriter               widgetMap   { private get; set; }
        public GUIResources                      resources   { private get; set; }
        public GUIBlackboard                     blackboard  { get;         set; }

        public IGUIBuilder.BuildWidgetEntityFunc BuildWidgetEntity { get; set; }
    }
}