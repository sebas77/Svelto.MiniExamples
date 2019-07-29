using System.Collections.Generic;
using Svelto.ECS.Debugger.DebugStructure;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Svelto.ECS.Debugger.Editor.ListViews
{
    public class EntityListView : TreeView
    {
        const int kAllEntitiesItemId = 0;
        internal readonly Dictionary<int, DebugEntity> entityById = new Dictionary<int, DebugEntity>();
        
        public static EntityListView CreateGroupListView(GroupSelectionGetter groupSelectionGetter, EntitySelectionCallback entitySelectionCallback)
        {
            var lv = new EntityListView(new TreeViewState(), groupSelectionGetter, entitySelectionCallback);
            return lv;
        }

        ListViewNode rootNode;
        GroupSelectionGetter _groupSelectionGetter;
        EntitySelectionCallback _entitySelectionCallback;
        public EntityListView(TreeViewState state, GroupSelectionGetter groupSelectionGetter, EntitySelectionCallback entitySelectionCallback) : base(state)
        {
            _groupSelectionGetter = groupSelectionGetter;
            _entitySelectionCallback = entitySelectionCallback;
            RebuildNodes();
        }

        void RebuildNodes()
        {
            rootNode = null;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            entityById.Clear();
            var currentId = kAllEntitiesItemId + 1;
            rootNode = new ListViewNode(new TreeViewItem() {id = currentId++, displayName = "Root"});
            if (Application.isPlaying)
            {
                var debugGroup = _groupSelectionGetter();
                //todo
                if (debugGroup != null)
                {
                    var entities = debugGroup.DebugEntities;
                    if (entities.Count > 0)
                        foreach (var debugEntity in entities)
                        {
                            var id = currentId++;
                            entityById.Add(id, debugEntity);
                            var node = new ListViewNode(new TreeViewItem
                                {id = id, displayName = $"Entity UnityId: {unchecked((int)debugEntity.Id)}; Svelto UInt Id:{debugEntity.Id}"});
                            rootNode.AddChild(node);
                        }
                }
            }

            var root = rootNode.BuildList();

            if (!root.hasChildren)
                root.children = new List<TreeViewItem>(0);
            
            root.depth = -1;
            
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
        
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count > 0 && entityById.ContainsKey(selectedIds[0]))
            {
                _entitySelectionCallback(entityById[selectedIds[0]].Id);
            }
            else
            {
                _entitySelectionCallback(null);
                SetSelection(new List<int>());
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }
    }
}