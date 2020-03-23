using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Svelto.ECS.Debugger.Editor.ListViews
{
    public class ListViewNode
    {
        public readonly TreeViewItem Item;
        public bool Active = true;
        public List<ListViewNode> Children;

        public ListViewNode(TreeViewItem item)
        {
            Item = item;
        }

        public void AddChild(ListViewNode child)
        {
            if (Children == null)
                Children = new List<ListViewNode>();
            Children.Add(child);
        }

        public TreeViewItem BuildList()
        {
            if (Active)
            {
                Item.children = null;
                if (Children != null)
                {
                    Item.children = new List<TreeViewItem>();
                    foreach (var child in Children)
                    {
                        var childItem = child.BuildList();
                        if (childItem != null)
                            Item.children.Add(childItem);
                    }
                }
                return Item;

            }
            else
            {
                return null;
            }
        }
    }
}