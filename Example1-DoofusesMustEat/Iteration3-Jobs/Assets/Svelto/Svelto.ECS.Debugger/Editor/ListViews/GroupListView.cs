using System.Collections.Generic;
using Svelto.ECS.Debugger.DebugStructure;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Svelto.ECS.Debugger.Editor.ListViews
{
    public class GroupListView : TreeView
    {
        const int kAllEntitiesItemId = 0;
        
        internal static MultiColumnHeaderState GetHeaderState()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Group Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortingArrowAlignment = TextAlignment.Right,
                    canSort = true,
                    sortedAscending = true,
                    width = 100,
                    minWidth = 100,
                    maxWidth = 2000,
                    autoResize = true,
                    allowToggleVisibility = false
                }
            };

            return new MultiColumnHeaderState(columns);
        }
        
        public static GroupListView CreateGroupListView(RootSelectionGetter rootSelectionGetter, GroupSelectionCallback groupSelectionCallback)
        {
            var lv = new GroupListView(new TreeViewState(), new MultiColumnHeader(GetHeaderState()), rootSelectionGetter, groupSelectionCallback);
            return lv;
        }

        ListViewNode rootNode;
        RootSelectionGetter _rootSelectionGetter;
        GroupSelectionCallback _groupSelectionCallback;
        readonly Dictionary<int, DebugGroup> groupsById = new Dictionary<int, DebugGroup>();
        
        public GroupListView(TreeViewState state, MultiColumnHeader multiColumnHeader,
            RootSelectionGetter rootSelectionGetter, GroupSelectionCallback groupSelectionCallback) : base(state, multiColumnHeader)
        {
            _rootSelectionGetter = rootSelectionGetter;
            _groupSelectionCallback = groupSelectionCallback;
            RebuildNodes();
        }

        void RebuildNodes()
        {
            rootNode = null;
            Reload();
        }
        
        protected override TreeViewItem BuildRoot()
        {
            groupsById.Clear();
            var currentId = kAllEntitiesItemId + 1;
            rootNode = new ListViewNode(new TreeViewItem() {id = currentId++, displayName = "Root"});
            if (Application.isPlaying)
            {
                var debugRoot = _rootSelectionGetter();
                //todo
                if (debugRoot != null)
                {
                    var groups = debugRoot.DebugGroups;
                    if (groups.Count > 0)
                        foreach (var debugGroup in groups)
                        {
                            var id = currentId++;
                            groupsById.Add(id, debugGroup);
                            var node = new ListViewNode(new TreeViewItem
                                {id = id, displayName = Debugger.GetNameGroup(debugGroup.Id)});
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
            if (selectedIds.Count > 0 && groupsById.ContainsKey(selectedIds[0]))
            {
                _groupSelectionCallback(groupsById[selectedIds[0]].Id);
            }
            else
            {
                _groupSelectionCallback(null);
                SetSelection(_rootSelectionGetter() == null ? new List<int>() : new List<int> {kAllEntitiesItemId});
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }
    }
}